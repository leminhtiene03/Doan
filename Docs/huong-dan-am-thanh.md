# Hướng dẫn cài đặt âm thanh

Liên quan: `thiet-ke-am-thanh.md` (danh sách tiếng, lúc phát, giới hạn).

Tiếng nào chưa có file thì để trống ô, game vẫn chạy bình thường. Có thể làm từng phần.

---

## 1. File âm thanh

- Đặt file vào `Assets/Audio/Music/` và `Assets/Audio/Sfx/` (tạo thư mục trong Unity).
- Định dạng `.ogg` hoặc `.wav`. Đã có sẵn 6 tiếng click trong `UI/UIPack/Sounds/`.
- Gợi ý gói miễn phí: Kenney (*Interface Sounds*, *Impact Sounds*, *RPG Audio*, *Music Jingles*), OpenGameArt.
- Chọn file trong Project, ở Inspector:
  - Nhạc nền: **Load Type** = `Streaming`.
  - Hiệu ứng ngắn: **Load Type** = `Decompress On Load`.

---

## 2. Tạo `SoundData`

Chuột phải trong `Scripts/Data/` (tạo thư mục `sound`) → **Create → Game → Sound Data**. Mỗi tiếng một asset:

| Asset | Clips | Volume | Pitch Variance | Min Interval |
|---|---|---|---|---|
| `Music_Menu` | nhạc menu | 0,6 | 0 | 0 |
| `Music_Shop` | nhạc Shop (hoặc bỏ, dùng `Music_Menu`) | 0,6 | 0 | 0 |
| `Music_Battle` | nhạc trận | 0,6 | 0 | 0 |
| `Sfx_EnemyHit` | 1–3 tiếng trúng đòn | 0,4 | 0,1 | 0,06 |
| `Sfx_EnemyDeath` | 1–3 tiếng quái chết | 0,5 | 0,1 | 0,05 |
| `Sfx_PlayerHurt` | tiếng bị đánh | 0,8 | 0,05 | 0,25 |
| `Sfx_GemPickup` | tiếng "ting" | 0,4 | 0,1 | 0,04 |
| `Sfx_CoinPickup` | tiếng xu | 0,5 | 0,1 | 0,05 |
| `Sfx_ChestPickup` | tiếng nhặt rương | 0,8 | 0 | 0 |
| `Sfx_LevelUp` | tiếng lên cấp | 0,8 | 0 | 0 |
| `Sfx_ChestOpen` | đoạn nhạc mở rương | 0,9 | 0 | 0 |
| `Sfx_GameOver` | tiếng thua | 0,9 | 0 | 0 |
| `Sfx_Click` | `click-a` / `click-b` | 0,7 | 0,05 | 0 |
| `Sfx_Purchase` | tiếng mua thành công | 0,8 | 0 | 0 |
| `Sfx_NotEnoughGold` | tiếng báo lỗi | 0,8 | 0 | 0 |

Các con số là **giá trị tạm**, chỉnh khi nghe thử.

---

## 3. Prefab `AudioManager`

1. Trong scene `login`: **Create Empty** ở gốc Hierarchy (không nằm trong object nào), tên `AudioManager`.
2. **Add Component → Audio Manager**. **Sfx Voices** = 16, **Default Volume** = 0,8.
3. Kéo vào `Prefab/` để tạo prefab.
4. Kéo prefab này vào **cả 6 scene**: `login`, `Mainmenu`, `Character`, `Difficute`, `Shop`, `UI_InGame`.

Script tự thêm các `AudioSource` lúc chạy, không cần thêm tay. Scene mở đầu tiên giữ lại bản của nó, các scene sau tự hủy bản trùng.

---

## 4. Nhạc nền mỗi scene

Trên một object bất kỳ trong mỗi scene (ví dụ `Canvas`): **Add Component → Scene Music**, ô **Music**:

| Scene | Music |
|---|---|
| `login`, `Mainmenu`, `Character`, `Difficute` | `Music_Menu` |
| `Shop` | `Music_Shop` (hoặc `Music_Menu`) |
| `UI_InGame` | `Music_Battle` |

Các scene dùng chung `Music_Menu` sẽ không phát lại từ đầu khi chuyển qua lại.

---

## 5. Tiếng trong ván (`UI_InGame`)

Trên `Managers` (hoặc object mới tên `GameAudio`): **Add Component → Game Audio**.

| Ô | Gán |
|---|---|
| Game Manager | object có `GameManager` |
| Pickup Manager | object có `PickupManager` |
| Level Up Manager | object có `LevelUpManager` |
| Enemy Hit … Game Over | các `SoundData` tương ứng ở mục 2 |

---

## 6. Tiếng bấm nút

Trên **Canvas** của mỗi scene: **Add Component → Button Sounds**, ô **Click Sound** = `Sfx_Click`. Mọi nút trong Canvas (kể cả panel đang ẩn) đều phát tiếng.

Ô hàng trong Shop sinh ra lúc chạy nên không nhận tiếng click; Shop dùng tiếng mua riêng: trên `Canvas` của scene `Shop`, ở **Shop View**, gán **Purchase Sound** = `Sfx_Purchase`, **Failure Sound** = `Sfx_NotEnoughGold`.

---

## 7. Thanh âm lượng

Trong panel Options (`Prefab/UI_Option`):
1. Thêm 2 **Slider** (UI → Slider), tên `MusicSlider`, `SfxSlider`, **Min** 0, **Max** 1. Thêm nhãn `Music`, `SFX`.
2. Trên gốc `UI_Option`: **Add Component → Volume Settings View**, kéo 2 slider vào **Music Slider** và **Sfx Slider**.

Âm lượng lưu trên máy, giữ nguyên khi tắt game.

---

## 8. Tiếng vũ khí (không bắt buộc)

Mở `WeaponData` của vũ khí, ô **Attack Sound** gán một `SoundData` (**Min Interval** ≥ 0,1). Để trống thì vũ khí im lặng. Chỉ nên gắn cho vũ khí đánh thưa (Heavy Strike, Lightning Strike), vũ khí bắn nhanh mà có tiếng sẽ rất ồn.

---

## 9. Kiểm tra

1. Mở `login` → Play: nhạc menu phát. Đăng nhập → Main Menu → chọn nhân vật → độ khó: nhạc **không bị phát lại từ đầu**.
2. Vào Shop: nhạc đổi (nếu dùng `Music_Shop`). Mua món: tiếng mua; thiếu vàng: tiếng lỗi.
3. Vào ván: nhạc trận. Đánh quái: tiếng trúng đòn, quái chết; 100 quái chết cùng lúc vẫn không vỡ tiếng.
4. Nhặt gem, coin, rương; lên cấp; mở rương bằng nút **Open chest** của DebugTools: mỗi việc một tiếng.
5. Để quái đánh: tiếng bị thương, không lặp liên tục khi bị vây.
6. Chết: nhạc tắt, tiếng thua. Retry: nhạc trận phát lại.
7. Options: kéo thanh Music về 0 thì nhạc tắt ngay; kéo SFX về 0 thì hết tiếng hiệu ứng. Tắt game mở lại: âm lượng giữ nguyên.
8. Chạy thẳng scene `UI_InGame`: vẫn có tiếng (nhờ prefab `AudioManager` có trong scene).
