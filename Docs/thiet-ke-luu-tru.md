# Thiết kế lưu trữ tiến trình chơi

Game: Survival Action kiểu Vampire Survivors (Unity), 2,5D
Liên quan: `kich-ban-man-choi.md` mục 3 (điểm cuối ván), mục 5 (lưu ván); `thiet-ke-shop.md` mục 1, 3, 4
Trạng thái: **Bản đề xuất, chờ nhóm duyệt.** Phần lưu ván thuộc mốc 7, Firebase thuộc mốc 8.

**Nhật ký thay đổi**
- 2026-10-13: bảng xếp hạng tách **theo từng độ khó** (`leaderboards/{difficultyId}/entries/{uid}`) thay cho một bảng chung. Bỏ đề xuất điểm nền theo độ khó. Đã cài đặt: lịch sử 10 ván và bảng xếp hạng (`RunResultService`, `LeaderboardService`). Chuyển vàng tài khoản làm cùng Shop (mốc 7).
- 2026-10-15: vàng tài khoản tính theo điểm cuối ván. Thay `unlockedWeaponIds` bằng map `purchases` (lượt mua của Shop vĩnh viễn, gồm cả mở khóa vũ khí). Ghi kết quả ván và lượt mua **không dùng transaction** nữa để chạy được khi mất mạng (mục 5.4). Luật bảo mật chặn `accountGold` âm.

---

## 1. Tổng quan

Có 3 loại dữ liệu, khác nhau về thời gian sống:

| Dữ liệu | Thời gian sống | Khi nào ghi | Khi nào xóa | Lưu ở (trước mắt → sau) |
|---|---|---|---|---|
| `RunSaveData` | Một ván bị tạm dừng | Bấm "Lưu và thoát" trong Shop | **Ngay khi nạp lại**, hoặc khi bắt đầu ván mới (có xác nhận), hoặc khi bỏ ván | `persistentDataPath/run_save.json` → Firebase (tùy chọn) |
| `AccountSaveData` | Vĩnh viễn | Cuối mỗi ván, khi mở khóa vũ khí, khi đổi cài đặt | Không xóa | `persistentDataPath/account_save.json` → Firebase |
| `RunResult` | Vĩnh viễn | Cuối mỗi ván thắng/thua (không tính bỏ ván) | Lịch sử chỉ giữ 10 ván gần nhất | Nằm trong `AccountSaveData` (lịch sử) và bảng xếp hạng của độ khó tương ứng trên DB (kết quả tốt nhất) |

`RunResult` đã có trong code (`Progression/RunResult.cs`).

---

## 2. Quy tắc chung

1. **Định dạng JSON qua `JsonUtility`** (đã chốt trong `kich-ban-man-choi.md` mục 5.3). Hệ quả:
   - Chỉ dùng class/struct có `[Serializable]`, field `[SerializeField] private` hoặc kiểu cơ bản, mảng và `List<T>`.
   - **Không dùng `Dictionary`** và không dùng kế thừa đa hình. Dữ liệu dạng từ điển lưu thành mảng cặp (`id`, `count`).
2. **Không lưu tham chiếu tới asset.** Mọi `WeaponData`, `PassiveItemData`, `StatCardData`, `EnemyData`, `CharacterData`, `DifficultyData`, `ShopItemData` được lưu bằng **`id` (string)**. Khi nạp, tra `id` → asset qua `GameDatabase` (mục 6).
3. **Có trường `version`** ở gốc mỗi file. Khi cấu trúc thay đổi, tăng `version` và viết hàm chuyển đổi. File có `version` lạ thì bỏ qua và báo lỗi, không làm crash game.
4. **Ghi an toàn:** ghi ra file tạm `*.tmp` rồi đổi tên đè lên file thật, để tránh hỏng file nếu game tắt giữa chừng.
5. **Tách lớp lưu trữ:** code gameplay chỉ làm việc với các class dữ liệu. Việc đọc/ghi do `SaveService` đảm nhiệm. Khi chuyển sang Firebase chỉ thay `SaveService`, cấu trúc dữ liệu giữ nguyên.

---

## 3. `RunSaveData`: bản lưu giữa ván

