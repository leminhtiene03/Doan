# Hướng dẫn cài đặt và gán asset cho vũ khí

Tài liệu này hướng dẫn cách **thay hình tạm, gán icon, chỉnh thông số** cho 4 vũ khí khởi đầu, 8 vũ khí nhận thêm và 8 bản tiến hóa, cùng cách **tạo vũ khí mới**. Mọi thao tác đều làm trong Unity Editor, không cần sửa code.

Liên quan: `thiet-ke-vu-khi.md` (luật chơi và con số), `viec-can-lam-sau.md` (danh sách hình tạm cần thay).

---

## 1. Cấu trúc của một vũ khí

Mỗi vũ khí gồm 2–3 phần:

| Phần | Vai trò | Ví dụ (Dao găm) |
|---|---|---|
| **`WeaponData`** (file `.asset`) | Tên, mô tả, icon, kiểu ngắm, loại sát thương, **thông số 5 cấp**, bản tiến hóa | `Scripts/Data/weapon data/WeaponData_ThrowingDagger.asset` |
| **Prefab vũ khí** | Object vô hình gắn vào Player, chứa script hành vi (`ProjectileWeapon`, `ZoneWeapon`…) | `Scripts/Data/weapon data/ThrowingDaggerWeapon.prefab` |
| **Prefab hình ảnh** (tùy vũ khí) | Thứ người chơi nhìn thấy: đạn, vùng trên mặt đất, hiệu ứng | `Prefab/Dagger.prefab` |

**Quy tắc vàng:** muốn **đổi hình** thì sửa **prefab hình ảnh**. Muốn **đổi số** (sát thương, nhịp, tầm…) thì sửa **`WeaponData`**. Muốn **đổi icon, tên** thì sửa **`WeaponData`**.

> ⚠️ **Bản tiến hóa dùng chung prefab hình ảnh với vũ khí gốc** (trừ Kiếm hồn). Ví dụ đổi sprite của `Prefab/Dagger.prefab` thì cả Dao găm lẫn Vũ điệu ngàn dao đều đổi. Muốn bản tiến hóa có hình riêng, xem mục 6.3.

---

## 2. Chuẩn import sprite (bắt buộc)

Mọi sprite vũ khí phải import theo chuẩn của project, nếu không sẽ to nhỏ lệch nhau hoặc bị nhòe:

1. Chọn file ảnh trong cửa sổ Project. Ở Inspector, đặt:
   - **Texture Type:** Sprite (2D and UI)
   - **Sprite Mode:** Single (một hình) hoặc Multiple (nhiều khung, cắt bằng Sprite Editor)
   - **Pixels Per Unit:** **24**
   - **Filter Mode:** **Point (no filter)**
   - **Compression:** **None**
2. Bấm **Apply**.
3. **Hướng vẽ:** đạn bay (mũi tên, dao, kiếm, mũi băng) phải **vẽ hướng sang phải**. Code xoay hình theo hướng bay, nên vẽ hướng khác sẽ bay ngang hoặc bay ngược.
4. **Pivot:** để **Center**, trừ tia sét (xem mục 4.6) dùng **Bottom**.
5. **Kích thước tham khảo** ở PPU 24: 24 px = 1 đơn vị. Nhân vật cao khoảng 20 px. Đạn nên dài 12–20 px. Vùng trên mặt đất (chông, vũng độc) nên vẽ **24 × 24 px**, vì code phóng to theo bán kính với quy ước "ảnh rộng 1 đơn vị = đường kính 1".

---

## 3. Thao tác chung

### 3.1. Thay sprite cho prefab hình ảnh
1. Mở prefab (nhấp đúp) trong `Assets/Prefab/`.
2. Chọn object gốc, ở component **Sprite Renderer** kéo sprite mới vào ô **Sprite**.
3. Nếu hình mới to hoặc nhỏ hơn hình cũ nhiều, chỉnh **collider** (Capsule hoặc Circle Collider 2D) cho ôm vừa hình. Riêng vùng mặt đất và quả cầu thì không có collider (dùng bán kính trong `WeaponData`).
4. Lưu prefab (Ctrl+S trong Prefab Mode).

