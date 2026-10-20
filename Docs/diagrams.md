# Lược đồ cho báo cáo

Danh sách chức năng: xem `phan-tich-chuc-nang.md`. Ghi chú trong lược đồ viết bằng tiếng Anh.
Xem/xuất ảnh: dán từng khối mã vào https://mermaid.live (Actions → PNG/SVG).

- Chức năng 1–5: vẽ theo **code hiện tại**.
- Chức năng 6 (Shop): vẽ theo `thiet-ke-shop.md`, **chưa có code**. Tên class đánh dấu *(planned)*, cần đối chiếu lại khi cài đặt.

---

## 1. Điều khiển nhân vật và sinh tồn

### 1.1. Sơ đồ trạng thái: Player

```mermaid
stateDiagram-v2
    state HealthCheck <<choice>>
    state ReviveCheck <<choice>>

    [*] --> Alive : Awake / HP = MaxHP, equip starting weapon

    state Alive {
        [*] --> Idle
        Idle --> Move : move input != 0 / IsMoving = true
        Move --> Idle : move input == 0 / IsMoving = false
    }

    Alive --> HealthCheck : TakeDamage [not invincible] / HP -= max(1, damage - Armor), hurt flash
    HealthCheck --> Alive : [HP > 0]
    HealthCheck --> ReviveCheck : [HP <= 0]
    ReviveCheck --> Alive : [Revival > 0] / Revival - 1, HP = MaxHP * 0.5
    ReviveCheck --> Dead : [Revival == 0]

    Dead : entry / velocity = 0, play Die (unscaled time), raise OnDeath
    Dead --> [*]

    note right of Alive
        Every frame while GameState == Playing:
        read input, aim at cursor, flip sprite,
        set velocity, regenerate HP (Recovery),
        tick all weapons
    end note
```

### 1.2. Sơ đồ tuần tự: Player nhận sát thương từ quái

```mermaid
sequenceDiagram
    participant EM as EnemyManager
    participant E as Enemy
    participant P as Player
    participant HUD as HudView
    participant GM as GameManager

    EM->>E: Tick(playerPosition, deltaTime)
    EM->>E: TryContact(playerPosition)
    E->>E: check state == Chase, contactTimer <= 0, distance² <= contactRadius²
    E-->>EM: true (contactTimer = contactDamageInterval)
    EM->>E: CreateContactDamage()
    E-->>EM: DamageInfo(contactDamage, Physical)
    EM->>P: TakeDamage(info)

    alt state == Dead or IsInvincible
        P-->>EM: ignore
    else alive
        P->>P: SetHealth(HP - max(1, amount - Armor))
        P->>HUD: OnHealthChanged(current, max)
        P->>P: StartHurtFlash()
        opt HP <= 0
            alt Revival > 0
                P->>P: Revival - 1, SetHealth(MaxHP * RevivalHealthRatio)
                P->>HUD: OnHealthChanged(current, max)
            else Revival == 0
                P->>P: ChangeState(Dead)
                P->>GM: OnDeath
                GM->>GM: SetState(GameOver), timeScale = 0
            end
        end
    end
```

---

## 2. Tấn công tự động

### 2.1. Sơ đồ trạng thái: Weapon

```mermaid
stateDiagram-v2
    [*] --> Ready : Equip(owner, data) / Level = 1, cooldownTimer = 0

    Ready --> CoolingDown : Tick / Attack(AimDirection), cooldownTimer = AttackInterval / AttackSpeed, [PlayOwnerAttackAnim] PlayAttackAnimation(interval)
    CoolingDown --> CoolingDown : Tick / cooldownTimer -= deltaTime
    CoolingDown --> Ready : [cooldownTimer <= 0]

    Ready --> Ready : LevelUp() [Level < MaxLevel] / Level + 1
    CoolingDown --> CoolingDown : LevelUp() [Level < MaxLevel] / Level + 1

    note right of CoolingDown
        Tick is called by Player only while
        GameState == Playing, so the cooldown
        freezes during LevelUp and GameOver
    end note
```

### 2.2. Sơ đồ tuần tự: một lần chém (SwordSlash)