Chỉ lưu được ở Shop (phút 10 và 20). Lúc đó trận đang đóng băng, không có đòn đánh đang diễn ra, không có lượt lên cấp đang chờ (lên cấp làm đồng hồ dừng nên Shop không thể mở trong lúc chọn thẻ).

### 3.1. Cấu trúc

```
RunSaveData
├── version                     int
├── savedAtUtc                  string (ISO 8601)
├── run                         RunStateData
├── player                      PlayerSaveData
├── inventory                   InventorySaveData
├── skill                       SkillSaveData
├── progression                 ProgressionSaveData
├── score                       ScoreSaveData
├── shop                        ShopSaveData
├── enemies                     EnemySaveData[]
└── pickups                     PickupSaveData[]
```

### 3.2. Chi tiết từng phần

**`RunStateData`**
| Trường | Kiểu | Ghi chú |
|---|---|---|
| `runTime` | float | Giây đã chơi (`GameManager.RunTime`) |
| `difficultyId` | string | `DifficultyData.Id` |
| `characterId` | string | `CharacterData.Id` |
| `nextShopIndex` | int | Shop tiếp theo trong `RunScript.shopTimes` (0, 1, 2) |
| `nextSpawnEventIndex` | int | Sự kiện spawn tiếp theo trong `RunScript`, để không chạy lại sự kiện đã qua |
| `werebearSpawnCount` | int | Số lần Werebear đã xuất hiện |

**`PlayerSaveData`**
| Trường | Kiểu | Ghi chú |
|---|---|---|
| `positionX`, `positionY` | float | |
| `currentHP` | float | |
| `stats` | `PlayerStats` | **Lưu nguyên 15 chỉ số đã cộng hết nâng cấp.** Class đã có `[Serializable]` |

**`InventorySaveData`**
| Trường | Kiểu | Ghi chú |
|---|---|---|
| `weapons` | `OwnedItemData[]` | Mỗi phần tử: `id`, `level`, `evolved` (bool). Phần tử đầu là vũ khí khởi đầu |
| `passives` | `OwnedItemData[]` | Mỗi phần tử: `id`, `level` |
| `statCardPicks` | `IdCountData[]` | Số lần đã chọn mỗi thẻ chỉ số, cần cho giới hạn `maxPicks` |

**`SkillSaveData`**
| Trường | Kiểu |
|---|---|
| `level` | int |
| `evolved` | bool |
| `cooldownRemaining` | float |

**`ProgressionSaveData`**
| Trường | Kiểu | Ghi chú |
|---|---|---|
| `level` | int | |
| `currentXP` | float | |
| `gold` | int | Vàng trong ván (`runGold`) |

**`ScoreSaveData`**
| Trường | Kiểu | Ghi chú |
|---|---|---|
| `kills` | int | Từ `RunScoreTracker` |
| `killedXP` | int | Cần để điểm cuối ván tính đúng sau khi nạp |

**`ShopSaveData`**
| Trường | Kiểu | Ghi chú |
|---|---|---|
| `limitedPurchases` | `IdCountData[]` | Số lần đã mua Sách tri thức, Túi tham lam, Ngọc hồi sinh (`thiet-ke-shop.md` mục 8) |
| `stock` | `ShopSlotSaveData[]` | 5 ô hàng đang bày. Mỗi ô: `itemId`, `targetId` (vũ khí/vật phẩm được nâng cấp, nếu có), `price`, `sold` |
| `rerollCost` | int | Giá làm mới hiện tại |

Phải lưu cả ô hàng và giá làm mới. Nếu không, người chơi có thể "Lưu và thoát" rồi nạp lại để được bày hàng mới mà không tốn tiền làm mới.

**`EnemySaveData`** (tối đa ~260 phần tử)
| Trường | Kiểu |
|---|---|
| `enemyId` | string |
| `positionX`, `positionY` | float |
| `currentHP` | float |

**`PickupSaveData`** (gem, coin, rương)
| Trường | Kiểu | Ghi chú |
|---|---|---|
| `kind` | int | `PickupKind`: Gem, Coin, Chest |
| `positionX`, `positionY` | float | |
| `value` | int | XP của gem, lượng vàng của coin, 0 với rương |

