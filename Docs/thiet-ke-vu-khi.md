# Thiết kế vũ khí & vật phẩm bị động

Game: Survival Action kiểu Vampire Survivors (Unity), 2,5D
Liên quan: `thiet-ke-nhan-vat.md` (chỉ số, loại sát thương, aimMode), `thiet-ke-quai.md` (kháng), `kich-ban-man-choi.md`
Trạng thái: **v1 đã chốt cấu trúc**. Các con số là bản nháp cân bằng, chỉnh sau khi test.

Quy ước: tên class, hàm, biến trong code viết bằng tiếng Anh.

---

## 1. Quy tắc chung

| Quy tắc | Nội dung |
|---|---|
| Ô trang bị | Tối đa **4 vũ khí** (1 khởi đầu + 3 nhận thêm) và **4 vật phẩm bị động** |
| Cấp | Mọi vũ khí và vật phẩm có **5 cấp** (trừ Bùa song ảnh: 2 cấp) |
| Tiến hóa | Vũ khí **nhận thêm** đạt cấp 5 + sở hữu **vật phẩm bị động tương ứng** (cấp bất kỳ) → thẻ tiến hóa xuất hiện |
| Vũ khí khởi đầu | Lên được cấp 5, **không tiến hóa** |
| Nguồn nhận | Thẻ khi lên cấp, rương của Elite / mini-boss. **Mỗi lớp chỉ nhận vũ khí và vật phẩm trong danh sách của lớp đó** (xem bảng dưới) |
| Mở khóa | Tài khoản mới có sẵn Dao găm, Kiếm bay, Tia sét, Băng trùy. Boomerang, Chông sắt, Quả cầu phép, Bình độc mở bằng vàng tài khoản. Vũ khí chưa mở khóa **không xuất hiện** ở bất kỳ đâu (xem `thiet-ke-shop.md`) |
| Hình ảnh | Vũ khí nhận thêm là **vật thể hiệu ứng riêng**, tự thi triển, không phát animation của nhân vật (`playOwnerAttackAnim = false`) |
| Chỉ số áp dụng | Sát thương × `Might`, nhịp ÷ `AttackSpeed`, kích thước × `Area`, số đạn + `Multishot`, tốc độ đạn × `ProjectileSpeed`, thời gian tồn tại × `Duration` |

### Vũ khí và vật phẩm theo lớp

Mỗi lớp có danh sách riêng (`CharacterData.weaponPool`, `CharacterData.passivePool`) để không bốc ra vũ khí khó dùng với lối chơi của lớp. Thẻ lên cấp và rương đều lấy từ danh sách này. Thẻ chỉ số dùng chung cho cả 4 lớp.

| Vũ khí (vật phẩm tiến hóa) | Swordsman | Archer | Mage | Guardian |
|---|:---:|:---:|:---:|:---:|
| Dao găm (Bao tay thiện xạ) | ✅ | ✅ | | |
| Boomerang (Lông vũ) | ✅ | ✅ | | |
| Kiếm bay (Găng tay chiến binh) | ✅ | ✅ | | |
| Chông sắt (Giáp sắt) | ✅ | | | ✅ |
| Quả cầu phép (Đồng hồ cát) | | | ✅ | ✅ |
| Tia sét (Tinh thể khuếch đại) | | | ✅ | ✅ |
| Bình độc (Nam châm) | | | ✅ | ✅ |
| Băng trùy (Bùa song ảnh) | | ✅ | ✅ | |

- **Vật phẩm bị động** của mỗi lớp = 4 vật phẩm tiến hóa ứng với 4 vũ khí của lớp đó + 3 vật phẩm hỗ trợ dùng chung (Tim khổng lồ, Nhẫn hồi phục, Giày gió).
- **Vũ khí khởi đầu** không nằm trong danh sách này: mỗi lớp chỉ có vũ khí khởi đầu của mình, vì nó gắn với animation Attack của nhân vật.

### Chỉ số đặc trưng của lớp áp dụng lên vũ khí

