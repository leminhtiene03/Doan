# Kịch bản ván chơi & hệ thống Spawn

Game: Survival Action kiểu Vampire Survivors (Unity), 2,5D
Liên quan: `thiet-ke-nhan-vat.md`, `thiet-ke-quai.md`
Trạng thái: **v3**. Cấu trúc: 1 ván 30 phút liên tục, không có Shop trong ván (bỏ ngày 2026-10-16, xem `thiet-ke-shop.md`). Các con số là bản nháp cân bằng.

---

## Thay đổi so với v2

| v2 | v3 |
|---|---|
| Mỗi độ khó gồm 3 màn × 5 phút | **Một ván liên tục 30 phút**, chia 3 giai đoạn × 10 phút |
| Độ khó là chuỗi màn | Độ khó là **tùy chọn của ván** (Dễ / Trung bình / Khó), chỉ đổi hệ số, dùng chung một kịch bản |
| Shop sau mỗi màn | **Không có Shop trong ván.** Chỉ có Shop vĩnh viễn trước khi vào ván |
| Không lưu giữa chừng | **Lưu và thoát** từ menu Pause, bản lưu bị xóa khi nạp lại (đang hoãn) |
| Máu quái tăng theo thời gian và theo màn | **Chỉ số quái cố định**. Độ khó tăng nhờ quái mạnh hơn xuất hiện, trọng số spawn và số lượng |

---

## 0. Cấu trúc một ván

```
Chọn nhân vật → Chọn độ khó → Shop vĩnh viễn → [Giai đoạn 1: 0–10'] → [Giai đoạn 2: 10–20'] → [Giai đoạn 3: 20–30'] → Boss → Kết quả
```

- **Thắng:** hạ Boss cuối (Necromancer) xuất hiện ở phút 30:00.
- **Thua:** HP người chơi về 0 ở bất kỳ thời điểm nào.
- **Cấp độ, vũ khí, nâng cấp và vàng giữ nguyên** suốt ván.
- Coin nhặt được trong ván cộng vào vàng tài khoản khi ván kết thúc (`thiet-ke-shop.md` mục 1).
- Nhịp độ **răng cưa**: áp lực tăng dần, sau mỗi sự kiện lớn có 15–20 giây nhẹ để nhặt XP và lên cấp.

---

## 1. Kịch bản theo thời gian

Tên quái lấy từ `thiet-ke-quai.md`. Số quái tối đa tính ở độ khó Trung bình. **Bản cài đặt (`RunScriptData.asset`) nhân ×1,5 các con số trong bảng** (cả `minAlive`), nên cuối ván Trung bình có 300 con, Khó có 390 con.

### Giai đoạn 1: Xâm lăng (0:00–10:00)

| Thời điểm | Quái / sự kiện | Số quái tối đa |
|---|---|---|
| 0:00 | Slime, nhịp spawn 1,5 giây | 30 |
| 1:30 | Thêm Bat | 30 |
| 3:00 | Thêm Skeleton, nhịp 1,2 giây | 45 |
| 4:00 | **Swarm**: đàn Bat bay ngang màn hình | 45 |
| 5:00 | Thêm Orc | 60 |
| 6:00 | **Ring**: vòng Slime khép quanh người chơi | 60 |
| 7:00 | Thêm Armored Orc, nhịp 1,0 giây | 70 |
| 8:00 | **Elite Orc** đầu tiên | 70 |
| 9:30 | Giảm spawn, khoảng lặng chuyển giai đoạn | 30 |

### Giai đoạn 2: Bộ tộc Orc (10:00–20:00)