**Kiểu dùng chung**
- `OwnedItemData`: `id` (string), `level` (int), `evolved` (bool)
- `IdCountData`: `id` (string), `count` (int)
- `ShopSlotSaveData`: `itemId` (string), `targetId` (string, rỗng nếu không có), `price` (int), `sold` (bool)

### 3.3. Không lưu (đã chốt trong `kich-ban-man-choi.md` mục 5.2)
- Đạn đang bay của cả Player và quái.
- Trạng thái AI của quái: khi nạp, mọi quái bắt đầu lại từ `Chase` (bỏ qua `Spawning`).
- Quái đang chết (state `Dead`): bỏ qua, coi như đã chết.
- Hiệu ứng hình ảnh, vệt chém, vùng gây sát thương đang tồn tại.

### 3.4. Ví dụ JSON (rút gọn)

```json
{
  "version": 1,
  "savedAtUtc": "2026-10-03T14:20:05Z",
  "run": { "runTime": 600.0, "difficultyId": "Normal", "characterId": "Swordsman",
           "nextShopIndex": 1, "nextSpawnEventIndex": 6, "werebearSpawnCount": 0 },
  "player": { "positionX": 12.4, "positionY": -3.1, "currentHP": 87.0,
              "stats": { "maxHP": 130.0, "armor": 2.0, "might": 1.25, "...": "..." } },
  "inventory": {
    "weapons":  [ { "id": "SwordSlash", "level": 4, "evolved": false },
                  { "id": "ThrowingDagger", "level": 2, "evolved": false } ],
    "passives": [ { "id": "WarriorGauntlet", "level": 3, "evolved": false } ],
    "statCardPicks": [ { "id": "StatCard_Might", "count": 2 } ]
  },
  "skill": { "level": 3, "evolved": false, "cooldownRemaining": 2.5 },
  "progression": { "level": 18, "currentXP": 42.0, "gold": 74 },
  "score": { "kills": 812, "killedXP": 1130 },
  "shop": { "limitedPurchases": [ { "id": "ShopItem_Tome", "count": 1 } ],
            "stock": [ { "itemId": "ShopItem_WeaponLevel", "targetId": "ThrowingDagger", "price": 40, "sold": false } ],
            "rerollCost": 10 },
  "enemies": [ { "enemyId": "Skeleton", "positionX": 20.1, "positionY": 4.0, "currentHP": 18.0 } ],
  "pickups": [ { "kind": 0, "positionX": 13.0, "positionY": -2.0, "value": 3 } ]
}
```

Kích thước ước tính: ~260 quái + vài trăm vật phẩm, khoảng 40–80 KB.

### 3.5. Luồng lưu và nạp

**Lưu (trong Shop → "Lưu và thoát"):**
1. `SaveService` gọi từng hệ thống tạo phần dữ liệu của mình: `GameManager`, `Player`, `PlayerInventory`, `PlayerProgression`, `RunScoreTracker`, `EnemyManager`, `PickupManager`, `ShopManager`, `SpawnManager`.
2. Ghép thành `RunSaveData`, ghi JSON (qua file tạm).
3. Chuyển về Main Menu. Main Menu thấy có file thì hiện nút **Tiếp tục ván**.

**Nạp (Main Menu → "Tiếp tục ván"):**
1. Đọc và kiểm tra `version`. Lỗi thì xóa file, báo người chơi, ở lại Main Menu.
2. **Xóa file lưu ngay** (chống nạp lại nhiều lần).
3. Ghi `characterId`, `difficultyId` vào `RunSelection`, tải scene ván chơi.
4. Từng hệ thống khôi phục phần của mình. Quái và vật phẩm lấy từ pool.
5. Mở thẳng Shop ở đúng mốc đã lưu (trận vẫn đóng băng), **dùng lại ô hàng và giá làm mới đã lưu**, không bày hàng mới.

### 3.6. Điểm cần lưu ý khi cài đặt (tránh lỗi cộng chỉ số hai lần)

