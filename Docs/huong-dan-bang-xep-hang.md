# Hướng dẫn cài đặt bảng xếp hạng (Firebase)

Liên quan: `thiet-ke-luu-tru.md` mục 5 (cấu trúc dữ liệu), `huong-dan-dang-nhap.md` mục 2.2 (luật bảo mật).

---

## 1. Cách hoạt động

- Khi chết, `RunScoreTracker` phát `OnRunFinished`. `RunResultUploader` gửi kết quả lên Firestore bằng **một transaction**:
  - Thêm ván vào đầu `users/{uid}.recentRuns`, chỉ giữ 10 ván gần nhất.
  - Nếu điểm cao hơn điểm cũ ở độ khó đó, ghi đè `leaderboards/{difficultyId}/entries/{uid}`.
- Sau đó đếm số người có điểm cao hơn **ván này** để ra hạng, rồi hiện trên màn GameOver.
- Mỗi độ khó có một bảng riêng. `difficultyId` lấy từ ô `id` của `DifficultyData` (`easy`, `normal`, `hard`).
- Bỏ ván hiện chưa có (sẽ làm ở phần menu Pause) và **không gửi kết quả**.

---

## 2. Firebase Console

1. **Firestore → Rules:** thay khối `match /leaderboard/{uid}` cũ bằng khối mới trong `huong-dan-dang-nhap.md` mục 2.2 (`match /leaderboards/{difficultyId}/entries/{uid}`), rồi bấm **Publish**.
2. Không cần tạo index. Mỗi bảng chỉ sắp theo một trường `score`, Firestore tự tạo index.

---

## 3. Prefab một dòng (dùng chung cho GameOver và Main Menu)

Lấy một dòng `User` có sẵn trong `GameOver/Scroll View/Viewport/Content` làm mẫu:

1. Thêm component **Leaderboard Row View** vào `User`, rồi gán:

| Ô | Gán | Bắt buộc |
|---|---|---|
| Rank Text | `Rank` | có |
| Name Text | `username` | có |
| Score Text | `username (1)` | có |
| Character Image | Image 80×80 đang có hình Swordsman | có |
| Time Text | để trống, hoặc thêm TMP Text nếu muốn hiện thời gian sống `12:34` | không |
| Current User Highlight | một Image nền để tô dòng của mình, hoặc để trống | không |
| Rank Format | `Rank {0:0}` | |
| Score Format | `{0:0} point` | |

Ảnh nhân vật lấy từ ô **Portrait** của `CharacterData`. Bảng xếp hạng chỉ lưu `id` nhân vật, **Leaderboard List View** tra `id` ra `CharacterData` (ô **Characters**, gán một lần cho cả danh sách). Cả 4 asset hiện để trống Portrait: kéo một khung hình từ Tiny RPG pack vào cho mỗi lớp. Lớp không có Portrait thì ô ảnh bị ẩn.

2. Kéo `User` vào thư mục `Prefab/` để tạo prefab, đặt tên `LeaderboardRow`.
3. **Xóa cả 10 dòng `User`** trong `Content`. Script sẽ tự sinh các dòng từ prefab. Grid Layout Group trên `Content` tự xếp chúng. Nếu muốn danh sách cuộn được khi dài, thêm **Content Size Fitter** (Vertical Fit = Preferred Size) vào `Content`.

**Danh sách gồm** top N của độ khó, cộng **1 dòng của bạn ở cuối** nếu bạn không nằm trong top N (hạng thật, ví dụ `Rank 57`). Dòng của bạn là **điểm tốt nhất** ở độ khó đó. Ô `rank` riêng ở màn GameOver hiện hạng của **ván vừa chơi**.

---

## 4. Màn GameOver (scene `UI_InGame`)

1. **Run Result Uploader:** thêm vào object **`Managers`** (nơi có Run Score Tracker). Ô **Score Tracker** kéo chính `Managers` vào. Không gắn vào Scroll View, vì đây là script logic, không có giao diện.
2. **Leaderboard List View:** thêm vào **`GameOver/Scroll View`**, rồi gán:
   - **Characters:** cả 4 asset `CharacterData` (Swordsman, Archer, Mage, Guardian).
   - **Row Prefab:** `LeaderboardRow`.
   - **Row Container:** `Content`.
   - **Top Count:** số dòng muốn hiện, ví dụ `10`.
   - **Show Current User Row:** bật.
   - **Status Text:** để trống, hoặc tạo một TMP Text cho chữ `Loading...`.
3. **Game Over View** (trên `Canvas`), gán thêm:
   - **Result Uploader:** `Managers`.
   - **Rank Text:** text **`rank`** (chữ thường, đang ghi "Rank 100") nằm trực tiếp trong `GameOver`. Không gán các text `Rank` bên trong dòng.
   - **Leaderboard List:** `Scroll View`.
4. Ô **Score Text** giữ nguyên là text `Điểm`. Phần tính và hiện điểm đã có từ trước.
5. Component **Run Score Tracker** không còn ô **Run Selection**. Độ khó giờ lấy từ `GameManager`, nên chạy thẳng scene vẫn dùng đúng **Default Difficulty**.

Ô `rank` lần lượt hiện:

