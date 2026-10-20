# Phân tích 6 chức năng chính (cho báo cáo)

Game: Survival Action kiểu Vampire Survivors (Unity), 2,5D
Liên quan: `thiet-ke-nhan-vat.md`, `thiet-ke-quai.md`, `thiet-ke-vu-khi.md`, `kich-ban-man-choi.md`
Trạng thái: **Bản đề xuất, chờ nhóm duyệt.** Lược đồ sẽ viết bằng Mermaid trong `diagrams.md` sau khi chốt.

---

## Tiêu chí chọn

- Là **một chức năng người chơi trải nghiệm được**, không phải một thành phần kỹ thuật lẻ.
- **Phạm vi vừa phải**, có **tính kỹ thuật cao** (thuật toán, hiệu năng, quản lý trạng thái), không chọn theo độ lớn.
- **Ưu tiên chức năng đã có code chạy được**, để lược đồ khớp với code thật. Riêng chức năng 6 (Shop) chưa có code, lược đồ vẽ theo `thiet-ke-shop.md` và cần đối chiếu lại khi cài đặt (mốc 7).
- Mỗi chức năng tách được thành **2–3 lược đồ** (State, Activity, Sequence).

---

## 1. Điều khiển nhân vật và sinh tồn

**Người chơi thấy:** di chuyển bằng WASD, nhân vật quay theo chuột, bị quái đánh thì mất máu, hết máu thì chết hoặc hồi sinh.

**Điểm kỹ thuật:**
- FSM viết bằng `enum` + `switch` ngay trong class `Player` (Idle, Move, Dead).
- Hướng ngắm (`AimDirection`, theo chuột) tách khỏi hướng chạy (`MoveDirection`, theo WASD). Sprite lật theo hướng ngắm.
- Sát thương nhận: `max(1, dmg − Armor)`. Giáp trừ thẳng.
- Hồi máu theo thời gian (`Recovery`). Không có thời gian bất tử, chỉ nháy đỏ 0,1 giây.
- Hồi sinh với 50% máu nếu còn `Revival`. Hết `Revival` thì chuyển sang `Dead` và phát sự kiện `OnDeath`.
- Khi chết, Animator chuyển sang Unscaled Time để animation chết vẫn chạy trong lúc game dừng.

**Class liên quan:** `Entity`, `Player`, `Swordsman`, `PlayerStats`, `CharacterData`

**Lược đồ đề xuất:**
1. State: Idle ↔ Move → Dead (kèm nhánh hồi sinh).
2. Activity: xử lý khi nhận một đòn sát thương.

---

## 2. Tấn công tự động

**Người chơi thấy:** vũ khí tự đánh theo nhịp về phía chuột. Quái trong vùng chém mất máu, nháy màu và bị đẩy lùi.

**Điểm kỹ thuật:**
- Nhịp đánh = `attackInterval / AttackSpeed`. Vòng lặp vũ khí do `Player` gọi `Tick()`, không dùng `Update` riêng.
- Xác định trúng đòn bằng hình học vector: khoảng cách ≤ tầm (`size × Area` + bán kính quái), và tích vô hướng với hướng chém ≥ cos(góc vòng cung / 2).
- Chuỗi tính sát thương: `damage × Might` → chí mạng (Swordsman) → `× (1 − kháng %)` theo loại Vật lý/Phép → đẩy lùi `× (1 − knockbackResist)`.
- Cách đánh nằm ở `Weapon`, chỉ số riêng nằm ở lớp con của `Player`, loại sát thương nằm ở nguồn gây sát thương (`WeaponData`).
- Animation Attack tự tăng tốc khi nhịp đánh ngắn hơn clip: `speed = max(1, clipLength / interval)`.
- Vũ khí có 5 cấp, mỗi cấp ghi giá trị tuyệt đối trong `WeaponData`.

**Class liên quan:** `Weapon`, `SwordSlash`, `WeaponData`, `WeaponLevelStats`, `DamageInfo`, `Enemy`

**Lược đồ đề xuất:**
1. State: vũ khí Chờ hồi ↔ Tấn công.
2. Activity: một lần chém (duyệt quái → lọc theo tầm → lọc theo góc → tính sát thương).
3. Sequence: Player → Weapon → EnemyManager → Enemy.

---

## 3. Quái xuất hiện, truy đuổi và tấn công

**Người chơi thấy:** quái hiện dần ngoài màn hình, đuổi theo người chơi, chạm vào thì gây sát thương, chết thì phát animation rồi biến mất. Quái bị bỏ lại quá xa sẽ xuất hiện lại phía trước.