| Thời điểm | Quái / sự kiện | Số quái tối đa |
|---|---|---|
| 10:00 | Skeleton, Orc, Armored Orc (Slime giảm dần) | 80 |
| 10:30 | Thêm Skeleton Archer | 80 |
| 12:00 | **Line**: hàng Orc Rider lao ngang màn hình | 90 |
| 13:00 | Elite Orc | 90 |
| 14:30 | Giảm spawn 30 giây | 40 |
| 15:00 | **Mini-boss: Werebear** | 60 |
| 16:00 | **Ring**: vòng Skeleton + Orc | 100 |
| 17:00 | Thêm Exploding Slime | 100 |
| 18:00 | Thêm Armored Skeleton, Elite Orc | 110 |
| 19:00 | **Swarm**: đàn Bat lớn | 110 |
| 19:30 | Giảm spawn, khoảng lặng chuyển giai đoạn | 40 |

### Giai đoạn 3: Đạo quân bất tử (20:00–30:00)

| Thời điểm | Quái / sự kiện | Số quái tối đa |
|---|---|---|
| 20:00 | Skeleton, Armored Skeleton, Skeleton Archer, Orc, Armored Orc | 130 |
| 20:30 | Thêm Werewolf | 130 |
| 22:00 | **Greatsword Skeleton** (Elite) | 140 |
| 23:00 | **Line**: hàng Orc Rider | 150 |
| 24:30 | Giảm spawn 30 giây | 60 |
| 25:00 | **Mini-boss: Werebear** (Khó: 2 con) | 80 |
| 26:00 | **Ring**: vòng Armored Skeleton + Armored Orc | 170 |
| 27:00 | **Swarm** Bat + Greatsword Skeleton | 180 |
| 28:00 | Mọi loại quái, spawn tối đa | 200 |
| 29:40 | **Pause**: ngưng spawn 20 giây | 0 |
| 30:00 | **Boss cuối: Necromancer**. Ngưng spawn quái thường (Necromancer tự triệu hồi). Hạ boss → thắng | — |

### Trọng số spawn

Trong mỗi đoạn thời gian, `WaveData` có danh sách `(quái, trọng số)`. Trọng số **dịch dần từ quái yếu sang quái mạnh**, nhưng quái yếu không biến mất hoàn toàn (làm mồi XP). Ví dụ:

| Thời điểm | Slime | Skeleton | Orc | Armored Orc | Skeleton Archer |
|---|---|---|---|---|---|
| 5:00 | 40% | 35% | 25% | — | — |
| 9:00 | 20% | 35% | 30% | 15% | — |
| 12:00 | 10% | 35% | 30% | 15% | 10% |
| 18:00 | 5% | 30% | 25% | 25% | 15% |

---

## 2. Độ khó (tùy chọn của ván)

Cả 3 độ khó dùng **chung kịch bản ở mục 1**. Độ khó chỉ nhân hệ số:

| Chỉ số | Dễ | Trung bình | Khó |
|---|---|---|---|
| Máu quái | ×0,7 | ×1,0 | ×1,5 |
| Sát thương quái | ×0,7 | ×1,0 | ×1,4 |
| Tốc độ quái | ×0,9 | ×1,0 | ×1,1 |
| Tốc độ spawn | ×0,8 | ×1,0 | ×1,3 |
| Số quái tối đa | ×0,8 | ×1,0 | ×1,3 |
| Vàng nhận được | ×1,0 | ×1,0 | ×1,3 (thưởng cho độ khó) |
| Mini-boss phút 25 | 1 con | 1 con | 2 con |
| Boss cuối | Chỉ pha 1 | Chỉ pha 1 | Thêm pha 2 khi còn 50% máu |

```
HP quái     = baseHP × diff.hpMul
Sát thương  = attackDamage × diff.damageMul
maxAlive    = wave.maxAlive × diff.maxAliveMul
interval    = wave.spawnInterval / diff.spawnRateMul
```

Số quái tối đa ở cuối ván độ khó Khó là 300 × 1,3 = **390 con** (đã nhân ×1,5 so với bảng ở mục 1). Đây là tình huống cần đo hiệu năng.

---

## 3. XP, vàng và level up