- **Chỉ số nhân vật được khôi phục bằng cách gán nguyên `stats` đã lưu.** Khi thêm lại vũ khí và vật phẩm, **không được** gọi lại `ApplyStatModifier`, nếu không chỉ số sẽ bị cộng thêm lần nữa. `PlayerInventory` cần hàm khôi phục riêng (`RestoreWeapon(id, level)`, `RestorePassive(id, level)`) không áp dụng chỉ số.
- Quái khôi phục cần hàm riêng (`Enemy.Restore(data, currentHP)`) vào thẳng `Chase`, bỏ qua `Spawning` và không reset máu về tối đa.
- `gemCount` của `PickupManager` phải được tính lại theo số gem nạp vào.
- Đồng hồ ván (`RunTime`) và chỉ số `nextSpawnEventIndex` phải được gán trước khi `SpawnManager` chạy frame đầu tiên.

---

## 4. `AccountSaveData`: dữ liệu tài khoản

```
AccountSaveData
├── version              int
├── accountGold          int
├── purchases            map<string, int>
├── settings             SettingsSaveData
└── recentRuns           RunResult[]   (10 ván gần nhất, mới nhất đứng đầu)
```

| Trường | Ghi chú |
|---|---|
| `accountGold` | Vàng tài khoản (`thiet-ke-shop.md` mục 1) |
| `purchases` | Map `id món → số lần đã mua` của Shop vĩnh viễn (`thiet-ke-shop.md` mục 6), gồm mở khóa vũ khí, vật phẩm mang sẵn, chỉ số vĩnh viễn. Thiếu ô này = chưa mua gì |
| `settings` | Âm lượng nhạc/hiệu ứng, bật/tắt thanh máu (đang có trong `UI_Option`). Người 3 bổ sung khi làm phần Option |
| `recentRuns` | **Lịch sử 10 ván gần nhất** (đã chốt), không phải 10 ván điểm cao nhất. Bảng xếp hạng nằm riêng trên DB (mục 5) |

**Cập nhật cuối ván** (khi `RunScoreTracker` phát `OnRunFinished`):
1. Cộng `floor(điểm / scorePerAccountGold)` vào `accountGold`. Bỏ ván không cộng.
2. Thêm `RunResult` vào đầu `recentRuns`, cắt còn 10 phần tử.
3. Cập nhật bảng xếp hạng nếu điểm cao hơn điểm tốt nhất cũ (mục 5.4).
4. Ghi file cục bộ, rồi đồng bộ lên DB.

---

## 5. Thiết kế cơ sở dữ liệu (Firebase, mốc 8)

### 5.1. Lựa chọn

| Thành phần | Chọn | Lý do |
|---|---|---|
| Cơ sở dữ liệu | **Cloud Firestore** | Dữ liệu dạng document khớp với các class lưu trữ; hỗ trợ truy vấn sắp xếp và đếm cho bảng xếp hạng; có luật bảo mật theo người dùng |
| Xác thực | **Firebase Authentication**: màn đăng nhập (scene `Login`) cho chọn **email + mật khẩu** (đăng ký, đăng nhập, quên mật khẩu) hoặc **chơi khách** (đăng nhập ẩn danh + tên hiển thị). Khách có thể **liên kết email** sau, giữ nguyên `uid` và dữ liệu | Không bắt buộc tạo tài khoản; vẫn giữ được dữ liệu khi đổi máy nếu dùng email. Cài đặt: `huong-dan-dang-nhap.md` |
| Định danh | `uid` của Firebase Auth | Mọi document của người chơi dùng `uid` làm khóa |

### 5.2. Cấu trúc collection

```
users (collection)
└── {uid} (document)                      ← AccountSaveData + hồ sơ
    └── runSave (subcollection)
        └── current (document)            ← RunSaveData, chỉ tồn tại khi có ván đang lưu

leaderboards (collection)
└── {difficultyId} (document, không cần tồn tại)   ← easy / normal / hard
    └── entries (subcollection)
        └── {uid} (document)              ← kết quả tốt nhất của người chơi ở độ khó này
```

