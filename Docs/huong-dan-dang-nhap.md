# Hướng dẫn cài đặt màn đăng nhập (Firebase)

Liên quan: `thiet-ke-luu-tru.md` mục 5 (cấu trúc Firestore), code trong `Assets/Scripts/Online/`.

---

## 1. Luồng màn hình

```
[Mở game] → scene Login
   ├─ Đã đăng nhập từ trước (khách hoặc email) → vào thẳng Mainmenu
   └─ Chưa → hiện bảng Đăng nhập
        ├─ Đăng nhập (email + mật khẩu)   ── nút "Quên mật khẩu" → Firebase gửi email đặt lại
        ├─ Đăng ký (email, mật khẩu, nhập lại, tên) → Firebase gửi email xác minh
        └─ Chơi khách (nhập tên) → đăng nhập ẩn danh

[Mainmenu]
   ├─ Hiện tên người chơi, nhãn "Khách" nếu là khách
   ├─ Nút "Lưu tài khoản" (chỉ khách) → nhập email + mật khẩu → liên kết vào tài khoản khách, giữ nguyên dữ liệu
   └─ Nút "Đăng xuất" → khách: hỏi xác nhận vì sẽ mất dữ liệu → quay về Login
```

Mỗi người chơi, kể cả khách, có một `uid` và một document `users/{uid}` trên Firestore, được tạo ngay lần đăng nhập đầu tiên.

---

## 2. Cấu hình Firebase Console

### 2.1. Bật phương thức đăng nhập
**Authentication → Sign-in method → Add new provider**:
- **Email/Password**: bật dòng đầu (*Email/Password*). Không cần bật *Email link (passwordless sign-in)*.
- **Anonymous**: bật.

### 2.2. Luật bảo mật Firestore
**Firestore Database → Rules**, thay toàn bộ nội dung bằng đoạn dưới rồi bấm **Publish**:

```
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    match /users/{uid} {
      allow read, delete: if request.auth != null && request.auth.uid == uid;
      allow create, update: if request.auth != null && request.auth.uid == uid
        && (!('accountGold' in request.resource.data)
            || (request.resource.data.accountGold is int && request.resource.data.accountGold >= 0));
      match /runSave/{doc} {
        allow read, write, delete: if request.auth != null && request.auth.uid == uid;
      }
    }
    match /leaderboards/{difficultyId}/entries/{uid} {
      allow read: if true;
      allow create, update: if request.auth != null && request.auth.uid == uid
        && request.resource.data.score is int
        && request.resource.data.score >= 0
        && (resource == null || request.resource.data.score > resource.data.score);
    }
  }
}
```

- Mỗi người chỉ đọc/ghi được dữ liệu của chính mình. Ai cũng đọc được bảng xếp hạng.
- `accountGold` phải là số nguyên không âm (chặn mua khi không đủ vàng, kể cả lúc đồng bộ từ máy khác).
- Luật này thay cho *test mode*. Test mode sẽ tự khóa database sau 30 ngày.

### 2.3. (Tùy chọn) Email gửi cho người chơi
**Authentication → Templates**: chỉnh tiêu đề và nội dung email **xác minh địa chỉ email** và **đặt lại mật khẩu**, ví dụ sang tiếng Việt. Người chơi bấm link trong email sẽ mở trang web của Firebase, không cần quay lại game.

---

## 3. Tạo scene Login

1. **File → New Scene** → lưu thành `Assets/Scenes/login.unity`.
2. **File → Build Profiles** (hoặc Build Settings) → **Add Open Scenes**, rồi **kéo `Login` lên vị trí đầu tiên** (index 0). Game sẽ luôn mở bằng scene này.
3. Thêm **Canvas** (kèm **EventSystem**) và dựng UI như mục 4.
4. Tạo một GameObject trống (ví dụ `LoginController`), gắn component **Login View**, rồi kéo các phần tử UI vào đúng ô.

---

## 4. UI của scene login

Đã dựng trong `Assets/Scenes/login.unity` và đã gán vào component **Login View** (trên object `UIlogin`). Chơi khách nằm ngay trong khung Đăng nhập.

```
UIlogin                     (Login View)
├── LoginPanel
│   ├── email               (TMP Input Field, Email)
│   ├── Password            (TMP Input Field, Password)
│   ├── login               (Button) đăng nhập
│   ├── forgot              (Button) gửi email đặt lại mật khẩu tới địa chỉ trong ô email
│   ├── register            (Button) mở RegisterPanel
│   ├── Name                (TMP Input Field) tên khi chơi khách
│   ├── Guest               (Button) đăng nhập khách
│   └── Notice              (TMP Text) thông báo của khung này
└── RegisterPanel
    ├── email, Name, Password, Password (1)   (4 ô nhập)
    ├── register            (Button) đăng ký
    ├── login               (Button) quay lại LoginPanel
    └── Notice              (TMP Text)
```

- Lúc mở game, LoginPanel hiện ngay, các nút khóa và Notice ghi "Connecting…" cho tới khi kết nối Firebase xong. Đã đăng nhập từ trước thì vào thẳng Mainmenu.
- **Không** gắn sự kiện On Click trong Inspector; script tự gắn.

---

## 5. Khung tài khoản ở Main Menu

Đã dựng trong scene `Mainmenu` và đã gán vào component **Account Panel View** (trên object `User_account`):