- Quái chết rơi **gem XP** (luôn luôn) và có tỉ lệ rơi **coin** (vàng). Elite và boss rơi nhiều vàng.
- Người chơi đi gần thì vật phẩm bị hút vào (bán kính = `Magnet`).
- Ngưỡng XP đề xuất: `xpToNext = 5 + level × 10`, chỉnh lại sau khi test. Ván 30 phút nên người chơi dự kiến đạt khoảng cấp 40–50.
- Khi lên cấp: `Time.timeScale = 0` → random 3 nâng cấp **không trùng** từ danh sách `UpgradeData` còn khả dụng → UI hiển thị → người chơi chọn → áp dụng → `Time.timeScale = 1`.
- Nhặt đủ XP cho nhiều cấp một lúc: xếp hàng các lần lên cấp, hiện UI lần lượt.

### Điểm cuối ván

```
Điểm = (giây sống sót × 10 + tổng xpDrop của quái đã giết × 5 + (thắng boss ? 5000 : 0)) × hệ số độ khó
Hệ số độ khó: Dễ ×0,8 · Trung bình ×1,0 · Khó ×1,5
```

- Không tính vàng (đã thành vàng tài khoản) và cấp độ (trùng với XP).
- Bỏ ván không tính điểm.
- Các hệ số nằm trong `ScoreConfig`; hệ số độ khó nằm trong `DifficultyData`.
- Kết thúc ván tạo một `RunResult`: `score`, `runTime`, `kills`, `level`, `victory`, `difficultyId`, `characterId`, `timestamp` (UTC, ISO 8601). Phát qua sự kiện `OnRunFinished`; `RunResultUploader` gửi lên Firebase (lịch sử 10 ván và bảng xếp hạng theo độ khó, xem `thiet-ke-luu-tru.md` mục 5).

---

## 4. Dữ liệu (ScriptableObject)

| ScriptableObject | Nội dung |
|---|---|
| `EnemyData` | Xem `thiet-ke-quai.md` |
| `WaveData` | `startTime`, `spawnInterval`, `spawnPerTick`, `minAlive`, `maxAlive`, danh sách `(EnemyData, weight)`. Không có `endTime`: wave kết thúc khi wave sau bắt đầu |
| `SpawnEvent` | `time`, `pattern` (`Ring` / `Line` / `Swarm` / `Pause`; `Boss` làm ở mốc 6), `enemy`, `count`, `duration` (cho Pause), `spread` (khoảng cách giữa các con trong Line, bán kính đám trong Swarm) |
| `RunScriptData` | Toàn bộ kịch bản một ván: `runDuration` (30 phút), danh sách `WaveData` + `SpawnEvent` (là class `[Serializable]` bên trong, không tách file). Boss cuối thêm ở mốc 6 |
| `DifficultyData` | Các hệ số ở mục 2 |
| `UpgradeData` | Thẻ lên cấp, gồm các loại `NewWeapon` / `WeaponLevel` / `NewPassive` / `PassiveLevel` / `StatCard` / `Evolution` / `Fallback`. Thẻ chỉ số dùng `StatCardData`. Chi tiết ở `thiet-ke-vu-khi.md` mục 5 và 7 |

Luồng hoạt động:
1. Màn chọn độ khó truyền `DifficultyData` vào `GameManager`. `RunScript` dùng chung cho mọi độ khó.
2. `GameManager` giữ đồng hồ ván và trạng thái: `Playing / Paused / LevelUp / BossFight / Victory / GameOver`.
3. `SpawnManager` chọn `WaveData` theo thời gian ván:
   - Số quái sống dưới `minAlive` → bù ngay cho đủ. Nhờ vậy người chơi giết nhanh thì quái ra nhanh hơn.
   - Mỗi `spawnInterval` giây spawn thêm `spawnPerTick` con, miễn chưa vượt `maxAlive`.
   - Mỗi con chọn theo trọng số, xuất hiện trong một vành đai ngoài mép camera: hướng ngẫu nhiên, sâu ngẫu nhiên từ 0 đến `spawnDepth` (mặc định 12 đơn vị). Nhờ vậy quái kéo đến rải rác thay vì ập tới cùng lúc.
   - Tới mốc `SpawnEvent` thì gọi pattern tương ứng. Quái của sự kiện **không bị giới hạn** bởi `maxAlive`, nhưng có tính vào số quái sống.
   - **Ring:** `count` con xếp đều trên vòng tròn ngoài màn hình quanh Player, rồi đuổi vào.
   - **Line:** một hàng dọc ở mép trái hoặc phải, lao ngang qua màn hình theo hướng cố định.
   - **Swarm:** một đám ở mép ngẫu nhiên, bay thẳng qua vị trí Player lúc xuất hiện.
   - Quái của Line và Swarm đi xuyên quái khác, ra khỏi màn hình đủ xa thì biến mất (không rơi đồ).
   - **Pause:** ngưng spawn thường trong `duration` giây.
   - Tua đồng hồ (DebugTools) thì bỏ qua các sự kiện đã bị vượt qua, không phát bù.
