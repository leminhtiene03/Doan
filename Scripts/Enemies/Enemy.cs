using System;
using Game.Core;
using Game.Data;
using UnityEngine;

namespace Game.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Enemy : Entity
    {
        private enum EnemyState { Spawning, Chase, Attack, Dead }

        private const float KnockbackDuration = 0.1f;
        private const float MaxBodyReach = 3f;

        private static readonly int IdleHash = Animator.StringToHash("Idle");
        private static readonly int MoveHash = Animator.StringToHash("Move");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int DieHash = Animator.StringToHash("Die");

        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private Collider2D bodyCollider;
        [SerializeField] private AnimationClip attackClip;
        [SerializeField] private AnimationClip deathClip;
        [SerializeField] private Vector2 hurtboxScale = new(0.7f, 0.9f);
        [SerializeField] private float faceTurnThreshold = 0.1f;
        [SerializeField] private Color hitFlashColor = Color.red;
        [SerializeField] private Color slowTintColor = new(0.6f, 0.8f, 1f);
        [SerializeField] private float hitFlashDuration = 0.1f;

        private Rigidbody2D body;
        private Hurtbox hurtbox;
        private EnemyState state;
        private float stateTimer;
        private float attackCooldown;
        private bool attackResolved;
        private bool attackLanded;
        private Vector2 knockbackVelocity;
        private float knockbackTimer;
        private float flashTimer;
        private float alpha = 1f;
        private int currentAnimation;
        private bool hasIdleAnimation;
        private float moveSpeed;
        private float slowAmount;
        private float slowTimer;
        private Vector2 pullVelocity;
        private float attackDamage;
        private Vector2 travelDirection;

        public event Action<Enemy> OnDeath;
        public event Action<Enemy> OnDamaged;

        public EnemyData Data { get; private set; }
        public Collider2D BodyCollider => bodyCollider;
        public Collider2D HurtboxCollider => hurtbox.Collider;
        public int ListIndex { get; set; }
        public override bool CanBeHit => state == EnemyState.Chase || state == EnemyState.Attack;
        public bool IsDead => state == EnemyState.Dead;
        public bool IsTraveling => travelDirection != Vector2.zero;
        public bool IsReadyToDespawn => IsDead && stateTimer <= 0f;

        private float AttackDuration => attackClip != null ? attackClip.length : Data.AttackHitTime;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            hurtbox = Hurtbox.Create(this, spriteRenderer, hurtboxScale, GameLayers.EnemyHurtbox);
        }

        public void Init(EnemyData enemyData, DifficultyData difficulty)
        {
            Data = enemyData;
            CurrentHP = enemyData.MaxHP * difficulty.HpMultiplier;
            moveSpeed = enemyData.MoveSpeed * difficulty.SpeedMultiplier;
            slowAmount = 0f;
            slowTimer = 0f;
            pullVelocity = Vector2.zero;
            attackDamage = enemyData.AttackDamage * difficulty.DamageMultiplier;
            SetTravelDirection(Vector2.zero);
            attackCooldown = 0f;
            attackLanded = false;
            knockbackTimer = 0f;
            flashTimer = 0f;
            body.mass = enemyData.Mass;
            body.simulated = true;
            body.linearVelocity = Vector2.zero;
            if (animator != null)
                animator.Rebind();
            hasIdleAnimation = animator != null && animator.HasState(0, IdleHash);
            PlayAnimation(MoveHash);
            EnterSpawning();
        }

        public void Tick(Vector2 playerPosition, Collider2D playerCollider, float deltaTime)
        {
            TickFlash(deltaTime);
            TickSlow(deltaTime);
            attackCooldown -= deltaTime;
            switch (state)
            {
                case EnemyState.Spawning:
                    TickSpawning(deltaTime);
                    break;
                case EnemyState.Chase:
                    TickChase(playerPosition, playerCollider, deltaTime);
                    break;
                case EnemyState.Attack:
                    TickAttack(playerPosition, playerCollider, deltaTime);
                    break;
                case EnemyState.Dead:
                    TickDead(deltaTime);
                    break;
            }
        }

        public void SetTravelDirection(Vector2 direction)
        {
            travelDirection = direction.normalized;
            body.excludeLayers = IsTraveling ? 1 << gameObject.layer : 0;
        }

        public bool ConsumeLandedAttack()
        {
            if (!attackLanded)
                return false;
            attackLanded = false;
            return true;
        }

        public DamageInfo CreateAttackDamage()
            => new DamageInfo(attackDamage, DamageType.Physical, Vector2.zero, this);

        public override void TakeDamage(DamageInfo info)
        {
            if (!CanBeHit)
                return;

            float resist = info.Type == DamageType.Physical ? Data.ArmorPercent : Data.MagicResistPercent;
            CurrentHP -= info.Amount * (1f - resist);
            StartKnockback(info.Knockback * (1f - Data.KnockbackResist));
            StartFlash();
            if (CurrentHP <= 0f)
                Die();
            else
                OnDamaged?.Invoke(this);
        }

        public override void ApplySlow(float amount, float duration)
        {
            if (!CanBeHit || amount <= 0f)
                return;
            slowAmount = Mathf.Max(slowAmount, amount);
            slowTimer = Mathf.Max(slowTimer, duration);
            ApplyTint();
        }

        public override void AddPull(Vector2 velocity) => pullVelocity += velocity;

        public void Teleport(Vector2 position)
        {
            body.position = position;
            transform.position = position;
        }

        protected override void Die()
        {
            state = EnemyState.Dead;
            stateTimer = (deathClip != null ? deathClip.length : 0f) + Data.CorpseDuration;
            attackLanded = false;
            body.linearVelocity = Vector2.zero;
            body.simulated = false;
            flashTimer = 0f;
            ApplyTint();
            PlayAnimation(DieHash);
            OnDeath?.Invoke(this);
        }

        private void TickDead(float deltaTime)
        {
            stateTimer -= deltaTime;
            if (stateTimer >= Data.CorpseFadeDuration)
                return;
            alpha = Data.CorpseFadeDuration > 0f ? Mathf.Clamp01(stateTimer / Data.CorpseFadeDuration) : 0f;
            ApplyTint();
        }

        private void EnterSpawning()
        {
            state = EnemyState.Spawning;
            stateTimer = 0f;
            alpha = 0f;
            ApplyTint();
        }

        private void TickSpawning(float deltaTime)
        {
            stateTimer += deltaTime;
            float progress = Data.SpawnFadeDuration > 0f ? stateTimer / Data.SpawnFadeDuration : 1f;
            alpha = Mathf.Clamp01(progress);
            ApplyTint();
            if (progress >= 1f)
                state = EnemyState.Chase;
        }

        private void TickChase(Vector2 playerPosition, Collider2D playerCollider, float deltaTime)
        {
            Vector2 moveDirection = IsTraveling ? travelDirection : (playerPosition - body.position).normalized;
            if (Mathf.Abs(moveDirection.x) >= faceTurnThreshold)
                Face(moveDirection.x < 0f);
            if (!IsInAttackRange(playerPosition, playerCollider))
            {
                SetVelocity(moveDirection * (moveSpeed * (1f - slowAmount)), deltaTime);
                SetLocomotionAnimation(MoveHash);
                return;
            }

            SetVelocity(Vector2.zero, deltaTime);
            SetLocomotionAnimation(hasIdleAnimation ? IdleHash : MoveHash);
            if (attackCooldown <= 0f)
                EnterAttack();
        }

        private void EnterAttack()
        {
            state = EnemyState.Attack;
            stateTimer = 0f;
            attackCooldown = Data.AttackInterval;
            attackResolved = false;
            PlayAnimation(AttackHash);
        }

        private void TickAttack(Vector2 playerPosition, Collider2D playerCollider, float deltaTime)
        {
            stateTimer += deltaTime;
            SetVelocity(Vector2.zero, deltaTime);
            if (!attackResolved && stateTimer >= Data.AttackHitTime)
            {
                attackResolved = true;
                attackLanded = IsInAttackRange(playerPosition, playerCollider);
            }

            if (stateTimer >= AttackDuration)
                state = EnemyState.Chase;
        }

        private bool IsInAttackRange(Vector2 playerPosition, Collider2D playerCollider)
        {
            float coarseReach = Data.AttackRange + MaxBodyReach;
            if ((playerPosition - body.position).sqrMagnitude > coarseReach * coarseReach)
                return false;

            ColliderDistance2D gap = bodyCollider.Distance(playerCollider);
            return gap.isValid && gap.distance <= Data.AttackRange;
        }

        private void StartKnockback(Vector2 displacement)
        {
            if (displacement == Vector2.zero)
                return;
            knockbackVelocity = displacement / KnockbackDuration;
            knockbackTimer = KnockbackDuration;
        }

        private void SetVelocity(Vector2 desired, float deltaTime)
        {
            Vector2 pull = pullVelocity;
            pullVelocity = Vector2.zero;
            if (knockbackTimer > 0f)
            {
                knockbackTimer -= deltaTime;
                body.linearVelocity = knockbackVelocity + pull;
                return;
            }
            body.linearVelocity = desired + pull;
        }

        private void SetLocomotionAnimation(int stateHash)
        {
            if (currentAnimation != stateHash)
                PlayAnimation(stateHash);
        }

        private void PlayAnimation(int stateHash)
        {
            currentAnimation = stateHash;
            if (animator != null)
                animator.Play(stateHash, 0, 0f);
        }

        private void TickSlow(float deltaTime)
        {
            if (slowTimer <= 0f)
                return;
            slowTimer -= deltaTime;
            if (slowTimer > 0f)
                return;
            slowAmount = 0f;
            ApplyTint();
        }

        private void StartFlash()
        {
            flashTimer = hitFlashDuration;
            ApplyTint();
        }

        private void TickFlash(float deltaTime)
        {
            if (flashTimer <= 0f)
                return;
            flashTimer -= deltaTime;
            if (flashTimer <= 0f)
                ApplyTint();
        }

        private void ApplyTint()
        {
            Color color = flashTimer > 0f ? hitFlashColor : slowAmount > 0f ? slowTintColor : Color.white;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}
