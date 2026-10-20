# CLAUDE.md — Survival Action (Unity)

Game hành động sinh tồn kiểu Vampire Survivors, góc nhìn 2,5D (map 2D, di chuyển tự do 8 hướng), làm bằng Unity cho đồ án nhóm 3 người. File này là **quy ước bắt buộc** khi viết code trong project.

---

## 1. Tài liệu thiết kế

Toàn bộ thiết kế nằm trong `Docs/`. **Đọc tài liệu liên quan trước khi code một tính năng.**

| File | Nội dung |
|---|---|
| `Docs/kich-ban-man-choi.md` | Cấu trúc ván 30 phút, kịch bản spawn theo thời gian, độ khó, lưu ván (đang hoãn) |
| `Docs/thiet-ke-nhan-vat.md` | 4 lớp nhân vật, 14 chỉ số chung, chỉ số đặc trưng, FSM, điều khiển, loại sát thương (kỹ năng đã bỏ) |
| `Docs/thiet-ke-quai.md` | 14 loại quái, hành vi, boss, kháng, rơi đồ |
| `Docs/thiet-ke-vu-khi.md` | Vũ khí khởi đầu, 8 vũ khí, tiến hóa, vật phẩm bị động, thẻ chỉ số, thẻ lên cấp |
| `Docs/thiet-ke-shop.md` | Shop vĩnh viễn (không có Shop trong ván), vàng tài khoản = coin + điểm |
| `Docs/thiet-ke-am-thanh.md` | Nhạc nền, danh sách tiếng hiệu ứng và lúc phát, giới hạn tiếng, âm lượng, cấu trúc code âm thanh |
| `Docs/huong-dan-vu-khi.md` | Hướng dẫn thay hình, gán icon, chỉnh thông số, nối tiến hóa và tạo vũ khí mới trong Editor |
| `Docs/huong-dan-dang-nhap.md` | Cấu hình Firebase Console, dựng UI màn đăng nhập và khung tài khoản ở Main Menu |
| `Docs/huong-dan-bang-xep-hang.md` | Dựng UI bảng xếp hạng ở Main Menu, ô hạng ở màn GameOver, kiểm tra trên Firebase |
| `Docs/huong-dan-shop.md` | Dựng scene Shop vĩnh viễn, prefab ô hàng, gán Inspector, kiểm tra vàng tài khoản và lượt mua trên Firebase |
| `Docs/huong-dan-am-thanh.md` | Tạo `SoundData`, prefab `AudioManager`, gán âm thanh trong Inspector, thanh âm lượng |
| `Docs/huong-dan-ban-do.md` | Dựng map 150 × 150 bằng Tiny Swords: layer `Obstacle`, Grid/Tilemap, `MapBounds`, giới hạn camera |
| `Docs/viec-can-lam-sau.md` | Việc đã phát hiện nhưng tạm hoãn (lỗi nhỏ, cân bằng, dọn dẹp). Xem trước khi bắt đầu một mốc mới |

**Thứ tự ưu tiên khi có mâu thuẫn:**
- **Luật chơi và con số:** tài liệu thiết kế là chuẩn.
- **Cách viết code và hiệu năng:** file `CLAUDE.md` này là chuẩn. Các đoạn code mẫu trong tài liệu thiết kế chỉ để minh họa ý tưởng (ví dụ `OnCollisionStay2D`, FSM tách class); khi cài đặt thật, theo quy tắc ở mục 3 và 4.
- Nếu phát hiện tài liệu thiết kế tự mâu thuẫn hoặc thiếu thông tin, **dừng lại và hỏi**, không tự đoán.

---

## 2. Môi trường

- Unity: **6000.6.3f1**, 2D, URP.
- Input: kiểm tra project đang dùng **Input System mới hay Input Manager cũ** trước khi viết code input, và dùng đúng cái đó. Hiện tại: **Input System mới** (file `Settings/InputSystem_Actions`).
- **Tỉ lệ:** mọi sprite nhân vật và quái dùng **Pixels Per Unit = 24** (nhân vật cao ~1 đơn vị, khớp các con số tầm đánh, tốc độ, bán kính trong tài liệu). Pixel Perfect Camera dùng **Assets PPU = 24**. Tileset (kể cả Tiny Swords, ô 64 px) cũng PPU 24, nên `Grid` có Cell Size 2,6667 (đã chốt 2026-10-19). Không đổi PPU riêng lẻ cho từng sprite.
- Ngôn ngữ: C#. **Mọi tên class, hàm, biến, file viết bằng tiếng Anh.** Tài liệu trong `Docs/` viết tiếng Việt.
- Asset nhân vật và quái: Tiny RPG Character Asset Pack (22 nhân vật). Hiệu ứng: các gói BDragon1727. Xem mục Asset trong từng tài liệu thiết kế.