### 3.2. Đạn có animation nhiều khung
Prefab đạn (component **Projectile**) có ô **Flight Frames**:
1. Cắt sprite sheet bằng Sprite Editor (Sprite Mode = Multiple).
2. Kéo **các khung theo thứ tự** vào danh sách **Flight Frames**.
3. **Frame Rate** = số khung mỗi giây (mặc định 12).
4. Để trống thì đạn dùng hình tĩnh trong Sprite Renderer.

Ví dụ có sẵn: `Prefab/ArcaneBolt.prefab` dùng 4 khung cầu lửa của Wizard.

### 3.3. Hiệu ứng có animation nhiều khung
Prefab hiệu ứng (component **Pooled Effect**) có:
- **Lifetime:** thời gian hiện (giây).
- **Frames:** các khung, phát trải đều trong thời gian Lifetime. Để trống thì dùng hình tĩnh.

Ví dụ có sẵn: `Prefab/ArcaneImpact.prefab` (3 khung nổ).

### 3.4. Gán icon và tên hiển thị
1. Chọn file `WeaponData_*.asset`.
2. Kéo sprite icon vào ô **Icon**. Icon hiện trên thẻ lên cấp.
   - Nếu **Icon để trống**, thẻ lên cấp tự lấy hình của vũ khí theo thứ tự: sprite của prefab đạn → sprite của prefab hiệu ứng → hình vùng (chông, vũng độc), quả cầu hoặc lưỡi kiếm hồn. Icon thật luôn được ưu tiên khi đã gán.
3. Sửa **Display Name** và **Description** nếu muốn. Lưu ý font: font TMP mặc định thường thiếu ký tự tiếng Việt có dấu. Nếu dùng tiếng Việt, kiểm tra thẻ lên cấp có hiện đủ chữ không.

### 3.5. Chỉnh thông số theo cấp
Trong `WeaponData`, mục **Levels** có 5 phần tử (bản tiến hóa chỉ có 1). Ý nghĩa từng ô:

| Ô | Ý nghĩa | Dùng bởi |
|---|---|---|
| Damage | Sát thương mỗi lần trúng | Tất cả |
| Attack Interval | Nhịp đánh (giây). Quả cầu: độ dài **cả chu kỳ** bật + tắt. Kiếm hồn: thời gian nghỉ của mỗi kiếm | Tất cả |
| Size | Hệ số kích thước hình (nhân thêm chỉ số Area). Kiếm chém: tầm chém | Đạn, quả cầu, kiếm chém |
| Projectiles | Số đạn / tia sét / quả cầu / vùng / kiếm mỗi lượt (cộng thêm Multishot) | Đa số |
| Arc Angle | Góc xòe giữa các đạn (độ). Kiếm chém: góc vòng cung | Đạn, kiếm chém |
| Knockback | Lực đẩy lùi | Tất cả |
| Projectile Speed | Tốc độ đạn. Quả cầu: **tốc độ xoay (độ/giây)**. Kiếm hồn: tốc độ lao | Đạn, quả cầu, kiếm hồn |
| Range | Tầm bay. NearestEnemy: bán kính tìm quái. Quả cầu: **bán kính quỹ đạo**. Bình độc: tầm ném. Bão sấm: tầm nảy | Đa số |
| Pierce | Số quái xuyên thêm (999 = xuyên mọi quái) | Đạn |
| Impact Radius | Bán kính nổ / vùng / va chạm của quả cầu và kiếm hồn | Tia phép, tia sét, vùng, quả cầu |
| Burst Interval | > 0 thì đạn bắn **lần lượt** cách nhau bấy nhiêu giây thay vì xòe quạt | Dao găm |
| Slow Amount / Slow Duration | Làm chậm khi trúng (0,3 = 30%; 1 = đứng im) | Băng trùy, Bãi chông gai, Bão tuyết |
| Duration | Thời gian tồn tại của vùng / thời gian bật của quả cầu | Vùng, quả cầu |
| Chain Count | Số lần sét nảy | Bão sấm |
| Pull Strength | Lực hút về tâm vùng | Đầm lầy chết chóc |
| Upgrade Description | Mô tả hiện trên thẻ khi lên cấp này | Tất cả |