| Lớp | Áp dụng cho |
|---|---|
| Kiếm sĩ: chí mạng | **Mọi** vũ khí |
| Cung thủ: xuyên thấu | Mọi vũ khí **dạng đạn** (Cung gỗ, Tia phép, Dao găm, Kiếm bay, Băng trùy) |
| Pháp sư: cộng hưởng phép | Chỉ vũ khí **phép** (Tia phép, Quả cầu phép, Tia sét, Bình độc, Băng trùy) |
| Đỡ đòn: phản sát thương | Không liên quan vũ khí; hợp với vũ khí tầm gần (Chông sắt, Quả cầu phép) |

### Kiểu thi triển (`aimMode`)

| Giá trị | Mô tả | Vũ khí |
|---|---|---|
| `Cursor` | Theo hướng chuột | Vũ khí khởi đầu, Dao găm, Boomerang |
| `NearestEnemy` | Quái gần nhất | Kiếm bay, Băng trùy |
| `RandomEnemy` | Quái ngẫu nhiên trong phạm vi | Tia sét, Bình độc |
| `Orbit` | Xoay quanh người | Quả cầu phép |
| `Trail` | Rải tại vị trí người chơi khi di chuyển | Chông sắt |
| `Omni` | Tỏa mọi hướng | Bản tiến hóa Bão tuyết |

---

## 2. Vũ khí khởi đầu (5 cấp, không tiến hóa)

| Cấp | Chung cho cả 4 | Riêng từng vũ khí |
|---|---|---|
| 2 | +25% sát thương | |
| 3 | +20% kích thước | |
| 4 | | Kiếm chém: vòng cung 120° → **180°** · Cung gỗ: **+1 mũi tên** · Tia phép: **+1 tia** · Trọng kích: đẩy lùi **+50%** |
| 5 | +25% sát thương, nhịp −10% | |

---

## 3. Tám vũ khí nhận thêm

### 3.1. Tổng quan

| Vũ khí | Class | Kiểu | Loại | Sát thương | Nhịp | Tiến hóa với |
|---|---|---|---|---|---|---|
| Dao găm | `ThrowingDagger` | `Cursor` | Vật lý | 6 | 0,8 giây | Bao tay thiện xạ |
| Boomerang | `Boomerang` | `Cursor` | Vật lý | 10 | 2,0 giây | Lông vũ |
| Kiếm bay | `SpiritSword` | `NearestEnemy` | Vật lý | 12 | 1,5 giây | Găng tay chiến binh |
| Chông sắt | `Caltrops` | `Trail` | Vật lý | 8 / 0,5 giây | 1,0 giây | Giáp sắt |
| Quả cầu phép | `OrbitingOrbs` | `Orbit` | Phép | 8 | Bật 3 giây / tắt 3 giây | Đồng hồ cát |
| Tia sét | `LightningStrike` | `RandomEnemy` | Phép | 15 | 2,0 giây | Tinh thể khuếch đại |
| Bình độc | `PoisonFlask` | `RandomEnemy` | Phép | 5 / 0,5 giây | 3,0 giây | Nam châm |
| Băng trùy | `IceShard` | `NearestEnemy` | Phép | 8 | 1,2 giây | Bùa song ảnh |

### 3.2. Dao găm (`ThrowingDagger`)

Phóng dao nhỏ bay nhanh (14 đơn vị/giây) theo hướng chuột. Nhiều dao thì bắn liên tiếp cách nhau 0,05 giây.

| Cấp | Thay đổi |
|---|---|
| 1 | 1 dao, 6 sát thương, nhịp 0,8 giây |
| 2 | +1 dao |
| 3 | +3 sát thương |
| 4 | +1 dao |
| 5 | +3 sát thương, nhịp −20% |

**Tiến hóa, "Vũ điệu ngàn dao"** (+ Bao tay thiện xạ): bắn dao **liên tục** mỗi 0,15 giây, mỗi dao xuyên 2 quái.

### 3.3. Boomerang (`Boomerang`)

Ném theo hướng chuột, bay tối đa 5 đơn vị rồi quay về người chơi. Xuyên mọi quái, trúng ở **cả lượt đi và lượt về**.

