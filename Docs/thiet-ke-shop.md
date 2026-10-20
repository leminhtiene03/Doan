# Thiết kế Shop & vàng

Game: Survival Action kiểu Vampire Survivors (Unity), 2,5D
Liên quan: `kich-ban-man-choi.md`, `thiet-ke-vu-khi.md` (vũ khí, vật phẩm), `thiet-ke-quai.md` (tỉ lệ rơi vàng, rương), `thiet-ke-luu-tru.md` mục 5 (Firestore)
Trạng thái: **v3**. Chỉ còn **một Shop vĩnh viễn**, không có Shop trong ván. Giá là bản nháp, chỉnh sau khi đo lượng vàng thực tế.

**Nhật ký thay đổi**
- 2026-10-15 (v2): vàng tài khoản tính theo điểm cuối ván. Thêm Shop vĩnh viễn (sau màn chọn độ khó) bán mở khóa vũ khí, vật phẩm mang sẵn và chỉ số vĩnh viễn. Ghi Firestore chạy được khi mất mạng.
- 2026-10-16 (v3): **bỏ Shop trong ván** (phút 10/20, reroll, bỏ đồ, bình máu, Sách/Túi/Ngọc/Rương bí ẩn) để đơn giản cấu trúc, giống Vampire Survivors. Coin nhặt trong ván **cộng vào vàng tài khoản** cùng vàng theo điểm. Chỉ số vĩnh viễn cộng thẳng.

Quy ước: tên class, hàm, biến trong code viết bằng tiếng Anh.

---

## 1. Vàng

Chỉ có **một loại vàng có giá trị lâu dài: vàng tài khoản** (`accountGold`), dùng ở Shop vĩnh viễn (mục 3).

| Nguồn | Lượng |
|---|---|
| Coin nhặt trong ván | Toàn bộ số coin đã nhặt (đã nhân `Greed`) |
| Điểm cuối ván | `floor(điểm / scorePerAccountGold)` (`ScoreConfig`, tạm 100) |

Vàng tài khoản cuối ván = **coin đã nhặt + vàng theo điểm**.

- Coin rơi xuống đất, người chơi phải đi nhặt (tự hút khi vào bán kính `Magnet`). Coin chưa nhặt khi ván kết thúc thì mất.
- Trong ván, HUD hiện số coin đã nhặt (`PlayerProgression.Gold`). Không tiêu được trong ván.

| Kết thúc ván | Vàng tài khoản |
|---|---|
| Thắng / chết | Coin + vàng theo điểm, kể cả khi **mất mạng** (ghi chờ, có mạng thì tự đồng bộ) |
| Bỏ ván (thoát từ menu Pause) | 0 |

- Tài khoản khách nhận vàng như tài khoản email.
- Ước tính vàng theo điểm với tỉ lệ 100: chết phút 10 ≈ 85, chết phút 20 ≈ 200, thắng ≈ 380. Coin cần **đo khi test** (ghi log tổng coin ở phút 10, 20, 30) rồi chỉnh giá Shop.

---

## 2. Hệ thống Shop

Mọi món hàng là một `ShopItemData` và được tính bằng **số lần đã mua**:

| Món | `maxPurchases` | Số lần đã mua nghĩa là |
|---|---|---|
| Mở khóa vũ khí | 1 | 1 = đã mở |
| Chỉ số vĩnh viễn | 1–5 | Cấp nâng |
| Vật phẩm mang sẵn | 1 | Đã sở hữu |

Logic (`ShopSession`):
- **Giá** = `basePrice + pricePerPurchase × số lần đã mua`.
- **Mua được** khi chưa đạt `maxPurchases` (0 = không giới hạn) và đủ vàng.
- **Mua:** trừ vàng (`AccountProgress`) → tăng số lần đã mua (`ShopLedger`) → phát `OnPurchased` → ghi Firestore.
- Lượt mua tải từ `users/{uid}.purchases`, áp dụng lúc bắt đầu ván (`GameManager`).

Giao diện: `ShopView` + prefab ô hàng `ShopSlotView`. Bấm món: `Purchase successful!` hoặc `Not enough gold!`.

---

## 3. Shop vĩnh viễn

**Luồng:** Chọn nhân vật → Chọn độ khó → **Shop vĩnh viễn** (scene `Shop`) → Start → vào ván. Shop luôn mở được, kể cả khi mất mạng (đọc bản đệm của Firestore).

Hiện toàn bộ mặt hàng, không reroll. Asset nằm ở `Scripts/Data/shop/`, danh sách trong `ShopConfig_Permanent`. **Giá và chỉ số là giá trị tạm.**