Ô không dùng tới thì để 0.

---

## 4. Từng vũ khí

Đường dẫn viết tắt: **`WD/`** = `Scripts/Data/weapon data/`, **`P/`** = `Prefab/`, **`PH/`** = `Art/Placeholder/` (hình tạm cần thay).

### 4.1. Vũ khí khởi đầu

| Vũ khí | Lớp | `WeaponData` | Prefab vũ khí | Hình ảnh |
|---|---|---|---|---|
| Kiếm chém | Swordsman | `WD/WeaponData.asset` | `Scripts/Data/SwordSlashWeapon.prefab` | Không có prefab hình; đòn đánh thể hiện bằng **animation Attack** của nhân vật. Có thể gán vệt chém vào ô **Effect Prefab** (một `PooledEffect`) |
| Cung gỗ | Archer | `WD/WeaponData_WoodenBow.asset` | `Scripts/Data/WoodenBowWeapon.prefab` | `P/Arrow.prefab` (sprite thật của Tiny RPG) |
| Tia phép | Mage | `WD/WeaponData_ArcaneBolt.asset` | `Scripts/Data/ArcaneBoltWeapon.prefab` | `P/ArcaneBolt.prefab` (đạn, 4 khung) + `P/ArcaneImpact.prefab` (nổ, 3 khung) |
| Trọng kích | Guardian | `WD/WeaponData_HeavyStrike.asset` | Dùng chung `SwordSlashWeapon.prefab` | Animation Attack của Knight Templar. Có thể gán vệt chém vào **Effect Prefab** |

Vũ khí khởi đầu có **Play Owner Attack Anim** bật: mỗi lần đánh, nhân vật phát animation Attack.

### 4.2. Dao găm → Vũ điệu ngàn dao
| | Đường dẫn |
|---|---|
| Data | `WD/WeaponData_ThrowingDagger.asset` → tiến hóa `WD/WeaponData_ThousandBlades.asset` |
| Prefab vũ khí | `WD/ThrowingDaggerWeapon.prefab` (script `ProjectileWeapon`) |
| **Cần thay** | `P/Dagger.prefab` ← hình tạm `PH/Dagger.png` |

Gợi ý asset: dao rời trong *0x72 DungeonTileset II*. Vẽ mũi dao hướng sang phải.

### 4.3. Boomerang → Nguyệt luân
| | Đường dẫn |
|---|---|
| Data | `WD/WeaponData_Boomerang.asset` → `WD/WeaponData_LunarWheel.asset` |
| Prefab vũ khí | `WD/BoomerangWeapon.prefab` (`ProjectileWeapon`) |
| **Cần thay** | `P/Boomerang.prefab` ← `PH/Boomerang.png` |

Trong prefab đạn, component **Projectile** đặt **Motion = Boomerang**, **Spin Speed = 900** (độ/giây, tự xoay khi bay). Hình boomerang **không cần** vẽ hướng phải vì nó xoay liên tục. Collider là **Circle**, bán kính 0,3.

### 4.4. Kiếm bay → Kiếm hồn
| | Đường dẫn |
|---|---|
| Data | `WD/WeaponData_SpiritSword.asset` → `WD/WeaponData_SoulBlades.asset` |
| Prefab vũ khí | `WD/SpiritSwordWeapon.prefab` (`ProjectileWeapon`); bản tiến hóa dùng `WD/SoulBladesWeapon.prefab` (script `SoulBlades`) |
| **Cần thay** | `P/SpiritSword.prefab` (đạn của Kiếm bay) và `P/SoulBlade.prefab` (kiếm xoay của Kiếm hồn) ← cả hai đang dùng `PH/SpiritSword.png` |