---

## 3. Quy ước code: ngắn gọn, dễ đọc

### Nguyên tắc chung
- **Mỗi class một trách nhiệm**, nhưng **không trừu tượng hóa quá mức**. Chỉ tạo interface hay lớp cơ sở khi đã có ít nhất 2 nơi dùng.
- **Không số cứng trong code.** Mọi con số cân bằng (máu, sát thương, nhịp, giá, …) nằm trong ScriptableObject.
- Dùng `[SerializeField] private`, **không dùng field `public`** để hiện trong Inspector. Cần đọc từ ngoài thì dùng property `{ get; private set; }`.
- **Không viết comment trong code**, kể cả `/// <summary>`, comment cuối dòng, comment `TODO` hay code bị comment lại. Code phải tự giải thích được qua tên class, hàm, biến rõ nghĩa và hàm ngắn. Nếu cần giải thích một quyết định, ghi vào câu trả lời hoặc vào `Docs/`, không ghi vào code.
- Hàm dài quá ~40 dòng thì tách.

### Đặt tên
| Loại | Quy ước | Ví dụ |
|---|---|---|
| Class, method, property, enum | PascalCase | `SpawnManager`, `TakeDamage()`, `AimDirection` |
| Field private, biến cục bộ, tham số | camelCase | `currentHP`, `spawnTimer` |
| Hằng số | PascalCase | `MaxEnemies` |
| ScriptableObject | hậu tố `Data` hoặc `Config` | `EnemyData`, `ShopConfig` |
| Event | tiền tố `On` | `OnDeath`, `OnLevelUp` |

### FSM
- Dùng **`enum` + `switch`** ngay trong class (Player, Enemy, GameManager). **Không** tách mỗi trạng thái thành một class.
- Mỗi trạng thái có hàm `Enter...()` nếu cần làm việc gì khi vào trạng thái.

### Giao tiếp giữa các hệ thống
- Dùng **C# event** (`event Action<T>`), không dùng `UnityEvent` trong code runtime.
- **Cấm** `FindObjectOfType`, `GameObject.Find`, `SendMessage`.
- Singleton **chỉ cho**: `GameManager`, `EnemyManager`, `PoolManager`, `ProjectileManager`, `AudioManager` (sống qua các scene, gọi qua hàm static `AudioManager.Play`). Các hệ thống khác nhận tham chiếu qua Inspector hoặc qua các manager này.
- **Player không đặt sẵn trong scene.** `GameManager` (chạy trước mọi script, `DefaultExecutionOrder(-100)`) sinh Player từ `CharacterData.playerPrefab` của lớp đã chọn trong `Awake`. Hệ thống khác lấy Player qua `GameManager.Instance.Player` trong `Awake` của mình, không giữ ô `[SerializeField] Player`.
- Nhớ **hủy đăng ký event** trong `OnDisable` / `OnDestroy`.

### Namespace và thư mục
```
Assets/
  Scripts/
    Core/          Game.Core        Entity, DamageInfo, GameEvents, PoolManager
    Player/        Game.Player      Player, PlayerStats, PlayerInventory, 4 lớp con
    Weapons/       Game.Weapons     Weapon, Projectile, MeleeHitbox, DamageZone, các vũ khí
    Enemies/       Game.Enemies     Enemy, EnemyManager, các hành vi, boss
    Spawning/      Game.Spawning    SpawnManager, SpawnPatterns
    Progression/   Game.Progression XP, level up, pickup, upgrade
    Shop/          Game.Shop
    Save/          Game.Save
    Audio/         Game.Audio       AudioManager, SceneMusic, GameAudio, ButtonSounds
    Online/        Game.Online      AuthService, UserProfileService, RunResultService, LeaderboardService (lớp static, Firebase), LoginView, AccountPanelView, RunResultUploader
    UI/            Game.UI
    Data/          Game.Data        Mọi class ScriptableObject
    Debug/         Game.DebugTools  Chỉ chạy trong Editor / development build
  Data/            File .asset (ScriptableObject)
  Prefabs/
  Art/             Sprite, animation, VFX
  Scenes/
```

---

## 4. Quy tắc hiệu năng (mục tiêu: 60 FPS với 390 quái)

Mục tiêu đo trên **bản build**, không đo trong Editor.

