using System;
using System.Collections.Generic;
using Game.Core;
using Game.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class Player : Entity
    {
        private enum PlayerState { Idle, Move, Dead }

        private const float MinimumDamageTaken = 1f;
        private const int SlideIterations = 3;
        private const float MaxSlideAngle = 90f;

        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int AttackSpeedHash = Animator.StringToHash("AttackSpeed");
        private static readonly int DieHash = Animator.StringToHash("Die");

        [SerializeField] private CharacterData data;
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private Collider2D bodyCollider;
        [SerializeField] private AnimationClip attackClip;
        [SerializeField] private Vector2 hurtboxScale = new(0.6f, 0.9f);
        [SerializeField] private Color hurtFlashColor = Color.red;
        [SerializeField] private float hurtFlashDuration = 0.1f;

        private Rigidbody2D body;
        private Rigidbody2D.SlideMovement slideMovement;
        private Camera mainCamera;
        private PlayerState state;
        private float hurtFlashTimer;

        public event Action<float, float> OnHealthChanged;
        public event Action OnStatsChanged;
        public event Action OnDeath;
        public event Action OnHurt;

        public PlayerStats Stats { get; private set; }
        public PlayerInventory Inventory { get; private set; }
        public Vector2 AimDirection { get; private set; } = Vector2.right;
        public Vector2 MoveDirection { get; private set; }
        public float CurrentMoveSpeed => data.BaseMoveSpeed * Stats.MoveSpeed;
        public Collider2D BodyCollider => bodyCollider;
        public string CharacterId => data.Id;
        public CharacterData Character => data;
        public bool IsInvincible { get; set; }
        public override bool CanBeHit => state != PlayerState.Dead;
        public virtual int BonusPierce => 0;

        protected virtual void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            slideMovement = CreateSlideMovement();
            Hurtbox.Create(this, spriteRenderer, hurtboxScale, GameLayers.PlayerHurtbox);
            mainCamera = Camera.main;
            Stats = data.BaseStats.Clone();
            CurrentHP = Stats.MaxHP;
            Inventory = new PlayerInventory(this);
            if (data.StartingWeapon != null)
                Inventory.AddWeapon(data.StartingWeapon);
        }

        private void OnEnable() => moveAction.action.Enable();

        private void OnDisable() => moveAction.action.Disable();

        protected virtual void Update()
        {
            if (state == PlayerState.Dead || !GameManager.Instance.IsPlaying)
                return;

            float deltaTime = Time.deltaTime;
            MoveDirection = Vector2.ClampMagnitude(moveAction.action.ReadValue<Vector2>(), 1f);
            UpdateAim();
            UpdateMovementState();
            Recover(deltaTime);
            TickHurtFlash(deltaTime);
            Inventory.TickWeapons(deltaTime);
        }

        protected virtual void FixedUpdate()
        {
            if (state == PlayerState.Dead || !GameManager.Instance.IsPlaying || MoveDirection == Vector2.zero)
                return;

            slideMovement.surfaceUp = -MoveDirection;
            body.Slide(MoveDirection * CurrentMoveSpeed, Time.fixedDeltaTime, slideMovement);
        }

        public override void TakeDamage(DamageInfo info)
        {
            if (state == PlayerState.Dead || IsInvincible)
                return;

            float armorReduction = Mathf.Clamp(Stats.Armor * data.ArmorReductionPerPoint, 0f, data.MaxArmorReduction);
            float damageTaken = Mathf.Max(MinimumDamageTaken, info.Amount * (1f - armorReduction));
            SetHealth(CurrentHP - damageTaken);
            StartHurtFlash();
            OnHurt?.Invoke();
            OnDamaged(info, damageTaken);
            if (CurrentHP <= 0f)
                Die();
        }

        public void Heal(float amount)
        {
            if (state != PlayerState.Dead)
                SetHealth(Mathf.Min(Stats.MaxHP, CurrentHP + amount));
        }

        public void RestoreFullHealth() => Heal(Stats.MaxHP);

        public void ApplyStatModifier(StatType stat, float value, StatModifierKind kind)
        {
            float amount = kind == StatModifierKind.PercentOfBase ? value * data.BaseStats.Get(stat) : value;
            Stats.Add(stat, amount);
            OnStatsChanged?.Invoke();
            if (stat == StatType.MaxHP)
                OnHealthChanged?.Invoke(CurrentHP, Stats.MaxHP);
        }

        public void PlayAttackAnimation(float attackInterval)
        {
            if (animator == null)
                return;
            if (attackClip != null)
                animator.SetFloat(AttackSpeedHash, Mathf.Max(1f, attackClip.length / attackInterval));
            animator.SetTrigger(AttackHash);
        }

        public virtual float ModifyOutgoingDamage(float damage) => damage;

        public virtual void OnLevelChanged(int level) { }

        public virtual bool RollSpellEcho(DamageType damageType) => false;

        protected virtual void OnDamaged(DamageInfo info, float damageTaken) { }

        public virtual void GetClassStats(List<(string Label, string Value)> results) { }

        protected override void Die()
        {
            if (Stats.Revival > 0)
            {
                Stats.Add(StatType.Revival, -1f);
                SetHealth(Stats.MaxHP * data.RevivalHealthRatio);
                return;
            }
            ChangeState(PlayerState.Dead);
        }

        private void SetHealth(float value)
        {
            CurrentHP = value;
            OnHealthChanged?.Invoke(CurrentHP, Stats.MaxHP);
        }

        private Rigidbody2D.SlideMovement CreateSlideMovement() => new Rigidbody2D.SlideMovement
        {
            maxIterations = SlideIterations,
            surfaceSlideAngle = MaxSlideAngle,
            gravitySlipAngle = MaxSlideAngle,
            surfaceUp = Vector2.up,
            surfaceAnchor = Vector2.zero,
            gravity = Vector2.zero,
            selectedCollider = bodyCollider,
            useSimulationMove = true
        };

        private void Recover(float deltaTime)
        {
            if (Stats.Recovery > 0f && CurrentHP < Stats.MaxHP)
                Heal(Stats.Recovery * deltaTime);
        }

        private void UpdateAim()
        {
            if (Mouse.current == null)
                return;

            Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 toMouse = mouseWorld - (Vector2)transform.position;
            if (toMouse != Vector2.zero)
                AimDirection = toMouse.normalized;
            if (AimDirection.x != 0f)
                Face(AimDirection.x < 0f);
        }

        private void UpdateMovementState()
        {
            PlayerState nextState = MoveDirection == Vector2.zero ? PlayerState.Idle : PlayerState.Move;
            if (nextState != state)
                ChangeState(nextState);
        }

        private void ChangeState(PlayerState nextState)
        {
            state = nextState;
            switch (state)
            {
                case PlayerState.Idle:
                    EnterIdle();
                    break;
                case PlayerState.Move:
                    EnterMove();
                    break;
                case PlayerState.Dead:
                    EnterDead();
                    break;
            }
        }

        private void EnterIdle()
        {
            if (animator != null)
                animator.SetBool(IsMovingHash, false);
        }

        private void EnterMove()
        {
            if (animator != null)
                animator.SetBool(IsMovingHash, true);
        }

        private void EnterDead()
        {
            hurtFlashTimer = 0f;
            spriteRenderer.color = Color.white;
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                animator.SetTrigger(DieHash);
            }
            OnDeath?.Invoke();
        }

        private void StartHurtFlash()
        {
            hurtFlashTimer = hurtFlashDuration;
            spriteRenderer.color = hurtFlashColor;
        }

        private void TickHurtFlash(float deltaTime)
        {
            if (hurtFlashTimer <= 0f)
                return;
            hurtFlashTimer -= deltaTime;
            if (hurtFlashTimer <= 0f)
                spriteRenderer.color = Color.white;
        }
    }
}