- Prefab đạn Kiếm bay: **Motion = Homing**, **Turn Rate = 540** (độ/giây, càng lớn càng bám sát).
- `SoulBladesWeapon.prefab` có ô **Orbit Radius** (bán kính xoay, 1,4) và **Orbit Speed** (độ/giây, 120).
- `SoulBlade.prefab` chỉ có Sprite Renderer, **không có collider** (code tự kiểm tra va chạm theo Impact Radius).

### 4.5. Chông sắt → Bãi chông gai
| | Đường dẫn |
|---|---|
| Data | `WD/WeaponData_Caltrops.asset` → `WD/WeaponData_ThornField.asset` |
| Prefab vũ khí | `WD/CaltropsWeapon.prefab` (script `ZoneWeapon`, **Placement = At Owner**) |
| **Cần thay** | `P/CaltropsZone.prefab` ← `PH/Caltrops.png` |

- Vẽ **24 × 24 px**. Component **Damage Zone** có **Sprite Scale Per Radius = 2**: bán kính 0,6 thì hình hiện rộng 1,2 đơn vị.
- **Sorting Order = −1** (nằm dưới chân nhân vật). Khi vẽ map, đặt Tilemap thấp hơn −1.
- **Fade Duration** = thời gian mờ dần lúc sắp biến mất.

### 4.6. Tia sét → Bão sấm
| | Đường dẫn |
|---|---|
| Data | `WD/WeaponData_LightningStrike.asset` → `WD/WeaponData_Thunderstorm.asset` |
| Prefab vũ khí | `WD/LightningStrikeWeapon.prefab` (script `LightningStrike`, ô **Effect Scale Per Radius**) |
| **Cần thay** | `P/LightningEffect.prefab` ← `PH/LightningBolt.png` (gán ở ô **Effect Prefab** của `WeaponData`) |

- Sprite tia sét đặt **Pivot = Bottom**: chân tia chạm đúng vị trí quái, thân tia vươn lên trên.
- Muốn tia sét có animation: gán các khung vào ô **Frames** của component **Pooled Effect** và tăng **Lifetime** (hiện 0,15 giây).

### 4.7. Quả cầu phép → Tinh cầu vĩnh cửu
| | Đường dẫn |
|---|---|
| Data | `WD/WeaponData_OrbitingOrbs.asset` → `WD/WeaponData_EternalOrbs.asset` |
| Prefab vũ khí | `WD/OrbitingOrbsWeapon.prefab` (script `OrbitingOrbs`: ô **Orb Prefab**, **Hit Cooldown** = 0,5) |
| Hình ảnh | `P/Orb.prefab`: đang **mượn sprite cầu lửa của Wizard** (không phải hình tạm), có thể thay bằng quả cầu năng lượng khác |

`Orb.prefab` chỉ có Sprite Renderer, không có collider.

### 4.8. Bình độc → Đầm lầy chết chóc
| | Đường dẫn |
|---|---|
| Data | `WD/WeaponData_PoisonFlask.asset` → `WD/WeaponData_DeadlySwamp.asset` |
| Prefab vũ khí | `WD/PoisonFlaskWeapon.prefab` (`ZoneWeapon`, **Placement = Random Enemy**) |
| **Cần thay** | `P/PoisonPool.prefab` ← `PH/PoisonPool.png` |

Giống Chông sắt: vẽ 24 × 24 px, Sorting Order −1. Vũng độc nên **bán trong suốt** (alpha khoảng 50–70%) để vẫn thấy quái bên trong.

### 4.9. Băng trùy → Bão tuyết
| | Đường dẫn |
|---|---|
| Data | `WD/WeaponData_IceShard.asset` → `WD/WeaponData_Blizzard.asset` |
| Prefab vũ khí | `WD/IceShardWeapon.prefab` (`ProjectileWeapon`) |
| **Cần thay** | `P/IceShard.prefab` ← `PH/IceShard.png` |