**Điểm kỹ thuật:**
- **Object Pool** (`UnityEngine.Pool.ObjectPool`) cho quái, không `Instantiate`/`Destroy` trong lúc chơi.
- Quái không có `Update` riêng. `EnemyManager` gọi `Tick()` cho mọi quái trong một vòng lặp.
- Quái không có collider. Va chạm với Player tính bằng `sqrMagnitude`, giới hạn tối đa 1 lần mỗi `contactDamageInterval` (0,5 giây) cho từng con.
- Xóa khỏi danh sách trong O(1) bằng cách hoán đổi với phần tử cuối (mỗi quái giữ `ListIndex`).
- Quái ở xa hơn ~1,5 lần màn hình được dịch chuyển ra phía trước hướng chạy của Player, không despawn.
- Kết quả đo: 300 quái giữ 60 FPS trên bản build (VSync bật).

**Class liên quan:** `Enemy`, `EnemyManager`, `EnemyData`, `PoolManager`

**Lược đồ đề xuất:**
1. State: Spawning (hiện dần 0,3 giây) → Chase → Dead (chờ hết clip Death) → về pool.
2. Activity: `EnemyManager` xử lý một frame.
3. Activity: lấy và trả object từ pool (`Get` / `Release`).

---

## 4. Thu thập kinh nghiệm và vàng

**Người chơi thấy:** quái chết rơi gem XP (luôn luôn) và coin (theo tỉ lệ). Người chơi đi gần thì vật phẩm bay về phía mình.

**Điểm kỹ thuật:**
- Vật phẩm bị hút khi vào bán kính `Magnet`, sau đó bay theo Player tới khi nhặt được. Tốc độ hút (12) phải luôn lớn hơn tốc độ chạy tối đa của Player.
- Khi có quá 300 gem trên bản đồ, gem mới gộp vào gem gần nhất để giữ hiệu năng.
- Màu và kích thước gem đổi theo giá trị: xanh dương 1–2, xanh lá 3–9, đỏ ≥ 10.
- Nhặt gem cộng `XP × Growth`, nhặt coin cộng `vàng × Greed`.
- `PickupManager` gọi `Tick()` cho mọi vật phẩm giống `EnemyManager`, vật phẩm cũng dùng pool.

**Class liên quan:** `PickupManager`, `Pickup`, `PlayerProgression`, `ProgressionConfig`, `GemTier`

**Lược đồ đề xuất:**
1. State: vật phẩm Nằm yên → Bị hút → Được nhặt.
2. Activity: rơi đồ khi quái chết (có nhánh gộp gem).
3. Sequence: Enemy chết → EnemyManager → PickupManager → PlayerProgression.

---

## 5. Lên cấp và chọn nâng cấp

**Người chơi thấy:** đủ XP thì game dừng lại, hiện 3 thẻ. Người chơi chọn 1 thẻ thì game chạy tiếp.

**Điểm kỹ thuật:**
- Ngưỡng XP: `xpToNext = 5 + level × 10`. Một lần nhặt có thể lên nhiều cấp, các lần lên cấp được xếp hàng và hiện UI lần lượt.
- Game dừng bằng `Time.timeScale = 0`, UI vẫn bấm được.
- Thuật toán rút thẻ:
  - Gom ứng viên: nâng cấp vũ khí, vũ khí mới, vật phẩm mới, nâng cấp vật phẩm, thẻ chỉ số.
  - Trộn ngẫu nhiên bằng Fisher–Yates.
  - Lấy tối đa 3, trong đó **tối đa 1 thẻ chỉ số**. Tôn trọng giới hạn 4 ô vũ khí và 4 ô vật phẩm.
  - Thiếu thì bù thẻ dự phòng (Hồi 30 HP / +20 vàng).
- Áp dụng chỉ số theo 2 kiểu: cộng thẳng (`Flat`) và theo % chỉ số gốc (`PercentOfBase`).

**Class liên quan:** `PlayerProgression`, `LevelUpManager`, `UpgradeOption`, `PlayerInventory`, `GameManager`, `LevelUpPanel`

**Lược đồ đề xuất:**
1. State: game Playing ↔ LevelUp (kèm số lần lên cấp đang chờ).
2. Activity: thuật toán rút 3 thẻ (`RollOptions`).
3. Sequence: nhặt gem → lên cấp → chọn thẻ → tiếp tục.

---

## 6. Shop trong ván

**Người chơi thấy:** ở phút 10:00 và 20:00, trận đấu đóng băng và Shop hiện ra với 5 ô hàng. Người chơi mua đồ bằng vàng nhặt được, làm mới hàng, hoặc bỏ bớt vũ khí/vật phẩm để lấy lại ô trống. Bấm Tiếp tục thì đếm ngược 3–2–1 rồi trận chạy tiếp từ đúng chỗ đã dừng.