| Cấp | Thay đổi |
|---|---|
| 1 | 1 boomerang, 10 sát thương, nhịp 2,0 giây |
| 2 | +5 sát thương |
| 3 | +25% kích thước |
| 4 | +1 boomerang (xòe 20°) |
| 5 | +5 sát thương, tầm +20% |

**Tiến hóa, "Nguyệt luân"** (+ Lông vũ): 3 boomerang to gấp đôi, đẩy lùi quái.

### 3.4. Kiếm bay (`SpiritSword`)

Kiếm tự bay đuổi theo quái gần nhất (bám mục tiêu), trúng rồi biến mất.

| Cấp | Thay đổi |
|---|---|
| 1 | 1 kiếm, 12 sát thương, nhịp 1,5 giây |
| 2 | +1 kiếm |
| 3 | +5 sát thương |
| 4 | +1 kiếm |
| 5 | +5 sát thương, xuyên +1 |

**Tiến hóa, "Kiếm hồn"** (+ Găng tay chiến binh): 5 kiếm **xoay quanh người**, liên tục lao vào quái gần nhất rồi quay về, không biến mất.

### 3.5. Chông sắt (`Caltrops`)

Mỗi nhịp rải 1 chông tại vị trí người chơi. Chông tồn tại 4 giây, gây sát thương cho quái chạm vào (mỗi con tối đa 1 lần mỗi 0,5 giây). Hiệu quả khi chạy vòng để quái đuổi qua bãi chông.

| Cấp | Thay đổi |
|---|---|
| 1 | 1 chông, 8 sát thương, tồn tại 4 giây, bán kính 0,6 |
| 2 | +2 sát thương |
| 3 | Tồn tại +1 giây |
| 4 | +25% kích thước |
| 5 | Rải 2 chông mỗi nhịp, +2 sát thương |

**Tiến hóa, "Bãi chông gai"** (+ Giáp sắt): chông to gấp đôi, tồn tại 8 giây, **làm chậm 40%**.

### 3.6. Quả cầu phép (`OrbitingOrbs`)

Các quả cầu phép xoay quanh người (bán kính 1,8, 180°/giây), bật 3 giây rồi tắt 3 giây. Mỗi quái bị một quả cầu đánh tối đa 1 lần mỗi 0,5 giây.

| Cấp | Thay đổi |
|---|---|
| 1 | 2 quả cầu, 8 sát thương |
| 2 | +1 quả cầu |
| 3 | +3 sát thương |
| 4 | Bán kính quỹ đạo và kích thước +25%, thời gian bật +1 giây |
| 5 | +1 quả cầu, +3 sát thương |

**Tiến hóa, "Tinh cầu vĩnh cửu"** (+ Đồng hồ cát): 6 quả cầu, **không bao giờ tắt**.

### 3.7. Tia sét (`LightningStrike`)

Sét đánh từ trên xuống vào quái ngẫu nhiên đang có trên màn hình, gây sát thương vùng nhỏ (bán kính 0,8).

| Cấp | Thay đổi |
|---|---|
| 1 | 2 tia, 15 sát thương, nhịp 2,0 giây |
| 2 | +1 tia |
| 3 | +5 sát thương |
| 4 | Bán kính +40% |
| 5 | +2 tia |

**Tiến hóa, "Bão sấm"** (+ Tinh thể khuếch đại): mỗi tia **nảy sang 3 quái** gần đó.

### 3.8. Bình độc (`PoisonFlask`)

Ném bình vào quái ngẫu nhiên trong phạm vi 6 đơn vị. Bình vỡ để lại vũng độc (bán kính 1,5) tồn tại 3 giây, gây sát thương theo thời gian.

| Cấp | Thay đổi |
|---|---|
| 1 | 1 bình, 5 sát thương mỗi 0,5 giây, nhịp 3,0 giây |
| 2 | +1 bình |
| 3 | +2 sát thương |
| 4 | Bán kính +25%, tồn tại +1 giây |
| 5 | +1 bình, +2 sát thương |

**Tiến hóa, "Đầm lầy chết chóc"** (+ Nam châm): vũng to gấp đôi và **hút quái về tâm**.