Vẽ mũi băng hướng sang phải. Màu xanh nhạt khi quái bị chậm được chỉnh ở ô **Slow Tint Color** trên component `Enemy` của từng prefab quái.

---

## 5. Gán vũ khí cho lớp nhân vật

Mỗi lớp chỉ bốc được vũ khí và vật phẩm trong danh sách riêng:

1. Mở `Scripts/Data/chacracter/CharacterData_<Lớp>.asset` (Swordsman là `CharacterData.asset`).
2. **Weapon Pool:** các `WeaponData` của **vũ khí gốc** lớp này có thể nhặt. **Không** thêm vũ khí khởi đầu hoặc bản tiến hóa vào đây.
3. **Passive Pool:** vật phẩm bị động lớp này có thể nhặt. Nên có vật phẩm tiến hóa của mọi vũ khí trong Weapon Pool, nếu không vũ khí đó sẽ không bao giờ tiến hóa được.
4. **Starting Weapon:** vũ khí khởi đầu của lớp.

Bảng phân chia hiện tại xem `thiet-ke-vu-khi.md`, mục "Vũ khí và vật phẩm theo lớp".

---

## 6. Tiến hóa

### 6.1. Cách nối
Trong `WeaponData` của **vũ khí gốc**:
- **Evolution:** kéo `WeaponData` của bản tiến hóa vào.
- **Evolution Item:** kéo `PassiveItemData` cần có (trong `Scripts/Data/passive items/`).

Bản tiến hóa để trống hai ô này. Bản tiến hóa chỉ có **1 phần tử** trong Levels.

### 6.2. Bảng nối hiện tại

| Vũ khí gốc | Vật phẩm | Bản tiến hóa |
|---|---|---|
| Throwing Dagger | Marksman Glove | Thousand Blades |
| Boomerang | Feather | Lunar Wheel |
| Spirit Sword | Warrior Gauntlet | Soul Blades |
| Caltrops | Iron Armor | Thorn Field |
| Orbiting Orbs | Hourglass | Eternal Orbs |
| Lightning Strike | Amplify Crystal | Thunderstorm |
| Poison Flask | Magnet | Deadly Swamp |
| Ice Shard | Twin Charm | Blizzard |

### 6.3. Cho bản tiến hóa hình ảnh riêng
1. Chọn prefab hình ảnh của vũ khí gốc (ví dụ `P/Dagger.prefab`) → **Ctrl+D** để nhân bản → đổi tên (ví dụ `ThousandBladesDagger`).
2. Thay sprite trong bản nhân bản.
3. Mở `WeaponData` của **bản tiến hóa**, kéo prefab mới vào ô **Projectile Prefab** (đạn) hoặc **Effect Prefab** (tia sét). Với vũ khí vùng, nhân bản **prefab vũ khí** (`CaltropsWeapon`, `PoisonFlaskWeapon`) rồi gán **Zone Prefab** mới cho nó, sau đó gán prefab vũ khí mới vào ô **Weapon Prefab** của bản tiến hóa.

---

## 7. Tạo vũ khí mới (không cần viết code)

Chọn loại gần nhất rồi nhân bản:

| Muốn làm | Nhân bản từ | Script |
|---|---|---|
| Đạn bay thẳng / xòe quạt / bắn liên tiếp | Dao găm, Băng trùy, Cung gỗ | `ProjectileWeapon` + prefab đạn **Motion = Straight** |
| Đạn bay ra rồi quay về | Boomerang | `ProjectileWeapon` + **Motion = Boomerang** |
| Đạn tự đuổi quái | Kiếm bay | `ProjectileWeapon` + **Motion = Homing** |
| Đạn nổ vùng khi trúng | Tia phép | `ProjectileWeapon` + Impact Radius > 0 |
| Đánh quái ngẫu nhiên trên màn hình | Tia sét | `LightningStrike` |
| Vật thể xoay quanh người | Quả cầu phép | `OrbitingOrbs` |
| Vùng sát thương trên mặt đất | Chông sắt (dưới chân), Bình độc (chỗ quái) | `ZoneWeapon` |

