# Kịch bản màn chơi & thiết kế hệ thống Spawn

Game: Roguelike Survival Shooter (Unity), lấy cảm hứng từ Vampire Survivors
Phụ trách: Người 2 (Systems): SpawnManager, XP, Level up, GameManager (wave, 3 độ khó)
Trạng thái: **Bản nháp**. Danh sách quái chưa chốt, tên quái bên dưới chỉ là tạm đặt.

---

## 0. Giả định

- 1 map, mỗi lượt chơi dài **15 phút**.
- Thắng khi sống đến 15:00 **và** hạ được boss cuối. Thua khi HP người chơi về 0.
- 3 độ khó: **Dễ / Thường / Khó**.
- Nhịp độ theo kiểu **răng cưa**: áp lực tăng dần, sau mỗi sự kiện lớn có 15–20 giây nhẹ để người chơi nhặt XP và lên cấp.

---

## 1. Danh sách quái (tạm đặt, chờ chốt)

| ID tạm | Loại | Vai trò | Đặc điểm |
|---|---|---|---|
| `Grunt` | Zombie/slime | Quân số | Chậm, máu thấp, đi thẳng tới người chơi |
| `Runner` | Dơi | Quấy rối | Rất nhanh, máu rất thấp |
| `Brute` | Quái to | Chặn đường | Chậm, máu cao, ít bị đẩy lùi |
| `Ranged` | Cung/pháp sư | Ép di chuyển | Giữ khoảng cách, bắn đạn |
| `Exploder` | Quái nổ | Trừng phạt đứng yên | Lao tới rồi tự nổ |
| `Elite` | Bản to của quái thường | Phần thưởng | Máu ×5, rơi rương |
| `MiniBoss` / `Boss` | Boss | Cột mốc | Phút 5, 10 (mini-boss) và 15 (boss cuối) |

> Hệ thống spawn phải **không phụ thuộc vào danh sách này**: thêm, bớt hay đổi quái chỉ cần sửa dữ liệu (ScriptableObject), không phải sửa code.

---

## 2. Kịch bản theo thời gian

### Giai đoạn 1: Làm quen (0:00–3:00)
- Quái: `Grunt`. Từ 1:30 thêm `Runner`.
- Tối đa 30 quái cùng lúc. Nhịp spawn giảm dần từ 1,5 giây xuống 1,0 giây.
- **2:30, Sự kiện "Đàn dơi" (Swarm):** một đàn `Runner` bay ngang màn hình theo một hướng.

### Giai đoạn 2: Áp lực (3:00–6:00)
- Từ 3:00 thêm `Brute`. Tối đa 60 quái.
- **4:00, Sự kiện "Vòng vây" (Ring):** `Grunt` xuất hiện thành vòng tròn khép lại quanh người chơi.
- **5:00, Mini-boss 1**, rơi rương khi bị hạ.

### Giai đoạn 3: Đa dạng (6:00–9:00)
- Từ 6:00 thêm `Ranged`, từ 7:30 thêm `Exploder`.
- Cứ 90 giây xuất hiện 1 `Elite`.
- Tối đa 100 quái.
- **8:00, Sự kiện "Càn quét" (Line):** một hàng `Brute` lao ngang qua màn hình.

### Giai đoạn 4: Cao trào (9:00–12:00)
- Trộn đủ mọi loại quái. Tối đa 150 quái.
- **10:00, Mini-boss 2.** Ở độ khó Khó sẽ có 2 con.
- **11:00:** Vòng vây dày hơn, gồm `Grunt` và `Brute`.

### Giai đoạn 5: Sống còn (12:00–15:00)
- Spawn ở mức tối đa.
- **14:00:** tạm ngưng spawn 20 giây, đây là khoảng lặng trước boss.
- **15:00, Boss cuối:** dừng spawn quái thường, thắng khi hạ được boss.

### Bảng tóm tắt