### 3.9. Băng trùy (`IceShard`)

Bắn mũi băng vào quái gần nhất, trúng thì làm chậm 30% trong 1,5 giây.

| Cấp | Thay đổi |
|---|---|
| 1 | 1 trùy, 8 sát thương, nhịp 1,2 giây |
| 2 | +1 trùy |
| 3 | +3 sát thương |
| 4 | Làm chậm 40%, +1 trùy |
| 5 | +3 sát thương, nhịp −15% |

**Tiến hóa, "Bão tuyết"** (+ Bùa song ảnh): mỗi nhịp bắn **8 trùy tỏa tròn 360°**, trúng thì **đóng băng 1 giây**.

---

### 3.10. Ghi chú cài đặt tiến hóa

- Mỗi bản tiến hóa là một `WeaponData` riêng chỉ có **1 cấp**. Vũ khí gốc trỏ tới nó qua `evolution` và `evolutionItem`.
- Điều kiện: vũ khí gốc **cấp 5** + **có vật phẩm tiến hóa** (cấp bất kỳ). Thẻ tiến hóa luôn chiếm 1 trong 3 ô; nếu nhiều vũ khí đủ điều kiện cùng lúc, chọn ngẫu nhiên 1.
- Tiến hóa thay vũ khí ở đúng ô đó. Vũ khí gốc không xuất hiện lại trong thẻ "vũ khí mới".
- Con số đã chọn khi cài đặt (tài liệu chưa có):
  - **Vũ điệu ngàn dao:** 12 sát thương mỗi dao, mỗi 0,15 giây, xuyên 2.
  - **Nguyệt luân:** 3 boomerang, kích thước 2,5, đẩy lùi 1, 20 sát thương.
  - **Kiếm hồn:** 5 kiếm xoay bán kính 1,4; mỗi kiếm lao vào quái gần nhất trong tầm 6 với tốc độ 14, hồi 1 giây giữa hai lần lao, 22 sát thương.
  - **Bãi chông gai:** bán kính 1,5, tồn tại 8 giây, làm chậm 40%, 2 chông mỗi nhịp, 12 sát thương.
  - **Tinh cầu vĩnh cửu:** 6 quả cầu, 14 sát thương, không tắt.
  - **Bão sấm:** 5 tia, mỗi tia nảy tiếp 3 lần sang quái gần nhất trong 3 đơn vị chưa bị đánh, 20 sát thương.
  - **Đầm lầy chết chóc:** bán kính 3,75, tồn tại 4 giây, hút quái về tâm với tốc độ 2,5.
  - **Bão tuyết:** 8 trùy tỏa đều 360° (xoay theo hướng chuột), 14 sát thương. "Đóng băng" cài bằng **làm chậm 100% trong 1 giây**: quái đứng im nhưng vẫn đánh nếu Player ở trong tầm.

## 4. Vật phẩm bị động

8 vật phẩm dùng để tiến hóa vũ khí, cộng 4 vật phẩm hỗ trợ. Người chơi mang tối đa 4 cùng lúc.

| Vật phẩm | Class | Chỉ số | Mỗi cấp | Cấp tối đa | Tiến hóa |
|---|---|---|---|---|---|
| Găng tay chiến binh | `WarriorGauntlet` | `Might` | +10% | 5 | Kiếm bay |
| Bao tay thiện xạ | `MarksmanGlove` | `AttackSpeed` | +8% | 5 | Dao găm |
| Tinh thể khuếch đại | `AmplifyCrystal` | `Area` | +10% | 5 | Tia sét |
| Bùa song ảnh | `TwinCharm` | `Multishot` | +1 | **2** | Băng trùy |
| Nam châm | `Magnet` | `Magnet` | +25% | 5 | Bình độc |
| Giáp sắt | `IronArmor` | `Armor` | +1 | 5 | Chông sắt |
| Đồng hồ cát | `Hourglass` | `Duration` | +10% | 5 | Quả cầu phép |
| Lông vũ | `Feather` | `ProjectileSpeed` | +10% | 5 | Boomerang |
| Tim khổng lồ | `GiantHeart` | `MaxHP` | +10% | 5 | — |
| Nhẫn hồi phục | `RegenRing` | `Recovery` | +0,2 HP/giây | 5 | — |
| Giày gió | `WindBoots` | `MoveSpeed` | +8% | 5 | — |