Quan hệ:
- Một `users/{uid}` có **0 hoặc 1** `runSave/current` (1 ô lưu duy nhất).
- Một `users/{uid}` có **0 hoặc 1** dòng ở mỗi độ khó, `leaderboards/{difficultyId}/entries/{uid}` (chỉ giữ kết quả tốt nhất).
- `recentRuns` (10 ván) **nhúng trong** `users/{uid}`, không tách collection, vì luôn đọc cùng hồ sơ và có kích thước nhỏ.

### 5.3. Các document

**`users/{uid}`**
| Field | Kiểu Firestore | Ghi chú |
|---|---|---|
| `displayName` | string | Tên hiển thị trên bảng xếp hạng |
| `createdAt` | timestamp | Server timestamp |
| `updatedAt` | timestamp | Server timestamp, dùng khi so sánh với bản cục bộ |
| `version` | number | Phiên bản cấu trúc |
| `accountGold` | number (int) | Chỉ thay đổi bằng `FieldValue.Increment` (cộng cuối ván, trừ khi mua). Luật bảo mật chặn số âm |
| `purchases` | map | `{ "perm_might": 3, "unlock_boomerang": 1 }`. Tăng bằng `FieldValue.Increment(1)` |
| `settings` | map | Các trường do Người 3 bổ sung |
| `recentRuns` | array of map | Mỗi phần tử là một `RunResult`, tối đa 10, mới nhất đứng đầu |

**`users/{uid}/runSave/current`**
| Field | Kiểu Firestore | Ghi chú |
|---|---|---|
| `version` | number | |
| `savedAt` | timestamp | |
| `characterId`, `difficultyId` | string | Để Main Menu hiện thông tin ván đang lưu mà không cần giải mã cả bản lưu |
| `runTime` | number | Như trên |
| `json` | string | **Toàn bộ `RunSaveData` dạng JSON** (`JsonUtility`), khoảng 40–80 KB, dưới giới hạn 1 MiB của một document |

Lưu bản lưu ván thành một chuỗi JSON thay vì hàng trăm field lồng nhau. Không bao giờ cần truy vấn bên trong bản lưu, và cách này dùng chung được code với bản lưu cục bộ.

**`leaderboards/{difficultyId}/entries/{uid}`** (id document chính là `uid`)
| Field | Kiểu Firestore | Ghi chú |
|---|---|---|
| `displayName` | string | Sao chép từ `users/{uid}` để không phải đọc thêm khi hiện bảng |
| `score` | number (int) | Khóa sắp xếp (mục 5.5) |
| `characterId` | string | |
| `runTime` | number | |
| `kills` | number | |
| `level` | number | |
| `victory` | boolean | |
| `achievedAt` | timestamp | |

### 5.4. Các thao tác chính

| Thao tác | Cách làm |
|---|---|
| Mở game | Đăng nhập ẩn danh (nếu chưa), đọc `users/{uid}`. So `updatedAt` với bản cục bộ, lấy bản mới hơn |
| Lưu và thoát | Ghi `users/{uid}/runSave/current` |
| Tiếp tục ván | Đọc `runSave/current`, **xóa document ngay trong cùng thao tác** rồi mới dựng lại ván. Luật bảo mật bảo đảm chỉ chủ tài khoản đọc/xóa được |
| Kết thúc ván | `RunResultService.SubmitAsync`: đọc `users/{uid}` và dòng bảng xếp hạng (từ server, mất mạng thì từ bản đệm). Ghi `users/{uid}` (`recentRuns` thêm vào đầu và cắt còn 10, `accountGold` += vàng của ván) và, nếu điểm cao hơn, ghi đè `leaderboards/{difficultyId}/entries/{uid}`. Hai lệnh ghi **tách riêng** để nếu dòng bảng xếp hạng bị luật từ chối thì vàng vẫn được cộng. Chờ tối đa 5 giây; quá hạn coi là offline, ghi vẫn nằm chờ và tự đồng bộ khi có mạng |
| Mở Shop vĩnh viễn | Đọc `users/{uid}` (`accountGold`, `purchases`), mất mạng thì đọc bản đệm |
| Mua ở Shop vĩnh viễn | Một lệnh `UpdateAsync`: `accountGold` Increment(−giá), `purchases.{id}` Increment(1). Chạy được khi mất mạng. Nếu luật từ chối (số dư âm do mua trên hai máy) thì cả lệnh bị hủy |
| Hiện bảng xếp hạng | `entries` của độ khó đang xem, sắp `score` giảm dần, lấy 50 dòng đầu. Chỉ cần index một trường, Firestore tự tạo |
| Hạng của người chơi (ô "Rank" trên màn GameOver) | Truy vấn đếm (`count()`) trong độ khó vừa chơi số document có `score` lớn hơn điểm **ván này**, hạng = kết quả + 1 |