Các bước:
1. Nhân bản (Ctrl+D) `WeaponData`, prefab vũ khí và prefab hình ảnh của vũ khí mẫu.
2. Trong `WeaponData` mới: đổi **Id** (không trùng), **Display Name**, **Description**, **Icon**; kéo prefab vũ khí mới vào **Weapon Prefab**, prefab hình mới vào **Projectile Prefab** / **Effect Prefab**; chỉnh **Aim Mode**, **Damage Type**, **Levels**; để **Is Starting Weapon** và **Play Owner Attack Anim** tắt.
3. Prefab đạn mới phải giữ: layer **PlayerProjectile**, **Rigidbody 2D** loại **Kinematic** có bật **Use Full Kinematic Contacts**, collider có tích **Is Trigger**.
4. Thêm `WeaponData` mới vào **Weapon Pool** của các lớp phù hợp (mục 5).
5. Nếu có bản tiến hóa: làm theo mục 6.

**Aim Mode:** `Cursor` (theo chuột), `Nearest Enemy` (quái gần nhất trong Range), `Random Enemy` (có quái là đánh; tia sét và bình độc tự chọn mục tiêu), `Orbit` / `Trail` (không cần hướng), `Omni` (theo chuột, dùng với góc xòe để tỏa tròn).

**Damage Type:** `Physical` (bị giáp quái giảm, được Swordsman chí mạng) hoặc `Magic` (bị kháng phép giảm, được Mage cộng hưởng).

---

## 8. Kiểm tra sau khi chỉnh

1. Vào Play, dùng DebugTools:
   - **+XP** để lên cấp và nhặt vũ khí.
   - **Max weapons** để đưa mọi vũ khí lên cấp tối đa.
   - **Give evolution items** để nhận vật phẩm tiến hóa, rồi **+XP** để thấy thẻ **EVOLVE**.
2. Kiểm tra:
   - Đạn bay **đúng hướng** (không bay ngang, không bay ngược).
   - Hình **không quá to hoặc quá nhỏ** so với nhân vật.
   - Đạn **trúng thân quái** (hurtbox), không chỉ trúng chân.
   - Vùng mặt đất **không bị nền map che**.
   - Console không có lỗi `MissingReferenceException` hay `NullReferenceException`.

### Lỗi thường gặp

| Hiện tượng | Nguyên nhân | Cách sửa |
|---|---|---|
| Đạn bay ngang hoặc ngược hướng | Sprite không vẽ hướng sang phải | Xoay ảnh gốc, hoặc chỉnh rotation của Sprite Renderer |
| Hình quá to (gấp ~4 lần) | Pixels Per Unit vẫn là 100 | Đặt **24** rồi Apply |
| Hình bị nhòe | Filter Mode là Bilinear | Đặt **Point (no filter)** |
| Đạn bay xuyên quái không gây sát thương | Prefab đạn sai layer, thiếu Rigidbody Kinematic, hoặc collider không tích Is Trigger | Xem mục 7 bước 3 |
| Thẻ lên cấp không có vũ khí mới | Vũ khí chưa có trong **Weapon Pool** của lớp, hoặc đã đủ 4 ô vũ khí | Xem mục 5 |
| Không bao giờ thấy thẻ tiến hóa | Thiếu **Evolution** / **Evolution Item**, vũ khí chưa cấp 5, hoặc vật phẩm không có trong **Passive Pool** của lớp | Xem mục 5 và 6 |
| `MissingReferenceException` sau khi đổi prefab | Reload Domain đang tắt, dữ liệu cũ còn trong bộ nhớ | Chuột phải `WeaponData` → **Reimport**, hoặc khởi động lại Unity |