```mermaid
sequenceDiagram
    participant P as Player
    participant INV as PlayerInventory
    participant W as SwordSlash
    participant EM as EnemyManager
    participant E as Enemy
    participant PM as PoolManager

    P->>INV: TickWeapons(deltaTime)
    INV->>W: Tick(deltaTime)
    W->>W: cooldownTimer -= deltaTime
    opt cooldownTimer <= 0
        W->>W: cooldownTimer = AttackInterval / AttackSpeed
        W->>W: Attack(AimDirection)
        W->>W: range = Size * Area, minDot = cos(ArcAngle / 2)
        W->>EM: Active
        EM-->>W: list of enemies
        loop each enemy
            opt CanBeHit and distance <= range + contactRadius and dot(toEnemy, aim) >= minDot
                W->>P: ModifyOutgoingDamage(Damage * Might)
                P-->>W: amount (Swordsman: critical hit chance)
                W->>E: TakeDamage(DamageInfo(amount, DamageType, knockback))
                E->>E: HP -= amount * (1 - resist), push back * (1 - knockbackResist), hit flash
                opt HP <= 0
                    E->>E: Die() / state = Dead, play Die
                    E->>EM: OnDeath(enemy)
                end
            end
        end
        opt EffectPrefab assigned
            W->>PM: Get(EffectPrefab, position, rotation)
            PM-->>W: slash effect (scale = range)
        end
        opt PlayOwnerAttackAnim
            W->>P: PlayAttackAnimation(attackInterval)
            P->>P: AttackSpeed param = max(1, clipLength / interval), trigger Attack
        end
    end
```

---

## 3. Quái xuất hiện, truy đuổi và tấn công

### 3.1. Sơ đồ trạng thái: Enemy (gồm vòng đời trong pool)

```mermaid
stateDiagram-v2
    [*] --> InPool : PoolManager creates instance / SetActive(false)

    InPool --> Spawning : EnemyManager.Spawn(data, position) / Get from pool, Init(data), HP = MaxHP, Register
    Spawning --> Spawning : Tick / alpha = elapsed / spawnFadeDuration
    Spawning --> Chase : [elapsed >= spawnFadeDuration]

    Chase --> Chase : Tick / move toward player, flip sprite, contactTimer -= deltaTime
    Chase --> Chase : TakeDamage [HP > 0] / HP -= damage * (1 - resist), knockback, hit flash
    Chase --> Chase : [too far from player] / Teleport ahead of player
    Chase --> Dead : TakeDamage [HP <= 0] / play Die, raise OnDeath

    Dead --> Dead : Tick / stateTimer -= deltaTime
    Dead --> InPool : [stateTimer <= 0] / Unregister (swap with last), Release to pool

    note right of Spawning
        Cannot deal or take damage
    end note
    note right of Dead
        stateTimer = length of the Death clip
    end note
```

### 3.2. Sơ đồ tuần tự: spawn, cập nhật mỗi frame và thu hồi quái

```mermaid
sequenceDiagram
    participant SRC as DebugTools / SpawnManager
    participant EM as EnemyManager
    participant PM as PoolManager
    participant E as Enemy
    participant P as Player

    SRC->>EM: Spawn(data, offscreenPosition)
    EM->>PM: Get(data.Prefab, position)
    alt pool has inactive instance
        PM->>PM: take instance
    else pool empty
        PM->>PM: Instantiate(prefab)
    end
    PM->>E: SetActive(true)
    PM-->>EM: enemy
    EM->>E: Init(data)
    EM->>E: subscribe OnDeath
    EM->>EM: Register(enemy) / ListIndex = Count, add to Active

    loop every frame while GameState == Playing (from last to first)
        EM->>E: Tick(playerPosition, deltaTime)
        alt IsReadyToDespawn
            EM->>EM: Unregister(enemy) / move last into ListIndex, remove last (O(1))
            EM->>PM: Release(enemy)
            alt pool below max size
                PM->>E: SetActive(false)
            else pool full
                PM->>E: Destroy
            end
        else still active
            EM->>E: TryContact(playerPosition)
            opt contact
                EM->>P: TakeDamage(CreateContactDamage())
            end
            opt distance² > relocateDistance²
                EM->>E: Teleport(ahead of player MoveDirection ± spread)
            end
        end
    end
```

