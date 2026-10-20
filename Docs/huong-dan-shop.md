# Hướng dẫn cài đặt Shop vĩnh viễn

Liên quan: `thiet-ke-shop.md` mục 2 (hệ thống) và mục 3 (mặt hàng), `thiet-ke-luu-tru.md` mục 5 (Firestore).

Gồm: hệ thống Shop, Shop vĩnh viễn, vàng tài khoản (coin nhặt được + vàng theo điểm). Không có Shop trong ván.

---

## 1. Firebase Console

**Firestore → Rules:** thay khối `match /users/{uid}` bằng khối mới trong `huong-dan-dang-nhap.md` mục 2.2, rồi bấm **Publish**. Luật mới chặn `accountGold` âm.

Không cần sửa dữ liệu cũ. Tài khoản chưa có `purchases` được coi là chưa mua gì. Ô `unlockedWeaponIds` cũ không còn dùng, để nguyên hoặc xóa đều được.

---

## 2. Asset có sẵn

Đã tạo trong `Scripts/Data/shop/`:
- 14 asset `ShopItemData`: `Perm_*` (chỉ số), `Start_*` (vật phẩm mang sẵn), `Unlock_*` (mở khóa vũ khí).
- `ShopConfig_Permanent`: danh sách 14 món theo thứ tự hiển thị.

Mở Unity và chờ biên dịch xong, rồi bấm vào `ShopConfig_Permanent` để kiểm tra danh sách đủ 14 món. Nếu ô nào báo *Missing*, báo lại cho Claude.

Món `Perm_*` chưa có icon: kéo icon vào ô **Icon**. Món `Start_*` và `Unlock_*` để trống thì tự lấy icon của vật phẩm / vũ khí.

---

## 3. Prefab một ô hàng `ShopSlot`

```
ShopSlot              (Button + Image nền, Shop Slot View)
├── Icon              (Image)
├── Name              (TMP Text)
├── Description       (TMP Text)
├── Level             (TMP Text, không bắt buộc) "Lv 2/5"
└── Price             (TMP Text) "75" / "SOLD OUT" / "Owned"
```

Gán **Shop Slot View**:

| Ô | Gán |
|---|---|
| Buy Button | chính `ShopSlot` |
| Icon Image | `Icon` |
| Name Text / Description Text / Price Text | các text tương ứng |
| Details Text | TMP Text mô tả chức năng vật phẩm / vũ khí (lấy từ `PassiveItemData` / `WeaponData`). Tự ẩn với món chỉ số. Không bắt buộc |
| Level Text | `Level`, hoặc để trống |
| Price Format | `{0:0}` (hoặc `{0:0} G`) |
| Level Format | `Lv {0:0}/{1:0}` |

Không đủ vàng: nút vẫn bấm được (có highlight), giá hiện màu đỏ (**Unaffordable Price Color**); đủ vàng thì giữ màu gốc của text `Price`, bấm vào thì hiện thông báo. Đã mua tối đa: nút bị làm mờ, chỉnh **Disabled Color** của Button cho dễ nhìn. Kéo `ShopSlot` vào `Prefab/` rồi xóa khỏi scene.

---

## 4. Scene `Shop`

1. **File → New Scene** → lưu thành `Assets/Scenes/Shop.unity` (tên phải đúng `Shop`).
2. **File → Build Profiles → Add Open Scenes**.
3. Dựng UI:

```
Canvas                    (Shop View, Permanent Shop View)
├── Gold                  (TMP Text) số vàng tài khoản
├── Status                (TMP Text) Loading... / lỗi tải
├── Scroll View
│   └── Viewport/Content  (Grid Layout Group + Content Size Fitter: Vertical = Preferred Size)
├── StartButton           (Button) "Start"
└── BackButton            (Button) "Back"
```

4. **Shop View** (trên Canvas):

| Ô | Gán |
|---|---|
| Slot Prefab | `ShopSlot` |
| Slot Container | `Content` |
| Gold Text | `Gold` |
| Message Text | một TMP Text nổi giữa màn hình (ví dụ `Message`), hiện `Not enough gold!` (màu **Failure Color**) hoặc `Purchase successful!` (màu **Success Color**) trong 2 giây. Script tự ẩn lúc đầu |

5. **Permanent Shop View** (trên Canvas):

| Ô | Gán |
|---|---|
| Run Selection | asset `RunSelection` (cùng asset mà Character / Difficulty đang dùng) |
| Catalog | `ShopConfig_Permanent` |
| Shop View | `Canvas` |
| Start Button / Back Button | `StartButton` / `BackButton` |
| Status Text | `Status` |

Script tự sinh các ô hàng. Nút Start bị khóa đến khi tải xong tài khoản.

---

## 5. Scene `UI_InGame`

1. **GameManager** (trên `Managers`): ô **Permanent Shop** gán `ShopConfig_Permanent`. Để trống thì ván không nhận bonus vĩnh viễn.
2. **Game Over View** (trên `Canvas`): ô **Account Gold Text** gán một TMP Text mới trong `GameOver` (hiện `+85 gold`). Không bắt buộc.
3. **Score Config** (`Prefab/ScoreConfig`): ô **Score Per Account Gold** mặc định `100`.

---

## 6. Kiểm tra

1. Đăng nhập → New Game → chọn lớp → chọn độ khó → vào scene `Shop`. Thấy `Loading...` rồi danh sách 14 món, vàng `0`, mọi giá màu đỏ. Bấm một món: hiện `Not enough gold!`.
2. Chơi tới khi chết (ví dụ ~2 phút). GameOver hiện `+N gold`. Firestore Console: `users/{uid}.accountGold` = N, phần tử đầu của `recentRuns` có `accountGold`.
3. Để test mua nhanh: sửa tay `accountGold` = 1000 trên Console.
4. Vào lại Shop: vàng = 1000. Mua `Power Training` hai lần: giá 50 → 75 → 100, `Lv 2/5`, vàng còn 875. Console: `purchases.perm_might = 2`, `accountGold = 875`.
5. Mua `Unlock_Boomerang`: ô hiện `Owned`. Mua `Start_GiantHeart`.
6. Start: vào ván với Swordsman, mở DebugTools / bảng chỉ số: Might cao hơn 10%, đã có Giant Heart cấp 1. Lên cấp: Boomerang có thể xuất hiện; Caltrops (chưa mở) **không bao giờ** xuất hiện.
7. Retry ở GameOver: ván mới vẫn có bonus.
8. **Mất mạng:** tắt mạng → vào Shop: vẫn tải được (bản đệm), mua được. Chết: GameOver hiện `Offline - result will sync later`. Bật mạng: Console cập nhật vàng và lượt mua.
9. Sửa tay `accountGold` = 10 trên Console khi máy đang offline với số dư cũ lớn, mua món 50 rồi bật mạng: lệnh bị từ chối, Console có cảnh báo `Purchase ... was rejected`. Vào lại Shop thấy món đó chưa mua.
10. Chạy thẳng scene `UI_InGame`: không có bonus, mọi vũ khí đều mở.

### Lỗi thường gặp

| Hiện tượng | Nguyên nhân | Cách sửa |
|---|---|---|
| Chọn độ khó báo scene `Shop` không có trong build | Chưa thêm vào Build Profiles | Mục 4 bước 2 |
| Mua xong, Console báo `PERMISSION_DENIED` | Chưa publish luật mới | Mục 1 |
| Status hiện `Could not load your account...` | Chưa đăng nhập, hoặc chưa từng có mạng | Đăng nhập lại khi có mạng |
| Vào ván không có bonus | Chưa gán Permanent Shop trên GameManager | Mục 5 bước 1 |