| Nhóm | Món (`id`) | Hiệu ứng mỗi lần mua | Giá | Tối đa |
|---|---|---|---|---|
| Chỉ số | `perm_might` | +5% Might | 50 + 25 × đã mua | 5 |
| | `perm_maxhp` | +10 Max HP | 40 + 20 × đã mua | 5 |
| | `perm_armor` | +1 Armor | 60 + 30 × đã mua | 3 |
| | `perm_movespeed` | +3% Move Speed | 40 + 20 × đã mua | 5 |
| | `perm_growth` | +5% Growth | 50 + 25 × đã mua | 5 |
| | `perm_greed` | +10% Greed | 40 + 20 × đã mua | 5 |
| | `perm_revival` | +1 Revival | 300 | 1 |
| Vật phẩm mang sẵn | `start_giant_heart`, `start_regen_ring`, `start_wind_boots` | Bắt đầu mọi ván với vật phẩm đó cấp 1 (chiếm 1 ô) | 80 | 1 |
| Mở khóa vũ khí | `unlock_boomerang`, `unlock_caltrops` | Vũ khí xuất hiện trong thẻ lên cấp và rương | 100 | 1 |
| | `unlock_orbiting_orbs`, `unlock_poison_flask` | Như trên | 150 | 1 |

- Mọi chỉ số vĩnh viễn **cộng thẳng** (`Flat`), giống nhau cho mọi lớp. Chỉ số dạng hệ số (Might, Move Speed, Growth, Greed) cộng thẳng vào hệ số: +5% Might = Might +0.05 (100% → 105%, Guardian 90% → 95%).
- Vật phẩm mang sẵn chỉ gồm 3 vật phẩm hỗ trợ có trong danh sách của **cả 4 lớp**.
- Vũ khí **chưa mở khóa không xuất hiện** ở đâu. Vũ khí khởi đầu, vật phẩm bị động, thẻ chỉ số luôn mở sẵn.
- Món là vũ khí bị khóa khi có một `ShopItemData` loại `UnlockWeapon` trỏ tới nó trong `ShopConfig_Permanent` và chưa mua. Muốn khóa thêm vũ khí: tạo thêm món, không cần sửa code.
- **Id phải cố định.** Lượt mua lưu theo `id`; đổi id thì người chơi mất món đó.
- Chạy thẳng scene `UI_InGame` (không qua Shop) thì không có bonus vĩnh viễn và mọi vũ khí đều mở, để tiện test.
- Điểm bảng xếp hạng **bị ảnh hưởng** bởi chỉ số vĩnh viễn (người chơi lâu năm mạnh hơn). Chấp nhận.

---

## 4. Dữ liệu

**`ShopItemData`** (ScriptableObject)
- `id`, `displayName`, `description`, `icon` (để trống thì lấy từ vũ khí / vật phẩm được trỏ tới)
- `kind`: `Stat` / `Passive` / `UnlockWeapon`
- `stat`, `modifierKind`, `value` (loại `Stat`); `passive` (loại `Passive`); `weapon` (loại `UnlockWeapon`)
- `basePrice`, `pricePerPurchase`, `maxPurchases` (0 = không giới hạn)

**`ShopConfig`** (ScriptableObject): `items`.

**Firestore `users/{uid}`**: `accountGold` (số), `purchases` (map `id → số lần đã mua`). Xem `thiet-ke-luu-tru.md` mục 5.

---

## 5. Checklist

- [x] Hệ thống Shop: `ShopItemData`, `ShopConfig`, `ShopLedger`, `ShopSession`, `IGoldWallet`, `ShopView`, `ShopSlotView`
- [x] Vàng tài khoản = coin + vàng theo điểm, ghi cùng kết quả ván, chạy được khi mất mạng
- [x] Shop vĩnh viễn sau màn chọn độ khó, lưu lượt mua lên Firestore
- [x] Áp dụng chỉ số và vật phẩm mang sẵn khi bắt đầu ván, lọc vũ khí chưa mở khóa khỏi thẻ lên cấp
- [ ] Ghi log coin ở phút 10, 20, 30 khi test để chỉnh giá

---

## 6. Đã chốt

- **Không có Shop trong ván.** Nâng cấp trong ván chỉ đến từ lên cấp và rương.
- Vàng tài khoản = **coin nhặt được + vàng theo điểm**. Bỏ ván không nhận gì. Khách nhận như thường. Mất mạng vẫn nhận (đồng bộ sau).
- Shop vĩnh viễn bán **mở khóa vũ khí, vật phẩm mang sẵn, chỉ số vĩnh viễn**, đặt ngay sau màn chọn độ khó.
- Chỉ số vĩnh viễn cộng thẳng, giống nhau cho mọi lớp.

---

## Ghi chú: danh sách theo lớp

Thẻ lên cấp và phần thưởng trong rương chỉ lấy từ danh sách của lớp đang chơi (`CharacterData.weaponPool`, `CharacterData.passivePool`; xem `thiet-ke-vu-khi.md` mục 1). Thẻ chỉ số dùng chung cho cả 4 lớp.
