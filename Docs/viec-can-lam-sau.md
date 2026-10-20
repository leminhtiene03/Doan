# Việc cần làm sau

Danh sách việc đã phát hiện nhưng tạm hoãn để làm các bước chính trước. Làm xong mục nào thì xóa mục đó.

Cập nhật lần cuối: 2026-10-19

---

## 1. Kiểm tra lật hướng (đã code 2026-10-19, chờ test)

Đã làm cách 1 cho cả Player và quái: `Entity.Face` đổi dấu `transform.localScale.x`, chỉ khi hướng thực sự đổi. Quái chỉ quay đầu khi `|moveDirection.x| ≥ faceTurnThreshold` (tạm 0,1) để không rung collider khi đứng ngay trên/dưới Player. Prefab `Swordsman`, `Acher`, `Wizard` đang lưu `Flip X = 1` trong SpriteRenderer, đã đặt lại 0.

Cần kiểm tra:
- Offset collider chân của mọi prefab khớp với tư thế **quay phải** (bật Physics 2D → Gizmos → Always Show Colliders). Offset hiện tại: Archer −0,07, Swordsman 0, Wizard +0,04, Skeleton −0,23, Knight Templar −0,24.
- Mũi tên trúng quái đều ở cả 2 hướng quay mặt.
- Nếu quái vẫn rung khi đứng thẳng trên/dưới Player thì tăng `faceTurnThreshold`.

---

## 2. Cần có map trước

- Mũi tên của Cung thủ dừng lại và biến mất khi chạm vật cản (layer `Obstacle`, thêm vào ma trận va chạm của `PlayerProjectile`).
- Skeleton Archer chỉ bắn khi đường bay thông thoáng (`CircleCast` trên `Obstacle` + `Enemy`). Thuộc mốc 6, đã ghi trong `thiet-ke-quai.md`.

---

## 3. Cân bằng và số liệu

- **Tầm bay của đạn:** Tia phép 9, mũi tên 10, chưa tới mép trái/phải màn hình (khoảng 12,4). Đề xuất 13 cho Tia phép, 14 cho Cung, hoặc giữ ngắn nếu muốn Pháp sư phải đánh gần.
- **Swordsman:** `CharacterData.asset` có giáp 0, tài liệu ghi +1. `baseMoveSpeed` 1 × `moveSpeed` 4 đang ngược quy ước (các lớp khác dùng `baseMoveSpeed` 4 × `moveSpeed` theo %).
- **Chỉ số quái** trong `EnemyData` lệch nhiều so với `thiet-ke-quai.md` (trâu gấp 2–3 lần, rơi vàng gấp 5–10 lần). Người dùng chọn giữ nguyên, cân bằng lại ở mốc 8.
- **Giới hạn phản đòn của Guardian** đang là 10 mỗi đòn cố định. Có thể đổi sang giới hạn theo % máu tối đa của kẻ đánh.
- Sau khi tăng tầm đánh quái lên 0,25: theo dõi Archer và Mage có bị vây quá khó không. Nếu có, giảm xuống khoảng 0,2.

---

## 4. Tính năng còn thiếu