4. Chỉ số quái nhân hệ số `DifficultyData` lúc spawn.
5. Quái lấy từ Object Pool.
6. 30:00 → `BossFight`. Hạ boss → `Victory`. HP = 0 → `GameOver`.

---

## 5. Lưu ván giữa chừng

### 5.1. Quy tắc

> **Đang hoãn** (2026-10-14). Nút **"Lưu và thoát"** nằm trong menu Pause; hiện bấm vào chỉ thoát, chưa lưu.

- Lưu từ **menu Pause** (trận đang dừng).
- **Lưu và thoát** (kiểu "suspend save" của các game roguelike): lưu xong thì về Main Menu. Main Menu hiện nút **"Tiếp tục ván"**.
- **Bản lưu bị xóa ngay khi được nạp lại.** Nếu nạp rồi chết, người chơi không thể nạp lại bản cũ để chơi lại đoạn đó. Nhờ vậy việc lưu chỉ để nghỉ giữa chừng, không dùng để gian lận.
- **1 ô lưu duy nhất.** Bắt đầu ván mới khi đang có bản lưu → hỏi xác nhận "Ván đang lưu sẽ bị xóa".
- Nạp lại → vào trận ở đúng thời điểm đã lưu, trận đang dừng (menu Pause), bấm Tiếp tục để chơi tiếp.

### 5.2. Dữ liệu cần lưu (`RunSaveData`)

| Nhóm | Dữ liệu |
|---|---|
| Ván | `runTime`, `difficultyId`, `characterId`, số lần Werebear đã xuất hiện |
| Player | vị trí, `currentHP`, `level`, `currentXP`, `gold`, 15 chỉ số chung đã cộng nâng cấp |
| Vũ khí | danh sách `(weaponId, level)` |
| Quái trên bản đồ | danh sách `(enemyId, vị trí, currentHP)` |
| Vật phẩm trên bản đồ | danh sách gem `(vị trí, giá trị)`, coin `(vị trí, lượng)`, rương chưa nhặt |

**Đơn giản hóa (không lưu):**
- **Đạn đang bay** (của cả Player và quái): bỏ đi khi lưu.
- **Trạng thái AI của quái** (đang nạp đòn lao, đang nhấp nháy sắp nổ, …): khi nạp lại, mọi quái bắt đầu lại từ `Chase`.
- Các thay đổi này chỉ ảnh hưởng một khoảnh khắc sau khi tiếp tục, người chơi gần như không nhận ra.

**Kích thước ước tính:** tối đa ~260 quái + vài trăm gem, file JSON cỡ vài chục KB.

### 5.3. Nơi lưu

- **Trước mắt:** file JSON trên máy (`Application.persistentDataPath/run_save.json`), dùng `JsonUtility`.
- **Sau tuần 6:** khi Người 3 tích hợp Firebase, có thể đưa bản lưu lên cloud. Cấu trúc `RunSaveData` giữ nguyên, chỉ đổi chỗ đọc/ghi.

---

## 6. Checklist phần Systems

