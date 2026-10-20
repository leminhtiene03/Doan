using Game.Core;
using Game.Data;
using Game.Enemies;
using UnityEngine;

namespace Game.Weapons
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite[] flightFrames;
        [SerializeField] private float frameRate = 12f;
        [SerializeField] private PooledEffect impactEffect;
        [SerializeField] private float impactEffectScalePerRadius = 2f;
        [SerializeField] private ProjectileMotion motion;
        [SerializeField] private float spinSpeed;
        [SerializeField] private float turnRate = 540f;
        [SerializeField] private float catchRadius = 0.4f;
        [SerializeField] private float maxReturnTime = 4f;

        private Rigidbody2D body;
        private DamageInfo damage;
        private HitEffect hitEffect;
        private float speed;
        private float distanceLeft;
        private int pierceLeft;
        private float impactRadius;
        private float animationTime;
        private float searchRadius;
        private bool isReturning;
        private float returnTime;
        private Enemy homingTarget;

        public Sprite Sprite => spriteRenderer != null ? spriteRenderer.sprite : null;
        public int ListIndex { get; set; }
        public bool IsFinished { get; private set; }

        private void Awake() => body = GetComponent<Rigidbody2D>();

        public void Launch(in ProjectileShot shot)
        {
            float angle = Mathf.Atan2(shot.Direction.y, shot.Direction.x) * Mathf.Rad2Deg;
            gameObject.layer = shot.Layer;
            transform.SetPositionAndRotation(shot.Origin, Quaternion.Euler(0f, 0f, angle));
            transform.localScale = Vector3.one * shot.Scale;
            body.position = shot.Origin;
            body.rotation = angle;
            body.linearVelocity = shot.Direction * shot.Speed;
            damage = shot.Damage;
            hitEffect = shot.HitEffect;
            speed = shot.Speed;
            distanceLeft = shot.Range;
            pierceLeft = shot.Pierce;
            impactRadius = shot.ImpactRadius;
            animationTime = 0f;
            searchRadius = shot.Range;
            isReturning = false;
            returnTime = 0f;
            homingTarget = null;
            IsFinished = false;
        }

        public void Tick(float deltaTime)
        {
            switch (motion)
            {
                case ProjectileMotion.Boomerang:
                    TickBoomerang(deltaTime);
                    break;
                case ProjectileMotion.Homing:
                    TickHoming(deltaTime);
                    TickRange(deltaTime);
                    break;
                default:
                    TickRange(deltaTime);
                    break;
            }

            if (spinSpeed != 0f)
                body.rotation += spinSpeed * deltaTime;
            AnimateFlight(deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsFinished || !other.TryGetComponent(out Hurtbox hurtbox) || !hurtbox.Owner.CanBeHit)
                return;

            if (impactRadius > 0f)
            {
                Explode(body.position);
            }
            else
            {
                hurtbox.Owner.TakeDamage(damage);
                hitEffect.ApplyTo(hurtbox.Owner);
            }

            pierceLeft--;
            if (pierceLeft < 0)
                IsFinished = true;
        }

        private void TickRange(float deltaTime)
        {
            distanceLeft -= speed * deltaTime;
            if (distanceLeft <= 0f)
                IsFinished = true;
        }

        private void TickBoomerang(float deltaTime)
        {
            if (!isReturning)
            {
                distanceLeft -= speed * deltaTime;
                isReturning = distanceLeft <= 0f;
                return;
            }

            returnTime += deltaTime;
            Vector2 toOwner = (Vector2)damage.Source.transform.position - body.position;
            if (toOwner.sqrMagnitude <= catchRadius * catchRadius || returnTime >= maxReturnTime)
            {
                IsFinished = true;
                return;
            }
            body.linearVelocity = toOwner.normalized * speed;
        }

        private void TickHoming(float deltaTime)
        {
            if (homingTarget == null || !homingTarget.CanBeHit)
                homingTarget = EnemyManager.Instance.FindNearest(body.position, searchRadius);
            if (homingTarget == null)
                return;

            Vector2 desired = ((Vector2)homingTarget.transform.position - body.position).normalized;
            Vector2 current = body.linearVelocity.normalized;
            Vector2 heading = Vector3.RotateTowards(current, desired, turnRate * Mathf.Deg2Rad * deltaTime, 0f);
            body.linearVelocity = heading * speed;
            body.rotation = Mathf.Atan2(heading.y, heading.x) * Mathf.Rad2Deg;
        }

        private void Explode(Vector2 center)
        {
            AreaDamage.Apply(center, impactRadius, Physics2D.GetLayerCollisionMask(gameObject.layer), damage, hitEffect);
            ShowImpact(center);
        }

        private void ShowImpact(Vector2 center)
        {
            if (impactEffect == null)
                return;

            PooledEffect effect = PoolManager.Instance.Get(impactEffect, center, Quaternion.identity);
            effect.transform.localScale = Vector3.one * (impactRadius * impactEffectScalePerRadius);
        }

        private void AnimateFlight(float deltaTime)
        {
            if (flightFrames == null || flightFrames.Length < 2)
                return;

            animationTime += deltaTime;
            spriteRenderer.sprite = flightFrames[(int)(animationTime * frameRate) % flightFrames.Length];
        }
    }
}