| Lúc | Chữ |
|---|---|
| Vừa chết | `Saving...` |
| Gửi xong | `Rank 5`, hoặc `Rank 5  New best!` nếu là điểm cao nhất của bạn ở độ khó này |
| Mất mạng | `Offline - result will sync later`. Kết quả và vàng tài khoản được ghi chờ, có mạng thì tự gửi |
| Bị từ chối, chưa đăng nhập | `Could not save result` (chi tiết trong Console). Danh sách vẫn được tải |

Gửi xong (hoặc lỗi) thì Scroll View tải danh sách của độ khó vừa chơi, nên điểm mới của bạn có mặt ngay.

---

## 5. Bảng xếp hạng ở Main Menu (scene `Mainmenu`)

```
LeaderboardButton           (Button) mở bảng, đặt cạnh New Game
LeaderboardPanel            (script tự ẩn lúc đầu)
├── Tabs
│   ├── EasyTab             (Button)
│   ├── NormalTab           (Button)
│   └── HardTab             (Button)
├── Scroll View             (Leaderboard List View)
│   └── Viewport/Content    (Vertical/Grid Layout Group + Content Size Fitter)
├── Status                  (TMP Text) Loading... / No results yet. / lỗi
└── Close                   (Button)
```

1. Trên `Scroll View`, thêm **Leaderboard List View**, gán như mục 4 bước 2. Top Count tùy ý, ví dụ `50`. Status Text gán `Status`.
2. Thêm **Leaderboard View** lên một object luôn bật (ví dụ Canvas), **không đặt lên chính `LeaderboardPanel`**:

| Ô | Gán |
|---|---|
| Open Button / Close Button | `LeaderboardButton` / `Close` |
| Panel Root | `LeaderboardPanel` |
| List | `Scroll View` |
| Tabs (3 phần tử) | mỗi phần tử: nút tab + asset `DifficultyData` tương ứng (Easy, Normal, Hard) |
| Default Tab Index | `1` (Normal) |

**Bục vinh danh top 3 (chỉ ở Main Menu):** ô **Podium** của Leaderboard List View nhận các bục theo thứ tự hạng 1, 2, 3. Người đứng hạng 1–3 lên bục, Scroll View bắt đầu từ hạng 4. Top Count vẫn là tổng số người, tính cả bục. Màn GameOver để trống ô Podium nên vẫn hiện đủ danh sách. Mỗi bục cần một object con (ví dụ `Winner`) gắn **Leaderboard Row View** với Name Text, Score Text, Character Image. Rank Text có thể để trống. Script ẩn/hiện object con này khi bục chưa có người, còn hình bục luôn hiện.

Tab đang chọn bị làm mờ (`interactable = false`), nên chỉnh **Disabled Color** của nút tab thành màu "đang chọn". Dòng được sinh một lần (Top Count + 1 dòng) rồi dùng lại.

---

## 6. Kiểm tra

1. Đăng nhập → New Game → chọn lớp và độ khó **Normal** → chơi tới khi chết (hoặc bật DebugTools rồi tắt bất tử).
2. Màn GameOver: ô `rank` hiện `Saving...` rồi `Rank 1  New best!`. Scroll View hiện 1 dòng của bạn.
3. Firebase Console → Firestore:
   - `leaderboards/normal/entries/{uid}` có `displayName`, `score`, `characterId`, `runTime`, `kills`, `level`, `victory`, `achievedAt`.
   - `users/{uid}.recentRuns` có 1 phần tử.
4. Chơi lại Normal với điểm **thấp hơn**: ô `rank` hiện `Rank 2` (không có New best), danh sách vẫn chỉ có 1 dòng của bạn với điểm cũ, `entries/{uid}` giữ điểm cũ, `recentRuns` có 2 phần tử, ván mới đứng đầu.
5. Chơi một ván **Easy**: xuất hiện `leaderboards/easy/entries/{uid}` riêng.
6. Main Menu → mở bảng xếp hạng: tab Normal mở sẵn, dòng của bạn được tô nổi bật. Chuyển tab Easy / Hard. Hard hiện `No results yet.`
7. Chơi 11 ván: `recentRuns` vẫn chỉ có 10 phần tử.
8. Tắt mạng rồi chết: ô `rank` hiện `Offline - result will sync later` (sau tối đa vài giây), danh sách báo lỗi tải, game không treo. Bật mạng lại: `recentRuns` và `accountGold` trên Console được cập nhật.
9. Đặt Top Count = 1 và có ít nhất 2 người chơi: người hạng 2 thấy dòng top 1 và dòng của mình ở dưới.
10. **Build Windows** rồi lặp lại bước 1–6 trên bản build. Firebase SDK cho desktop chỉ hỗ trợ chính thức lúc phát triển, nên đây là bước quan trọng.

### Lỗi thường gặp

| Hiện tượng | Nguyên nhân | Cách sửa |
|---|---|---|
| `Could not save result`, Console có `PERMISSION_DENIED` | Chưa publish luật mới ở mục 2 | Làm lại mục 2 |
| Bảng hiện `Could not load the leaderboard.` | Luật chưa cho đọc `leaderboards`, hoặc không có mạng | Như trên, kiểm tra mạng |
| Có điểm nhưng nằm ở bảng `Normal` viết hoa | Bản cũ dùng id mặc định `"Normal"` | Đã sửa trong `RunScoreTracker`. Xóa document thừa trên Console |