`Growth`, `Greed` và `Revival` không có vật phẩm lên cấp; chỉ tăng qua Shop vĩnh viễn (`thiet-ke-shop.md` mục 3).


---

## 5. Thẻ khi lên cấp

Mỗi lần lên cấp, rút 3 thẻ **không trùng** từ các loại khả dụng:

| Loại thẻ | Điều kiện xuất hiện |
|---|---|
| Vũ khí mới (cấp 1) | Còn ô vũ khí trống |
| Nâng cấp vũ khí | Vũ khí đang có, chưa cấp 5 |
| Vật phẩm mới (cấp 1) | Còn ô vật phẩm trống |
| Nâng cấp vật phẩm | Vật phẩm đang có, chưa cấp tối đa |
| **Cộng thẳng chỉ số** | Luôn khả dụng (mục 5.1), **tối đa 1 trong 3 ô** |
| **Tiến hóa** (vũ khí) | Đủ điều kiện → **luôn chiếm 1 trong 3 ô** |
| Dự phòng: "Hồi 30 HP" hoặc "+20 vàng" | Khi không còn gì để nâng |

### 5.1. Thẻ cộng thẳng chỉ số

Cộng trực tiếp vào chỉ số người chơi. **Không chiếm ô vật phẩm**, **cộng dồn** được, nhưng mỗi thẻ chỉ bằng khoảng **một nửa** một cấp vật phẩm bị động.

| Thẻ | Class | Chỉ số | Mỗi lần chọn | Giới hạn số lần |
|---|---|---|---|---|
| Máu | `StatCard_MaxHP` | `MaxHP` | +10 (và hồi 10 HP) | Không giới hạn |
| Sức mạnh | `StatCard_Might` | `Might` | +5% | 10 |
| Giáp | `StatCard_Armor` | `Armor` | +1 | 5 |
| Tốc độ đánh | `StatCard_AttackSpeed` | `AttackSpeed` | +4% | 10 |
| Tốc độ chạy | `StatCard_MoveSpeed` | `MoveSpeed` | +4% | 5 |
| Vùng ảnh hưởng | `StatCard_Area` | `Area` | +5% | 10 |
| Hồi máu | `StatCard_Recovery` | `Recovery` | +0,1 HP/giây | 10 |
| Bán kính hút | `StatCard_Magnet` | `Magnet` | +10% | 5 |

**Phân biệt với vật phẩm bị động:**

| | Thẻ chỉ số | Vật phẩm bị động |
|---|---|---|
| Chiếm ô | Không | Có (tối đa 4) |
| Giá trị | Nhỏ (khoảng ½ một cấp vật phẩm) | Lớn |
| Mở khóa tiến hóa **vũ khí** | **Không** | Có |

- **Không có thẻ `Multishot`**: +1 đạn cho mọi vũ khí quá mạnh để cộng dồn không giới hạn; chỉ nhận qua Bùa song ảnh.
- **Tối đa 1 thẻ chỉ số trong 3 lựa chọn**: nếu không giới hạn, thẻ chỉ số (luôn khả dụng) sẽ chiếm phần lớn các lượt rút và người chơi hiếm khi gặp vũ khí mới.
- Khi đã đầy ô và nâng hết vũ khí, vật phẩm, thẻ chỉ số trở thành lựa chọn chính ở cuối ván (thay cho thẻ dự phòng).

**Rương** (Elite, mini-boss; cờ `EnemyData.dropsChest`): nhặt vào mở **bảng chọn 1 trong 3**, giống hệt lượt lên cấp (cùng quy tắc rút thẻ, thẻ tiến hóa luôn chiếm 1 ô nếu đủ điều kiện), tiêu đề `TREASURE CHEST!`. Nhiều lượt chờ cùng lúc thì xử lý lên cấp trước, rương sau.

---

## 6. Liên quan đến Shop