- **Đổi mật khẩu** (nút ChangePass trong khung tài khoản đang tạm khóa). Hai cách: (A) đổi trong game, hỏi mật khẩu hiện tại → `ReauthenticateAsync` rồi `UpdatePasswordAsync`; (B) gửi email đặt lại bằng `AuthService.SendPasswordResetAsync` có sẵn.
- **HUD vũ khí và vật phẩm đang có** (người dùng hoãn): 2 hàng ô ở góc trên trái (4 vũ khí, 4 vật phẩm), mỗi ô có icon và cấp (MAX / EVO). Đề xuất: `PlayerInventory` thêm sự kiện `OnChanged`, script `InventoryHudView` vẽ lại ô khi có thay đổi; ô mẫu dựng trong Canvas (prefab) để đồng bộ giao diện với nhóm.
- **Thay hình tạm** trong `Assets/Art/Placeholder/` (`Dagger`, `IceShard`, `LightningBolt`, `Boomerang`, `SpiritSword`, `Caltrops`, `PoisonPool`) bằng sprite thật. Chỉ cần đổi sprite trong prefab tương ứng ở `Prefab/` (`Dagger`, `IceShard`, `LightningEffect`, `Boomerang`, `SpiritSword`, `CaltropsZone`, `PoisonPool`). Quả cầu phép (`Prefab/Orb`) đang mượn sprite cầu lửa của Wizard.
- **Icon** cho vật phẩm bị động, thẻ chỉ số và vũ khí nhận thêm đang để trống (người dùng sẽ tự bổ sung).
- Nội tại *Chiến ý* của Swordsman (mỗi 10 cấp +5% Might, tối đa +25%). Dùng `Player.OnLevelChanged` như các lớp khác.
- Hiệu ứng vệt chém cho Trọng kích (tài liệu gợi ý gói *250 Pixel Shader Slash*, chưa có trong project).
- Bình độc hiện tạo vũng ngay tại chỗ quái, chưa có cảnh bình bay. Nếu không có quái trong tầm 6, lượt ném vẫn bị tính (mất 3 giây).
- Vùng sát thương (chông, vũng độc) dùng sorting order −1. Khi vẽ map, đặt Tilemap thấp hơn −1 để vùng không bị nền che.
- Đóng băng của Bão tuyết là làm chậm 100%: quái đứng im nhưng vẫn ra đòn nếu Player trong tầm. Muốn đóng băng thật thì `Enemy` cần trạng thái bị khống chế (không đánh).
- Kiếm hồn đang mượn hình tạm `SpiritSword`.
- Hit flash hiện là tô màu đỏ. Muốn nháy trắng thật thì cần shader.
- Xác quái có thể vẽ đè lên quái còn sống. Nếu khó nhìn: hạ sorting order khi chết.
- Ô `hurtbox` tùy chọn trong `Enemy`: gán collider vẽ tay thì dùng, bỏ trống thì tự sinh như hiện tại. Dùng cho boss hoặc quái dáng lạ.
- `goldMul` của độ khó (làm cùng Shop, mốc 7).
- 2 Werebear ở phút 25 độ khó Khó; boss phút 15, 25, 30 (mốc 6).

---

## 5. Kiểm thử còn nợ

- **Stress test** sau khi thêm vật lý: độ khó Khó, tua tới phút 28, khoảng 390 quái, ghi FPS trên bản build.
- Test đủ các sự kiện spawn bằng tua giờ: phút 4 (Swarm), 6 (Ring), 12 (Line), 15 (Bear).

---

## 6. Cập nhật sơ đồ báo cáo

`Docs/diagrams-fixed.md` và các file `Docs/diagrams/*.puml` đã lệch so với code và thiết kế hiện tại. Nên cập nhật một lượt trước khi nộp báo cáo:
- Bỏ hệ thống kỹ năng: `ActivateSkill`, `SkillData`, trạng thái `UsingSkill`, thẻ `SkillLevel`, chỉ số `Cooldown`.
- Quái đánh theo tầm (`attackRange`, `attackHitTime`, trạng thái `Attack`) thay cho sát thương va chạm (`contactDamage`, `contactRadius`, `TryContact`).
- Thêm `SpawnManager` (wave theo thời gian, sự kiện Ring/Line/Swarm/Pause), `ProjectileManager`, `Projectile`, `Hurtbox`.
- `GameManager` sinh Player theo lớp đã chọn.
- Giữ quy tắc: không đưa DebugTools vào sơ đồ báo cáo.

---

## 7. Dọn dẹp project

- `Scripts/kich-ban-man-choi.md`: bản nháp cũ (v1, ván 15 phút), trùng tên với bản v3 trong `Docs/`.
- `Scripts/Systems/`: thư mục trống.
- `Animation/player-Acher/Acher 1.controller` và `Animation/skeleton/skeleton.controller`: controller thừa, không prefab nào dùng.
- Xóa các file này trong Unity Editor, không xóa bằng lệnh hệ thống.