---

## 4. Thu thập kinh nghiệm và vàng

### 4.1. Sơ đồ trạng thái: Pickup (gem / coin)

```mermaid
stateDiagram-v2
    [*] --> InPool

    InPool --> Idle : spawned at dead enemy position / Init(value), set gem tier color and scale
    Idle --> Idle : another gem merges in [gems on map >= MaxGemsOnMap] / AddValue, update tier
    Idle --> Attracted : [distance <= Magnet]

    Attracted --> Attracted : Tick / move toward player at AttractSpeed
    Attracted --> Collected : [distance <= CollectRadius]

    Collected : entry / Gem - AddXP(value * Growth), Coin - AddGold(value * Greed)
    Collected --> InPool : Release to pool

    note right of Attracted
        Stays attracted even if the player
        moves out of the Magnet radius.
        AttractSpeed must exceed the player's
        maximum move speed
    end note
```

### 4.2. Sơ đồ tuần tự: quái chết rơi đồ và người chơi nhặt

```mermaid
sequenceDiagram
    participant E as Enemy
    participant EM as EnemyManager
    participant ST as RunScoreTracker
    participant PU as PickupManager
    participant PM as PoolManager
    participant PP as PlayerProgression
    participant HUD as HudView

    E->>EM: OnDeath(enemy)
    EM->>ST: OnEnemyDied(enemy)
    ST->>ST: Kills + 1, KilledXP += XpDrop
    EM->>PU: OnEnemyDied(enemy)

    opt XpDrop > 0
        alt gemCount >= MaxGemsOnMap and a gem exists
            PU->>PU: find nearest gem, AddValue(XpDrop), update tier
        else
            PU->>PM: Get(gemPrefab, position)
            PM-->>PU: gem
            PU->>PU: Init(XpDrop), set tier, gemCount + 1
        end
    end
    opt random < CoinDropChance
        PU->>PM: Get(coinPrefab, position)
        PM-->>PU: coin
    end

    loop every frame while GameState == Playing
        PU->>PU: TickPickup / attract inside Magnet, move toward player
        opt distance <= CollectRadius
            alt gem
                PU->>PP: AddXP(value * Growth)
                PP->>HUD: OnXPChanged(current, required)
                opt CurrentXP >= RequiredXP
                    PP->>PP: Level + 1, PendingLevelUps + 1, raise OnLevelUpReady (see 5.2)
                end
            else coin
                PU->>PP: AddGold(round(value * Greed))
                PP->>HUD: OnGoldChanged(gold)
            end
            PU->>PM: Release(pickup)
        end
    end
```

---

## 5. Lên cấp và chọn nâng cấp

### 5.1. Sơ đồ trạng thái: GameManager

```mermaid
stateDiagram-v2
    [*] --> Playing : Awake / timeScale = 1

    Playing --> LevelUp : OnLevelUpReady / timeScale = 0, RollOptions, show panel
    LevelUp --> LevelUp : ChooseUpgrade(i) [PendingLevelUps > 0] / Apply, RollOptions
    LevelUp --> Playing : ChooseUpgrade(i) [PendingLevelUps == 0] / Apply, hide panel, timeScale = 1

    Playing --> GameOver : Player.OnDeath / timeScale = 0, show GameOver, compute score
    GameOver --> [*] : Retry (reload scene) or Exit (Main Menu) / timeScale = 1

    note right of Playing
        RunTime increases only in Playing.
        Player, weapons, enemies and pickups
        update only in Playing
    end note
```

### 5.2. Sơ đồ tuần tự: lên cấp và chọn thẻ