### 5.5. Bảng xếp hạng theo từng độ khó

Đã chốt (2026-10-13): **mỗi độ khó một bảng riêng.** Ván chỉ so điểm với các ván cùng độ khó, nên không cần điểm nền hay sắp theo bậc độ khó. Công thức điểm giữ nguyên (`kich-ban-man-choi.md` mục 3).

- Màn GameOver hiện hạng trong độ khó vừa chơi.
- Main Menu có 3 tab Easy / Normal / Hard, mặc định mở Normal.
- Mỗi độ khó là một collection riêng nên chỉ cần index một trường `score`, không cần composite index.

### 5.6. Luật bảo mật (phác thảo)

```
match /users/{uid} {
  allow read, delete: if request.auth != null && request.auth.uid == uid;
  allow create, update: if request.auth != null && request.auth.uid == uid
    && (!('accountGold' in request.resource.data)
        || (request.resource.data.accountGold is int && request.resource.data.accountGold >= 0));
  match /runSave/{doc} {
    allow read, write, delete: if request.auth != null && request.auth.uid == uid;
  }
}
match /leaderboards/{difficultyId}/entries/{uid} {
  allow read: if true;
  allow create, update: if request.auth != null && request.auth.uid == uid
    && request.resource.data.score is int
    && request.resource.data.score >= 0
    && (resource == null || request.resource.data.score > resource.data.score);
}
```

Điểm do máy người chơi tính, nên vẫn có thể bị sửa. Chống gian lận thật sự cần kiểm tra trên server (Cloud Functions), nằm ngoài phạm vi đồ án.

### 5.7. Rủi ro cần kiểm tra sớm

- **Nền tảng phát hành:** Firebase Unity SDK hỗ trợ chính thức Android và iOS. Bản cho Windows/macOS chỉ dành cho phát triển. Nếu game phát hành trên PC, cần thử sớm SDK trên bản build Windows, hoặc chuyển sang gọi **REST API** của Firestore và Firebase Auth bằng `UnityWebRequest`.
- **Mất mạng:** không dùng transaction cho kết quả ván và lượt mua, vì transaction cần server. Lệnh ghi thường được Firestore giữ trong bản đệm và tự gửi khi có mạng. **Cần kiểm tra trên bản build Windows** rằng bản đệm còn sau khi tắt game. Hạng trên màn GameOver vẫn cần mạng. Riêng "Tiếp tục ván" từ DB cần có mạng.

---

## 6. `GameDatabase`: tra cứu id → asset

ScriptableObject chứa danh sách mọi asset dữ liệu, dùng khi nạp để đổi `id` thành asset:

```
GameDatabase
├── characters    CharacterData[]
├── difficulties  DifficultyData[]
├── weapons       WeaponData[]
├── passives      PassiveItemData[]
├── statCards     StatCardData[]
├── enemies       EnemyData[]
└── shopItems     ShopItemData[]
```

- Khi khởi động, dựng các bảng tra `id → asset` một lần (không dùng trong vòng lặp nóng).
- Thêm kiểm tra trong Editor: báo lỗi nếu hai asset trùng `id` hoặc có asset để trống `id`.

---

## 7. Đã chốt

- Có lưu lên cơ sở dữ liệu (Firebase).
- Bảng xếp hạng tách theo độ khó, mỗi người giữ kết quả tốt nhất ở mỗi độ khó.
- Nền tảng phát hành: PC Windows (cần build thử Firebase sớm, mục 5.7).
- Lịch sử giữ 10 ván gần nhất.

## 8. Cần nhóm chốt

1. Cách đăng nhập: ẩn danh + tên hiển thị (đề xuất), hay bắt buộc email?
2. Người 3 bổ sung các trường của `settings`.