Xem `thiet-ke-shop.md`: Shop vĩnh viễn (mở khóa vũ khí, vật phẩm mang sẵn, chỉ số vĩnh viễn). Không có Shop trong ván.

---

## 7. Dữ liệu (ScriptableObject)

**`WeaponData`** (bổ sung vào bản trong `thiet-ke-nhan-vat.md`)
- `id`, `displayName`, `description`, `icon`
- `weaponPrefab` (prefab chứa component lớp con của `Weapon`, gắn vào Player khi trang bị), `effectPrefab` (vật thể hiệu ứng, lấy từ pool mỗi lần đánh)
- `damageType`, `aimMode`, `playOwnerAttackAnim`, `knockback`
- `levels[5]`: sát thương, nhịp, số đạn, kích thước, tầm, thời gian tồn tại, hiệu ứng thêm. Mỗi cấp ghi **giá trị tuyệt đối** (không ghi phần trăm cộng thêm), kèm dòng mô tả hiện trên thẻ lên cấp
- `isStartingWeapon`
- `evolution`: `requiredPassive` (`PassiveItemData`), `evolvedWeapon` (`WeaponData`)

**`PassiveItemData`**
- `id`, `displayName`, `description`, `icon`
- `stat` (chỉ số được cộng), `modifierKind`, `valuePerLevel`, `maxLevel`
- `modifierKind`: `Flat` cộng thẳng (Might +10% ghi `0.1`, Armor +1 ghi `1`), `PercentOfBase` cộng theo phần trăm chỉ số gốc của nhân vật (Tim khổng lồ MaxHP +10% ghi `0.1`, Nam châm +25% ghi `0.25`). `StatCardData` dùng cùng quy ước

**`StatCardData`**
- `id`, `displayName`, `icon`
- `stat`, `value`, `maxPicks` (0 = không giới hạn), `healAmount` (chỉ thẻ Máu)

`UpgradeData` (thẻ lên cấp) có các loại: `NewWeapon`, `WeaponLevel`, `NewPassive`, `PassiveLevel`, `StatCard`, `Evolution`, `Fallback`.

Thêm vũ khí hoặc vật phẩm mới chỉ cần tạo dữ liệu và prefab. Mỗi vũ khí cần một lớp con của `Weapon` vì cách thi triển khác nhau, nhưng các vũ khí cùng `aimMode` dùng chung được phần lớn code.

---

## 8. Asset hiệu ứng (cần kiểm tra khi import)

| Vũ khí | Nguồn đề xuất |
|---|---|
| Dao găm, Kiếm bay | Sprite vũ khí trong [16x16 DungeonTileset II – 0x72](https://0x72.itch.io/dungeontileset-ii) (CC0, có sẵn dao và kiếm rời) |
| Boomerang | [500 Pixel Bullet 24x24 – BDragon1727](https://bdragon1727.itch.io/500-pixel-bullet-24x24), xoay bằng code |
| Chông sắt | Bẫy gai trong 0x72 DungeonTileset II, hoặc tự vẽ 1 sprite 16×16 |
| Quả cầu phép | Quả cầu năng lượng từ [Magic Shader All](https://bdragon1727.itch.io/magic-shader-all) hoặc [500 Pixel Bullet 24x24](https://bdragon1727.itch.io/500-pixel-bullet-24x24) |
| Tia sét, Băng trùy | [1050 RPG Effects 64x64 – BDragon1727](https://bdragon1727.itch.io/1050-rpg-effects-64x64) (hiệu ứng phép nhiều màu) |
| Bình độc | Lọ thuốc trong 0x72 DungeonTileset II + vũng độc từ [Magic Shader All](https://bdragon1727.itch.io/magic-shader-all) (màu xanh lá) |
| Icon vũ khí và vật phẩm (UI) | Chưa chọn, xem cùng UI Pack của Người 3 |

---

## 9. Còn cần chốt

(Chưa có câu hỏi mở.)

### Đã chốt
- Vũ khí xoay quanh người là **Quả cầu phép** (thay cho Sách phép, vì có sẵn asset).
- Giới hạn **4 vũ khí và 4 vật phẩm bị động**.
