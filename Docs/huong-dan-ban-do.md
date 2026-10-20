# Hướng dẫn dựng bản đồ

Liên quan: `kich-ban-man-choi.md` (spawn), `CLAUDE.md` mục 4 (quy tắc 13: vành đai spawn)
Trạng thái: **v1** (2026-10-19). Map hình chữ nhật 150 × 150 đơn vị, có tường viền và các cụm vật cản bên trong.

---

## 0. Cách hoạt động

- Object `Map` có `BoxCollider2D` xác định vùng chơi và component `MapBounds`.
- Lúc vào ván, `MapBounds` **tự sinh 4 bức tường** ngoài viền vùng chơi (layer `Obstacle`). Không cần đặt tường bằng tay.
- Player (Kinematic, `Rigidbody2D.Slide`) và quái (Dynamic) bị tường và vật cản chặn qua ma trận va chạm.
- Quái thường vẫn sinh trong vành đai ngoài mép camera, nhưng **chỉ ở chỗ nằm trong map và không trúng vật cản**. Đứng gần mép thì quái dồn ra từ phía còn trống.
- Quái đi ngang (Line, Swarm) bị hủy khi chạm viền map.
- Quái vẫn đuổi thẳng tới Player, không tìm đường. Gặp vật cản thì trượt dọc theo nó. Vì vậy **vật cản nên là cụm tròn hoặc vuông**, tránh hình chữ U hay ngõ cụt làm quái kẹt.
- Chưa làm: đạn của người chơi bị vật cản chặn, tầm nhìn của Skeleton Archer (xem `viec-can-lam-sau.md`).

---

## 1. Layer và ma trận va chạm

1. **Edit → Project Settings → Tags and Layers**, thêm 2 layer: `Obstacle` và `MapArea`.
2. **Project Settings → Physics 2D → Layer Collision Matrix**:
   - `Obstacle`: chỉ bật với `Player` và `Enemy`.
   - `MapArea`: **tắt hết** (vùng map chỉ để đọc kích thước, không được va chạm với gì).

Phải có layer `Obstacle` trước khi bấm Play, nếu không `MapBounds` sẽ báo lỗi.

---

## 2. Ảnh Tiny Swords (PPU 24)

Giữ quy ước PPU 24 của project.

1. Chọn `UI/Tiny Swords (Free Pack)/Terrain/Tileset/Tilemap_color1.png` (và các ảnh khác sẽ dùng: cây, đá, bụi).
2. Inspector: **Pixels Per Unit = 24**, **Filter Mode = Point**, **Compression = None** → Apply.
3. Ảnh tileset đã cắt sẵn (`Sprite Mode = Multiple`). Nếu chưa cắt: **Sprite Editor → Slice → Grid By Cell Size 64 × 64**.

Mỗi ô 64 px ở PPU 24 rộng **2,667 đơn vị**, nên map 150 đơn vị ≈ **56 × 56 ô**.

---

## 3. Grid và Tilemap

Trong scene `UI_InGame`:

1. **Create → 2D Object → Tilemap → Rectangular**. Đổi tên object `Grid` (giữ nguyên), tilemap đầu tiên đặt tên `Ground`.
2. Chọn `Grid`: **Cell Size = 2.6667 × 2.6667**.
3. Tạo thêm tilemap con thứ hai trong `Grid`, tên `Obstacles`.

| Tilemap | Layer | Sorting | Component thêm |
|---|---|---|---|
| `Ground` | Default | Order in Layer thấp nhất (ví dụ -100) | Không |
| `Obstacles` | `Obstacle` | Thấp hơn nhân vật một chút (ví dụ -50) | `TilemapCollider2D` (bật **Composite Operation = Merge**), `CompositeCollider2D`. `Rigidbody2D` tự thêm vào: đặt **Body Type = Static** |

4. **Window → 2D → Tile Palette** → Create New Palette (Cell Size: Automatic) → kéo `Tilemap_color1.png` vào để tạo tile.
5. Vẽ nền lên `Ground` phủ kín vùng 150 × 150 (từ khoảng -75 đến +75 trên cả 2 trục). Nên vẽ **rộng hơn vùng chơi vài ô** để mép camera không lộ nền trống.
6. Vẽ vật cản (vách đá cao, nước) lên `Obstacles`.

**Cây, đá, nhà đặt rời** (không phải tile): kéo sprite vào scene, đặt layer `Obstacle`, thêm `BoxCollider2D` hoặc `CircleCollider2D` **chỉ ôm phần gốc** (như collider chân của quái) để nhân vật đi được ra sau tán cây. Gom các object này vào một object cha `Props` cho gọn.

---

## 4. Vùng map (`MapBounds`)

1. **Create Empty** ở gốc Hierarchy, tên `Map`, **Position (0, 0, 0)**, layer `MapArea`.
2. **Add Component → Box Collider 2D**: **Size = 150 × 150**, Offset 0. (Is Trigger tự bật khi chạy.)
3. **Add Component → Map Bounds**:

| Ô | Giá trị tạm | Ý nghĩa |
|---|---|---|
| Wall Thickness | 2 | Độ dày tường viền tự sinh |
| Edge Padding | 1 | Quái không sinh ra trong khoảng này sát viền |
| Obstacle Check Radius | 0,5 | Bán kính kiểm tra "điểm spawn có trúng vật cản không" |

Viền vùng map hiện bằng khung vàng trong cửa sổ Scene.

4. Chọn object có `EnemyManager` (trong `Managers`): kéo `Map` vào ô **Map Bounds**. **Spawn Attempts** = 12 (số lần thử tìm chỗ spawn hợp lệ).
5. Chọn object có `GameManager`: kiểm tra **Player Spawn Position** nằm trong map và không trúng vật cản (mặc định (0, 0) là giữa map).

Để trống ô **Map Bounds** thì game chạy như cũ (map vô hạn).

---

## 5. Giới hạn camera

1. Chọn `CinemachineCamera` → **Add Component → Cinemachine Confiner 2D**.
2. **Bounding Shape 2D** = kéo object `Map` (dùng chính `BoxCollider2D` 150 × 150).
3. Nếu camera giật ở góc map: bấm **Invalidate Bounding Shape Cache** trong Inspector của Confiner sau khi đổi kích thước map.

---

## 6. Kiểm tra

1. Play: Player đi tới mép map thì bị chặn, camera dừng ở mép, không thấy khoảng trống ngoài map.
2. Đi vào vật cản: bị chặn, trượt dọc theo cạnh.
3. Đứng ở góc map, bật **Auto spawn** trong DebugTools: quái chỉ đến từ phía trong map, không xuất hiện ngoài tường hay trong vật cản.
4. Chạy dọc mép map lâu: quái bị bỏ lại phía sau được dịch chuyển lên phía trước, vẫn nằm trong map.
5. Tua đến phút 4 (Swarm) và phút 12 (Line) khi đứng gần mép: đàn Bat và hàng Orc Rider xuất phát trong map, chạm tường bên kia thì biến mất.
6. Tua đến phút 6 (Ring) khi đứng ở góc: vòng Slime bị ép vào trong map, không con nào kẹt ngoài tường.
7. Đo lại FPS với 300 quái (đổi cấu hình vật lý, theo quy tắc 2 của `CLAUDE.md`).