```mermaid
sequenceDiagram
    actor U as Player (user)
    participant PP as PlayerProgression
    participant LM as LevelUpManager
    participant GM as GameManager
    participant INV as PlayerInventory
    participant UI as LevelUpPanel
    participant P as Player

    PP->>PP: while CurrentXP >= RequiredXP / Level + 1, RequiredXP = 5 + Level * 10
    PP->>LM: OnLevelUpReady
    alt already choosing
        LM-->>PP: ignore (pending count is kept)
    else
        LM->>GM: EnterLevelUp()
        GM->>GM: timeScale = 0
        loop while PendingLevelUps > 0
            LM->>INV: read weapons and passives
            LM->>LM: BuildCandidates (weapon levels, new weapons, new passives, passive levels, stat cards)
            LM->>LM: Shuffle (Fisher-Yates)
            LM->>LM: take up to 3, max 1 stat card, fill with fallback (heal / gold)
            LM->>UI: OnOptionsReady(options)
            UI->>UI: show panel, fill title and description
            U->>UI: click card i
            UI->>LM: ChooseUpgrade(i)
            alt NewWeapon / WeaponLevel
                LM->>INV: AddWeapon / weapon.LevelUp()
            else NewPassive / PassiveLevel
                LM->>INV: AddPassive / LevelUpPassive
                INV->>P: ApplyStatModifier(stat, value, kind)
            else StatCard
                LM->>P: ApplyStatModifier, Heal(healAmount)
            else Fallback heal
                LM->>P: Heal(30)
            else Fallback gold
                LM->>PP: AddGold(20)
            end
            LM->>PP: ConsumePendingLevelUp()
        end
        LM->>UI: OnClosed
        UI->>UI: hide panel
        LM->>GM: ResumePlaying()
        GM->>GM: timeScale = 1
    end
```

---

## 6. Shop trong ván *(planned)*

### 6.1. Sơ đồ trạng thái: Shop

```mermaid
stateDiagram-v2
    [*] --> Closed

    Closed --> Open : RunTime reaches shop time (minute 10 or 20) / timeScale = 0, rerollCost = base, GenerateStock

    Open --> Open : Buy(slot) [gold >= price] / spend gold, apply item, slot = Sold
    Open --> Open : Reroll [gold >= rerollCost] / spend gold, rerollCost += step, GenerateStock
    Open --> ConfirmDiscard : select owned item [not starting weapon]

    ConfirmDiscard --> Open : Confirm / remove item, refund 30% of invested value
    ConfirmDiscard --> Open : Cancel

    Open --> Countdown : Continue
    Countdown --> Countdown : 3 - 2 - 1 (unscaled time)
    Countdown --> Closed : countdown finished / timeScale = 1, GameState = Playing

    Open --> [*] : Save and Exit (out of scope)

    note right of Open
        The run stays frozen behind the shop.
        Enemies, pickups and the clock are kept
    end note
```

### 6.2. Sơ đồ tuần tự: mở Shop, bày hàng và mua một món

```mermaid
sequenceDiagram
    actor U as Player (user)
    participant GM as GameManager
    participant SM as ShopManager (planned)
    participant INV as PlayerInventory
    participant PP as PlayerProgression
    participant UI as ShopView (planned)

    GM->>GM: RunTime >= next shop time
    GM->>GM: SetState(Shop), timeScale = 0
    GM->>SM: OpenShop()
    SM->>SM: rerollCost = rerollBaseCost
    SM->>INV: read free slots and owned items
    SM->>SM: filter items (slots, unlocked weapons, maxPerRun)
    SM->>SM: slot 1 = random potion (guaranteed)
    SM->>SM: slot 2 = level-up of an owned item (guaranteed)
    SM->>SM: slots 3-5 = weighted random from remaining items
    SM->>UI: OnStockChanged(slots)

    U->>UI: click Buy on slot
    UI->>SM: Buy(slot)
    SM->>PP: TrySpendGold(price)
    alt enough gold
        PP-->>SM: true
        SM->>INV: apply item (shared with level-up apply code)
        SM->>SM: slot = Sold
        SM->>UI: OnStockChanged(slots)
    else not enough gold
        PP-->>SM: false
        SM->>UI: show not enough gold
    end

    U->>UI: click Continue
    UI->>SM: Close()
    SM->>UI: show countdown 3 - 2 - 1 (unscaled time)
    SM->>GM: ResumePlaying()
    GM->>GM: timeScale = 1
```
