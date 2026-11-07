using Game.Core;
using Game.Weapons;
using UnityEngine;

namespace Game.Enemies
{
    public class RangedEnemy : Enemy
    {
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float retreatRange = 3f;
        [SerializeField] private float projectileSpeed = 7f;
        [SerializeField] private float projectileRange = 10f;
        [SerializeField] private float projectileScale = 1f;
        [SerializeField] private float sightRadius = 0.15f;
        [SerializeField] private float strafeSwitchInterval = 2f;

        private readonly RaycastHit2D[] sightHits = new RaycastHit2D[8];
        private ContactFilter2D sightFilter;
        private bool hasSightFilter;
        private float fireCooldown;
        private bool isShooting;
        private bool hasFired;
        private float shootTimer;
        private float strafeSign = 1f;
        private float strafeTimer;

        private Vector2 FireOrigin => firePoint != null ? (Vector2)firePoint.position : Body.position;

        protected override void ResetBehavior()
        {
            EnsureSightFilter();
            fireCooldown = 0f;
            isShooting = false;
            hasFired = false;
            shootTimer = 0f;
            strafeSign = Random.value < 0.5f ? -1f : 1f;
            strafeTimer = strafeSwitchInterval;
        }

        protected override void TickChase(Vector2 playerPosition, Collider2D playerCollider, float deltaTime)
        {
            if (IsTraveling)
            {
                FaceToward(TravelDirection);
                Move(TravelDirection, deltaTime);
                return;
            }

            fireCooldown -= deltaTime;
            if (isShooting)
            {
                TickShooting(playerPosition, deltaTime);
                return;
            }

            Vector2 toPlayer = playerPosition - Body.position;
            float sqrDistance = toPlayer.sqrMagnitude;
            Vector2 direction = sqrDistance > 0f ? toPlayer / Mathf.Sqrt(sqrDistance) : Vector2.right;
            FaceToward(direction);

            if (sqrDistance < retreatRange * retreatRange)
                Move(-direction, deltaTime);
            else if (sqrDistance > Data.AttackRange * Data.AttackRange)
                Move(direction, deltaTime);
            else if (!HasClearShot(playerPosition))
                Move(GetStrafeDirection(direction, deltaTime), deltaTime);
            else
                HoldPosition(deltaTime);
        }

        private void HoldPosition(float deltaTime)
        {
            SetVelocity(Vector2.zero, deltaTime);
            SetLocomotionAnimation(HasIdleAnimation ? IdleHash : MoveHash);
            if (fireCooldown <= 0f)
                StartShooting();
        }

        private void Move(Vector2 direction, float deltaTime)
        {
            SetVelocity(direction * CurrentMoveSpeed, deltaTime);
            SetLocomotionAnimation(MoveHash);
        }

        private Vector2 GetStrafeDirection(Vector2 toPlayer, float deltaTime)
        {
            strafeTimer -= deltaTime;
            if (strafeTimer <= 0f)
            {
                strafeSign = -strafeSign;
                strafeTimer = strafeSwitchInterval;
            }
            return new Vector2(-toPlayer.y, toPlayer.x) * strafeSign;
        }

        private void StartShooting()
        {
            isShooting = true;
            hasFired = false;
            shootTimer = 0f;
            fireCooldown = Data.AttackInterval;
            PlayAnimation(AttackHash);
        }

        private void TickShooting(Vector2 playerPosition, float deltaTime)
        {
            SetVelocity(Vector2.zero, deltaTime);
            shootTimer += deltaTime;
            if (!hasFired && shootTimer >= Data.AttackHitTime)
            {
                hasFired = true;
                Fire(playerPosition);
            }

            if (shootTimer >= AttackDuration)
                isShooting = false;
        }

        private void Fire(Vector2 playerPosition)
        {
            if (projectilePrefab == null)
                return;

            Vector2 origin = FireOrigin;
            Vector2 direction = (playerPosition - origin).normalized;
            FaceToward(direction);
            var shot = new ProjectileShot(
                origin, direction, projectileSpeed, projectileRange, 0, projectileScale, 0f,
                CreateAttackDamage(), default, GameLayers.EnemyProjectile);
            ProjectileManager.Instance.Fire(projectilePrefab, shot);
        }

        private bool HasClearShot(Vector2 playerPosition)
        {
            Vector2 origin = FireOrigin;
            Vector2 toPlayer = playerPosition - origin;
            float distance = toPlayer.magnitude;
            if (distance <= 0f)
                return true;

            int count = Physics2D.CircleCast(origin, sightRadius, toPlayer / distance, sightFilter, sightHits, distance);
            for (int i = 0; i < count; i++)
            {
                if (!sightHits[i].collider.transform.IsChildOf(transform))
                    return false;
            }
            return true;
        }

        private void EnsureSightFilter()
        {
            if (hasSightFilter)
                return;
            sightFilter.SetLayerMask((1 << GameLayers.Obstacle) | (1 << gameObject.layer));
            sightFilter.useTriggers = false;
            hasSightFilter = true;
        }
    }
}