### Spawn System
- [ ] `SpawnManager`: sinh quái ngẫu nhiên **ngoài rìa camera**, quanh Player
- [ ] Chọn `WaveData` theo thời gian ván, spawn theo trọng số
- [ ] Các pattern sự kiện: Ring, Line, Swarm, Boss, Pause (`SpawnEvent`)
- [ ] Giới hạn `maxAlive`, đếm số quái sống qua sự kiện `OnDeath`

### XP & Level up
- [ ] Quái chết → rơi gem XP + coin
- [ ] Player nhặt vật phẩm (hút theo `Magnet`) → cộng XP / vàng
- [ ] Ngưỡng XP tăng dần theo cấp
- [ ] Lên cấp → pause → random 3 `UpgradeData` không trùng → áp dụng → resume
- [ ] Xử lý lên nhiều cấp liên tiếp

### GameManager & Độ khó
- [ ] Máy trạng thái: Playing / Paused / LevelUp / BossFight / Victory / GameOver
- [ ] Đồng hồ ván 30 phút, dừng khi LevelUp / Paused
- [ ] 30:00 → Boss cuối; hạ boss → thắng; HP = 0 → thua
- [ ] 3 `DifficultyData` áp hệ số lên cùng một `RunScript`
- [ ] Dữ liệu cho HUD: thời gian ván, XP hiện tại/cần thiết, cấp, vàng

### Lưu ván
- [ ] `RunSaveData`: thu thập dữ liệu Player, vũ khí, quái, vật phẩm
- [ ] Lưu và thoát trong menu Pause → ghi JSON → về Main Menu
- [ ] Main Menu: nút Tiếp tục ván khi có bản lưu
- [ ] Nạp: dựng lại Player, quái (từ pool, trạng thái `Chase`), vật phẩm → mở menu Pause → xóa bản lưu
- [ ] Xác nhận khi bắt đầu ván mới mà đang có bản lưu

### Kiểm thử
- [ ] Chơi trọn 30 phút ở cả 3 độ khó
- [ ] Đo FPS ở phút 28–30 độ khó Khó (tối đa ~260 quái), dùng Profiler
- [ ] Tăng tốc thời gian khi test (`Time.timeScale` hoặc nút tua đồng hồ trong Editor), để không phải chơi đủ 30 phút mỗi lần

### Báo cáo
- [ ] Sequence Diagram gợi ý: *Spawn quái*, *Quái chết → rơi XP*, *Level up → chọn nâng cấp*, *Mua ở Shop vĩnh viễn*, *Lưu và thoát / Tiếp tục ván*

---

## 7. Điểm cần đồng bộ với các thành viên khác

| Với ai | Nội dung |
|---|---|
| Người 1 | Base class `Enemy` / `Entity`, hàm `Init()`, sự kiện `OnDeath` |
| Người 1 | API Object Pool (lấy / trả object). Enemy cần hàm lấy/khôi phục `(enemyId, vị trí, currentHP)` cho việc lưu ván |
| Người 1 | Cách áp dụng `UpgradeData` vào Player / Weapon |
| Người 3 | Dữ liệu cho HUD: thời gian ván, XP, cấp, vàng |
| Người 3 | Sự kiện `OnLevelUp` và callback khi chọn nâng cấp |
| Người 3 | Màn chọn độ khó truyền `DifficultyData` vào `GameManager` |
| Người 3 | Scene Shop vĩnh viễn (`huong-dan-shop.md`); menu Pause có Tiếp tục và Lưu và thoát |
| Người 3 | Main Menu: nút Tiếp tục ván; sau tuần 6 có thể đưa bản lưu lên Firebase |
| Cả nhóm | Sửa checklist nhóm: bỏ "mỗi độ khó nhiều màn" và "tăng HP quái theo màn", thay bằng cấu trúc ván 30 phút |

---

## 8. Câu hỏi còn mở

(Chưa có câu hỏi mở.)

### Đã chốt
- **Bỏ ván** (thoát mà không lưu) → **không** tính điểm và vàng lên leaderboard. Ván bị hủy, xóa bản lưu nếu có.