| # | Quy tắc |
|---|---|
| 1 | **Object Pool** cho quái, đạn, gem, coin, VFX. Dùng `UnityEngine.Pool.ObjectPool<T>`, không tự viết pool. **Cấm** `Instantiate` / `Destroy` trong gameplay (trừ khi khởi tạo pool). |
| 2 | **Có va chạm vật lý quái–quái** để quái không chồng lên nhau (xem quy tắc 6). Collider chỉ ôm phần chân. Phải đo lại FPS mỗi khi đổi cấu hình vật lý. |
| 3 | **Quái không có `Update()` riêng.** `EnemyManager` gọi `enemy.Tick(playerPos, dt)` cho mọi quái trong một vòng lặp. |
| 4 | **Quái đánh theo tầm**: trong `Tick`, quái dừng lại khi khoảng cách từ mép collider của nó tới mép collider Player (`Collider2D.Distance`) ≤ `attackRange`, phát đòn đánh, đến `attackHitTime` thì kiểm tra lại tầm rồi mới gây sát thương. Không dùng `OnCollisionStay2D` / `OnTriggerStay2D`. |
| 5 | **Tìm mục tiêu** (quái gần nhất, quái ngẫu nhiên) từ danh sách `EnemyManager.Active`, không quét scene. |
| 6 | **Truy vấn vùng** (vụ nổ, hào quang) dùng `Physics2D.OverlapCircle` với **buffer dựng sẵn** (`ContactFilter2D` + mảng/List tái sử dụng), không dùng bản trả về mảng mới. **Va chạm vật lý:** quái có `Rigidbody2D` Dynamic (gravity 0, khóa xoay) và collider ôm **phần chân** (mặc định `CapsuleCollider2D` nằm ngang, layer `Enemy`) để không chồng lên nhau. Player là `Rigidbody2D` **Kinematic**, di chuyển bằng `Rigidbody2D.Slide` nên bị quái chặn lại (bị vây là kẹt) và không bị quái đẩy đi. **Hurtbox:** mỗi quái và Player có object con `Hurtbox` (tự sinh trong `Awake` theo kích thước sprite) ôm cả thân, layer `EnemyHurtbox` / `PlayerHurtbox`. Ma trận va chạm chỉ cho hurtbox chạm đạn của phe kia (`PlayerProjectile` ↔ `EnemyHurtbox`, `EnemyProjectile` ↔ `PlayerHurtbox`), nên không ảnh hưởng việc quái đứng chen nhau. Đòn cận chiến duyệt `EnemyManager.Active`: lọc thô bằng `sqrMagnitude` theo tâm, rồi đo chính xác bằng `Collider2D.ClosestPoint` trên hurtbox. **Đạn** là `Rigidbody2D` Kinematic (`useFullKinematicContacts`) + collider trigger, trúng qua `OnTriggerEnter2D` vào `Hurtbox`; `ProjectileManager` bắn, đếm tầm bay và thu hồi về pool (đạn không có `Update` riêng). Nếu Profiler cho thấy kiểm tra va chạm đạn tốn kém thì bổ sung **spatial grid** trong `EnemyManager`. |
| 7 | **Không tạo rác trong vòng lặp nóng** (`Update`, `Tick`, hàm gọi mỗi frame): không LINQ, không `new List` / `new` mảng, không nối chuỗi, không lambda có capture, không `foreach` trên kiểu tạo enumerator rác. |
| 8 | Cache mọi `GetComponent` trong `Awake`. Cache `Camera.main`. |
| 9 | Dùng `sqrMagnitude` thay cho `Vector2.Distance` khi chỉ cần so sánh. |
| 10 | **HUD chỉ cập nhật text khi giá trị thay đổi**, không cập nhật mỗi frame. |
| 11 | **Sprite Atlas** cho sprite quái, vũ khí, pickup. |
| 12 | Animator của quái đặt **Culling Mode = Cull Completely**. |
| 13 | Quái thường spawn trong một **vành đai** ngoài mép camera, sâu ngẫu nhiên từ 0 đến `spawnDepth`. Quái ở **quá xa người chơi** (xa hơn vành đai + `relocateBuffer`) được dịch chuyển vào vành đai phía trước hướng di chuyển của người chơi, không despawn. Quái bay ngang của sự kiện Line/Swarm thì despawn. Map có giới hạn (`MapBounds`): điểm spawn / dịch chuyển phải nằm trong map và không trúng layer `Obstacle`, thử lại tối đa `spawnAttempts` góc; quái bay ngang chạm viền map thì despawn. |
| 14 | Gem trên bản đồ vượt 300 thì gộp gem mới vào gem gần nhất. |
| 15 | Không để `Debug.Log` trong vòng lặp nóng. Log dùng cho debug phải bọc trong `#if UNITY_EDITOR` hoặc tắt được. |

`Unregister` hoán đổi phần tử cần xóa với phần tử cuối rồi xóa cuối, để thao tác là O(1) và không phải dồn mảng. Mỗi `Enemy` giữ `ListIndex` của mình để làm việc này.

### Khung `EnemyManager`