**Phạm vi phân tích:** chỉ phần **trong ván** (bày hàng, mua, làm mới, bỏ đồ, tiếp tục). Không phân tích vàng tài khoản, mở khóa vũ khí ở Main Menu và Lưu và thoát.

**Điểm kỹ thuật:**
- **Thuật toán bày 5 ô hàng:** rút ngẫu nhiên có trọng số kèm ràng buộc.
  - Lọc bỏ món không khả dụng: hết ô vũ khí/vật phẩm, đạt giới hạn mua mỗi ván, vũ khí chưa mở khóa.
  - Bảo đảm ít nhất 1 bình máu và 1 món liên quan đến build hiện tại (+1 cấp vũ khí hoặc vật phẩm đang có).
  - 3 ô còn lại rút theo trọng số: nâng cấp đồ đang có 30, đồ mới 25, bình máu 15, Sách/Túi 15, Rương bí ẩn 10, Ngọc hồi sinh 5.
- **Giá động:** nâng cấp vũ khí `20 + 10 × cấp hiện tại`, vật phẩm `15 + 10 × cấp hiện tại`. Làm mới hàng 5 vàng, mỗi lần sau +5, reset ở lần mở Shop sau.
- **Bỏ đồ:** hoàn 30% tổng giá trị đã đầu tư theo bảng giá (kể cả cấp nhận miễn phí khi lên cấp). Không bỏ được vũ khí khởi đầu. Bỏ vật phẩm có thể làm mất điều kiện tiến hóa của vũ khí tương ứng.
- **Đóng băng và quay lại:** `Time.timeScale = 0`, Shop là UI phủ lên màn chơi, không chuyển scene, không xóa quái. Đếm ngược 3–2–1 chạy bằng unscaled time.
- **Dùng chung code áp dụng nâng cấp** với chức năng 5 (lên cấp).

**Class liên quan (dự kiến):** `ShopManager`, `ShopItemData`, `ShopConfig`, `PlayerInventory`, `PlayerProgression`, `GameManager`

**Lược đồ đề xuất:**
1. State: Đóng → Đang mở (xem hàng) → Xác nhận bỏ đồ → Đếm ngược 3–2–1 → Đóng (kèm nhánh Lưu và thoát).
2. Activity: thuật toán bày 5 ô hàng (lọc → ô bảo đảm → rút theo trọng số).
3. Activity: mua một món (đủ vàng? → trừ vàng → áp dụng → ô hiện "Đã bán").

---

## Tổng hợp

| # | Chức năng | Số lược đồ | Điểm kỹ thuật chính |
|---|---|---|---|
| 1 | Điều khiển nhân vật và sinh tồn | 2 | FSM, tách hướng ngắm/hướng chạy, hồi sinh |
| 2 | Tấn công tự động | 3 | Hình học vector, chuỗi công thức sát thương |
| 3 | Quái xuất hiện, truy đuổi và tấn công | 3 | Object Pool, va chạm không physics, O(1), 60 FPS |
| 4 | Thu thập kinh nghiệm và vàng | 3 | Hút theo Magnet, gộp gem giới hạn hiệu năng |
| 5 | Lên cấp và chọn nâng cấp | 3 | Hàng đợi lên cấp, thuật toán rút thẻ có ràng buộc |
| 6 | Shop trong ván | 3 | Rút có trọng số kèm ràng buộc, giá động, hoàn tiền, đóng băng trận |

Tổng cộng khoảng **17 lược đồ**.

---

## Không chọn

| Chức năng | Lý do |
|---|---|
| Spawn quái theo kịch bản 30 phút | Quá lớn, chưa có code (đợt B) |
| Kết thúc ván và tính điểm | Đã có code nhưng kỹ thuật đơn giản (đồng hồ ván, công thức điểm). Mô tả ngắn trong phần giới thiệu chung |
| Lưu và nạp ván | Quá lớn, chưa có code (mốc 7) |
| Boss Werebear, Necromancer | Chưa có code (mốc 6). **Phương án thay thế** nếu cần một chức năng có AI: Werebear có FSM chu kỳ Đuổi → Đập đất (báo hiệu) → Lao tới, vẽ theo `thiet-ke-quai.md` mục 4.1 |
| Chọn nhân vật và độ khó | Quá đơn giản |
| Bảng chỉ số (giữ Tab) | Quá đơn giản |

---

## Câu hỏi chờ chốt

- Duyệt 6 chức năng trên hay đổi mục nào?
- Ghi chú trong lược đồ dùng tiếng Việt hay tiếng Anh?
- Trường hoặc giảng viên có yêu cầu mẫu/chuẩn UML cụ thể nào không?