| Thời điểm | Quái mới | Số quái tối đa | Sự kiện |
|---|---|---|---|
| 0:00 | Grunt | 30 | — |
| 1:30 | Runner | 30 | — |
| 2:30 | — | 30 | Swarm (Runner) |
| 3:00 | Brute | 60 | — |
| 4:00 | — | 60 | Ring (Grunt) |
| 5:00 | — | 60 | Mini-boss 1 |
| 6:00 | Ranged | 100 | Elite mỗi 90 giây |
| 7:30 | Exploder | 100 | — |
| 8:00 | — | 100 | Line (Brute) |
| 9:00 | (trộn tất cả) | 150 | — |
| 10:00 | — | 150 | Mini-boss 2 |
| 11:00 | — | 150 | Ring (Grunt + Brute) |
| 12:00 | — | tối đa | — |
| 14:00 | — | 0 | Tạm ngưng 20 giây |
| 15:00 | — | 0 | Boss cuối |

---

## 3. Hệ số theo độ khó

| Chỉ số | Dễ | Thường | Khó |
|---|---|---|---|
| Máu quái | ×0.7 | ×1.0 | ×1.5 |
| Sát thương quái | ×0.7 | ×1.0 | ×1.4 |
| Tốc độ quái | ×0.9 | ×1.0 | ×1.1 |
| Tốc độ spawn | ×0.8 | ×1.0 | ×1.3 |
| Số quái tối đa | ×0.8 | ×1.0 | ×1.3 |
| Tăng máu theo thời gian | +3%/phút | +5%/phút | +8%/phút |
| Boss | Kỹ năng cơ bản | Kỹ năng cơ bản | Thêm pha 2 khi còn 50% máu |

Công thức máu quái tại thời điểm `t` (phút):
`HP = baseHP × difficulty.hpMultiplier × (1 + difficulty.hpGrowthPerMinute × t)`

---

## 4. Chuyển kịch bản thành dữ liệu (ScriptableObject)

- **`WaveData`**: `startTime`, `endTime`, danh sách `(EnemyData, weight)`, `spawnInterval`, `maxAlive`
- **`SpawnEvent`**: `time`, `pattern` (`Ring` / `Line` / `Swarm` / `Boss` / `Pause`), `enemy`, `count`, `duration`
- **`DifficultyData`**: các hệ số ở mục 3
- **`LevelScript`** (hoặc `StageData`): gom danh sách `WaveData` và `SpawnEvent` cho một map, kèm `runDuration`

Luồng hoạt động:
1. `GameManager` giữ đồng hồ trận và độ khó đang chọn.
2. `SpawnManager` đọc thời gian hiện tại để chọn `WaveData` đang hiệu lực, spawn theo trọng số, không vượt `maxAlive`.
3. Khi đến mốc của `SpawnEvent`, gọi đúng pattern spawn tương ứng.
4. Mọi chỉ số quái được nhân hệ số từ `DifficultyData` lúc spawn.
5. Quái lấy từ Object Pool (do Người 1 làm), không dùng `Instantiate`/`Destroy` trực tiếp.

---

## 5. Điểm cần đồng bộ với các thành viên khác

| Với ai | Nội dung | Tuần |
|---|---|---|
| Người 1 | Interface/base class của `Enemy` (khởi tạo chỉ số, sự kiện chết để rơi XP) | 1 |
| Người 1 | API của Object Pool (lấy và trả object) | 3 |
| Người 3 | Sự kiện `OnLevelUp` (pause game, mở UI chọn 3 nâng cấp) | 3 |
| Người 3 | Dữ liệu cho HUD: thời gian trận, XP hiện tại/cần thiết, cấp độ | 2 |
| Người 3 | Màn chọn độ khó truyền `DifficultyData` vào `GameManager` | 4 |

---

## 6. Câu hỏi còn mở

- Chủ đề game và danh sách quái chính thức?
- Độ dài lượt chơi 15 phút có phù hợp để demo/bảo vệ không? (Có thể rút xuống 10 phút bằng cách co các mốc thời gian.)
- Có làm nhiều map không, hay chỉ 1 map?
- Công thức XP cần cho mỗi cấp (đề xuất: `xpToNext = 5 + level × 10`, chỉnh sau khi test).