```csharp
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [SerializeField] private Transform player;
    private readonly List<Enemy> active = new(300);
    public IReadOnlyList<Enemy> Active => active;

    void Awake() => Instance = this;

    public void Register(Enemy e)   { e.ListIndex = active.Count; active.Add(e); }

    public void Unregister(Enemy e)
    {
        Enemy last = active[^1];
        active[e.ListIndex] = last;
        last.ListIndex = e.ListIndex;
        active.RemoveAt(active.Count - 1);
    }

    void Update()
    {
        Vector2 playerPos = player.position;
        float dt = Time.deltaTime;
        for (int i = active.Count - 1; i >= 0; i--)
            active[i].Tick(playerPos, dt);
    }

    public Enemy FindNearest(Vector2 from, float maxRange)
    {
        Enemy best = null;
        float bestSqr = maxRange * maxRange;
        for (int i = 0; i < active.Count; i++)
        {
            float d = ((Vector2)active[i].transform.position - from).sqrMagnitude;
            if (d < bestSqr) { bestSqr = d; best = active[i]; }
        }
        return best;
    }
}
```

---

## 5. Lộ trình: làm từng lát cắt chơi được

Mỗi mốc phải **chạy được và test được** trước khi sang mốc sau.

| Mốc | Nội dung | Tiêu chí hoàn thành |
|---|---|---|
| **1** | Player di chuyển + ngắm chuột, 1 vũ khí, 1 loại quái đuổi theo, `PoolManager`, `EnemyManager`, `DebugTools` | **Stress test 300 quái** trên bản build, ghi lại FPS |
| **2** | Sát thương, quái chết, rơi gem/coin, nhặt, lên cấp với 3 thẻ (vài nâng cấp đơn giản) | Chơi được vòng cơ bản 5 phút |
| **3** | `SpawnManager` đọc `RunScript`, sự kiện Ring/Line/Swarm/Pause, 3 độ khó | Tua đồng hồ kiểm tra từng mốc thời gian |
| **4** | 4 lớp nhân vật (không có kỹ năng) | Mỗi lớp chơi thử 1 ván ngắn |
| **5** | 8 vũ khí, vật phẩm bị động, thẻ chỉ số, tiến hóa | Dùng DebugTools cấp đồ trực tiếp để test tiến hóa |
| **6** | Hành vi quái đặc biệt, Werebear, Necromancer | Tua đến phút 15 và 30 |
| **7** | Shop, lưu ván, vàng tài khoản, mở khóa | Lưu ở Shop → thoát → nạp lại đúng trạng thái |
| **8** | Hoàn thiện UI, Firebase, cân bằng | Chơi trọn 30 phút ở cả 3 độ khó, đo lại FPS |

### `DebugTools` (làm ở mốc 1, chỉ có trong Editor / development build)
- Tua đồng hồ ván (+1 phút, nhảy đến phút bất kỳ)
- Bất tử, hồi đầy máu
- Spawn N quái loại bất kỳ
- Cộng XP / vàng, cấp vũ khí / vật phẩm trực tiếp
- Hiện FPS và số quái đang sống

---

## 6. Cách làm việc với Claude Code

- **Trước khi code:** đọc tài liệu thiết kế liên quan và code hiện có trong thư mục liên quan. Nêu ngắn gọn kế hoạch (file nào tạo / sửa) và **chờ xác nhận** với thay đổi lớn (từ 3 file trở lên hoặc đổi kiến trúc).
- **Thay đổi nhỏ và tập trung:** mỗi lần chỉ làm một tính năng / một mốc con. Không sửa file ngoài phạm vi yêu cầu.
- **Không sửa phần của thành viên khác** (nếu được đánh dấu) khi chưa được yêu cầu; nếu cần thay đổi ở đó, nêu rõ và hỏi.
- **Không tự bịa con số cân bằng.** Lấy từ tài liệu thiết kế; nếu tài liệu chưa có, hỏi hoặc đặt giá trị tạm trong ScriptableObject và **liệt kê các giá trị tạm đó trong câu trả lời** (không ghi chú trong code).
- **Sau mỗi thay đổi, luôn kèm:**
  1. Danh sách file đã tạo / sửa.
  2. Việc cần làm trong Unity Editor (tạo asset, gán tham chiếu, cài layer, …), vì Claude Code không thao tác được Editor.
  3. Các bước test cụ thể để kiểm tra tính năng vừa làm.
  4. Các giá trị tạm hoặc việc còn dang dở (thay cho comment `TODO` trong code).
- **Không di chuyển / đổi tên asset bằng lệnh file hệ thống** (sẽ làm hỏng file `.meta`); hướng dẫn làm trong Unity Editor.
- Khi thay đổi làm lệch tài liệu thiết kế (ví dụ đổi cách tính), **cập nhật file trong `Docs/`** tương ứng và ghi vào phần nhật ký thay đổi.
