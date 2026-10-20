# Thiết kế âm thanh

Game: Survival Action kiểu Vampire Survivors (Unity), 2D
Liên quan: `kich-ban-man-choi.md` (luồng scene), `thiet-ke-vu-khi.md` (vũ khí), `huong-dan-am-thanh.md` (cài đặt trong Unity)
Trạng thái: **v1**. Âm lượng, khoảng cách phát là giá trị tạm, chỉnh khi nghe thử.

**Nhật ký thay đổi**
- 2026-10-16 (v1): bản đầu, theo hướng của Vampire Survivors: ít loại tiếng, giới hạn số tiếng phát cùng lúc.

---

## 1. Nguyên tắc

- **Ít loại tiếng, rõ nghĩa.** Mỗi tiếng báo một sự kiện người chơi cần biết (trúng đòn, nhặt đồ, lên cấp, bị thương).
- **Không để màn hình đông thành tiếng ồn.** Mỗi loại tiếng có khoảng cách phát tối thiểu (`minInterval`); trong khoảng đó các lần phát sau bị bỏ. Tổng số tiếng hiệu ứng phát cùng lúc tối đa 16 (`sfxVoices`).
- **Đổi nhẹ cao độ** mỗi lần phát (`pitchVariance`) để tiếng lặp lại không nhàm. Một tiếng có thể có nhiều clip, chọn ngẫu nhiên.
- Âm thanh 2D (không giảm theo khoảng cách).
- Tiếng phát cả khi game dừng (bảng lên cấp, Pause, GameOver) vì dùng thời gian thực.

---

## 2. Nhạc nền

| Scene | Nhạc | Ghi chú |
|---|---|---|
| Đăng nhập, Main Menu, chọn nhân vật, chọn độ khó | Nhạc menu | Chuyển scene giữa các màn này **không phát lại từ đầu** (cùng một clip thì giữ nguyên) |
| Shop vĩnh viễn | Nhạc Shop (hoặc dùng lại nhạc menu) | |
| Trong ván (`UI_InGame`) | Nhạc trận | Lặp suốt ván |
| GameOver | Tắt nhạc, phát tiếng thua | Retry: nhạc trận phát lại từ đầu |

Nhạc boss và tiếng thắng làm cùng boss cuối (mốc 6, trạng thái `Victory`).

---

## 3. Hiệu ứng

| Tiếng (`SoundData`) | Khi nào | Nguồn sự kiện | `minInterval` tạm |
|---|---|---|---|
| `Sfx_EnemyHit` | Quái trúng đòn mà chưa chết | `EnemyManager.OnEnemyDamaged` | 0,06 s |
| `Sfx_EnemyDeath` | Quái chết | `EnemyManager.OnEnemyDied` | 0,05 s |
| `Sfx_PlayerHurt` | Player mất máu | `Player.OnHurt` | 0,25 s |
| `Sfx_GemPickup` | Nhặt gem XP | `PickupManager.OnCollected` | 0,04 s |
| `Sfx_CoinPickup` | Nhặt coin | `PickupManager.OnCollected` | 0,05 s |
| `Sfx_ChestPickup` | Nhặt rương | `PickupManager.OnCollected` | 0 |
| `Sfx_LevelUp` | Mở bảng lên cấp | `LevelUpManager.OnOptionsReady` | 0 |
| `Sfx_ChestOpen` | Mở bảng chọn của rương | `LevelUpManager.OnOptionsReady` (`IsChestReward`) | 0 |
| `Sfx_GameOver` | Thua | `GameManager.OnStateChanged` = `GameOver` | 0 |
| `Sfx_Click` | Bấm nút bất kỳ | `ButtonSounds` gắn vào Canvas | 0 |
| `Sfx_Purchase` / `Sfx_NotEnoughGold` | Mua thành công / thiếu vàng ở Shop | `ShopView` | 0 |
| Tiếng vũ khí | Mỗi lượt đánh | `WeaponData.attackSound`, **để trống thì im lặng** | 0,1 s |

- Vũ khí phần lớn **để im lặng** như Vampire Survivors; chỉ gắn tiếng cho vũ khí chính hoặc vũ khí đánh thưa (Heavy Strike, Lightning Strike).
- Tiếng nào chưa có file thì để trống, game vẫn chạy bình thường.

---

## 4. Âm lượng

- 2 thanh trượt trong Options: **Music** và **SFX**, từ 0 đến 1, mặc định 0,8.
- Lưu bằng `PlayerPrefs` (`MusicVolume`, `SfxVolume`), giữ khi tắt game. Không lưu lên Firestore.
- Âm lượng thực = âm lượng của `SoundData` × thanh trượt.

---

## 5. Cấu trúc code (`Scripts/Audio/`)

| Lớp | Vai trò |
|---|---|
| `SoundData` (ScriptableObject) | `clips[]`, `volume`, `pitchVariance`, `minInterval` |
| `AudioManager` | Singleton sống qua các scene (`DontDestroyOnLoad`). 1 `AudioSource` nhạc + nhóm `sfxVoices` `AudioSource` hiệu ứng dùng xoay vòng. `Play(sound)`, `PlayMusic(music)`, `StopMusic()`, âm lượng Music / SFX |
| `SceneMusic` | Gắn vào mỗi scene, phát nhạc của scene đó lúc bắt đầu |
| `GameAudio` | Trong `UI_InGame`: nghe các sự kiện gameplay ở mục 3 rồi gọi `AudioManager.Play`. Code gameplay không cần biết âm thanh |
| `ButtonSounds` | Gắn vào Canvas: mọi `Button` con phát tiếng click |
| `VolumeSettingsView` | Nối 2 `Slider` trong Options với `AudioManager` |

Prefab `AudioManager` đặt trong **mọi scene**. Scene mở đầu tiên giữ lại bản của nó, các bản sau tự hủy. Nhờ vậy chạy thẳng một scene bất kỳ để test vẫn có tiếng.

---

## 6. Checklist

- [x] `SoundData`, `AudioManager`, `SceneMusic`, `GameAudio`, `ButtonSounds`, `VolumeSettingsView`
- [x] Sự kiện mới: `Enemy.OnDamaged` / `EnemyManager.OnEnemyDamaged`, `Player.OnHurt`, `PickupManager.OnCollected`
- [x] Tiếng vũ khí tùy chọn (`WeaponData.attackSound`), tiếng mua hàng ở Shop
- [ ] Chọn file âm thanh, tạo các asset `SoundData`
- [ ] Nhạc boss và tiếng thắng (cùng boss cuối, mốc 6)