```
User_account                (Account Panel View)
└── User                    (Button) chữ con = tên người chơi → bấm để mở/đóng khung chi tiết
UserGuest                   (khung khách, script tự ẩn lúc đầu)
├── InputField (TMP) ×3     email / mật khẩu / nhập lại mật khẩu  → liên kết email
├── save                    (Button) gửi liên kết
├── Cancel                  (Button) đóng khung
├── Cancel (1) "Signout"    (Button) bấm lần 1 hiện dòng warning, bấm lần 2 mới đăng xuất
├── warning                 (TMP Text) cảnh báo mất dữ liệu
└── Text (TMP) "Status"     dòng thông báo
User                        (khung member, script tự ẩn lúc đầu)
├── email_text              (TMP Text) email của tài khoản
├── Button "SignOut"        đăng xuất ngay
└── Button (1) "ChangePass" tạm khóa (chức năng làm sau)
```

| Ô trong Inspector | Đang gán |
|---|---|
| Account Button, Name Text | `User_account/User`, chữ con của nó |
| Guest Panel | `UserGuest` |
| Link Email / Password / Confirm Input | 3 ô nhập trong `UserGuest` |
| Save Account Button, Guest Close Button, Guest Sign Out Button | `save`, `Cancel`, `Cancel (1)` |
| Sign Out Warning, Status Text | `warning`, `Text (TMP)` trong `UserGuest` |
| Member Panel, Email Text | `User`, `email_text` |
| Member Sign Out Button, Change Password Button | `Button`, `Button (1)` trong `User` |

Hành vi:
- Bấm nút tên → mở **`UserGuest` nếu là khách**, **`User` nếu có tài khoản email**. Bấm lần nữa để đóng.
- Khách liên kết email thành công → tự chuyển sang khung `User`, hiện email vừa liên kết.
- Nếu chạy thẳng scene `Mainmenu` khi chưa đăng nhập, script tự chuyển về scene `login`.

---

## 6. Font chữ

Các thông báo do code hiện ra (lỗi, trạng thái) đang viết **tiếng Anh**, gom trong `Scripts/Online/AuthMessages.cs`. Muốn dịch sang tiếng Việt thì chỉ sửa file đó, nhưng phải dùng **TMP Font Asset có đủ ký tự tiếng Việt**. Font mặc định *LiberationSans SDF* thiếu nhiều ký tự có dấu, sẽ hiện ô vuông. Chữ bạn tự gõ trong UI (tên nút…) cũng cần font như vậy.

---

## 7. Kiểm tra

1. Mở scene `login` → Play. Dòng trạng thái hiện "Connecting…", sau đó hiện bảng Đăng nhập.
2. **Chơi khách:** nhập tên 3–16 ký tự → vào Main Menu, tên hiện kèm nhãn "Khách". Trên Firebase Console:
   - **Authentication → Users** có thêm một dòng *(anonymous)*.
   - **Firestore → users** có document mới với `displayName`, `createdAt`, `accountGold = 0`…
3. **Dừng rồi Play lại:** vào thẳng Main Menu, không phải đăng nhập lại.
4. **Lưu tài khoản:** bấm "Lưu tài khoản", nhập email và mật khẩu → thông báo "Account saved". Trên Console, dòng *anonymous* giờ có email, **cùng UID**. Hộp thư nhận email xác minh.
5. **Đăng xuất** → về scene Login. **Đăng nhập** lại bằng email vừa lưu → vào Main Menu, cùng tên cũ.
6. **Đăng ký** một tài khoản mới, và thử **Quên mật khẩu** (nhận email đặt lại).
7. Thử các lỗi: sai mật khẩu, email đã tồn tại, mật khẩu dưới 6 ký tự, 2 ô mật khẩu khác nhau. Mỗi trường hợp phải hiện thông báo tương ứng.

### Lỗi thường gặp

| Hiện tượng | Nguyên nhân | Cách sửa |
|---|---|---|
| Kẹt ở "Connecting…" hoặc báo không kết nối được | Thiếu `Assets/StreamingAssets/google-services-desktop.json`, hoặc không có mạng | Kiểm tra file, kiểm tra mạng, xem Console |
| Đăng nhập khách báo "Something went wrong", Console có `PERMISSION_DENIED` | Chưa publish luật Firestore ở mục 2.2, hoặc chưa tạo Firestore Database | Làm lại mục 2.2 |
| Đăng ký báo lỗi ngay, Console có `OPERATION_NOT_ALLOWED` / `ADMIN_ONLY_OPERATION` | Chưa bật Email/Password hoặc Anonymous | Làm lại mục 2.1 |
| Chữ hiện ô vuông | Font thiếu ký tự | Xem mục 6 |
| Game không mở bằng màn đăng nhập | `Login` chưa ở vị trí đầu Build Settings | Xem mục 3 bước 2 |

---

## 8. Ghi chú kỹ thuật

- `AuthService` và `UserProfileService` là **lớp static** (không phải MonoBehaviour) để dùng được ở mọi scene. Không vi phạm quy tắc "chỉ 4 singleton MonoBehaviour" trong `CLAUDE.md`.
- Firebase tự nhớ phiên đăng nhập trên máy. Vì vậy khách mất dữ liệu nếu đăng xuất, cài lại game hoặc đổi máy mà chưa "Lưu tài khoản".
- Firebase SDK trên **Windows chỉ hỗ trợ cho phát triển** (xem `thiet-ke-luu-tru.md` mục 5.7). Nên build thử bản Windows sớm.
- Bảng xếp hạng và lịch sử ván: xem `huong-dan-bang-xep-hang.md`. Chưa làm: đồng bộ vàng và vũ khí mở khóa lên `users/{uid}` (mốc 7).
