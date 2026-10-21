using Game.Core;
using Game.Data;
using UnityEngine;

namespace Game.Weapons
{
    public class ProjectileWeapon : Weapon
    {
        private int burstRemaining;
        private float burstTimer;
        private Vector2 burstDirection;

        protected override void Attack(Vector2 direction)
        {
            WeaponLevelStats level = CurrentLevel;
            int count = Mathf.Max(1, level.Projectiles + Owner.Stats.Multishot);
            if (level.BurstInterval > 0f)
            {
                Fire(direction);
                burstRemaining = count - 1;
                burstTimer = level.BurstInterval;
                burstDirection = direction;
                return;
            }

            float startAngle = -(count - 1) * 0.5f * level.ArcAngle;
            for (int i = 0; i < count; i++)
                Fire(Quaternion.Euler(0f, 0f, startAngle + i * level.ArcAngle) * direction);
        }

        protected override void TickActive(float deltaTime)
        {
            if (burstRemaining <= 0)
                return;

            burstTimer -= deltaTime;
            if (burstTimer > 0f)
                return;

            if (TryGetAimDirection(out Vector2 direction))
                burstDirection = direction;
            Fire(burstDirection);
            burstRemaining--;
            burstTimer += CurrentLevel.BurstInterval;
        }

        private void Fire(Vector2 direction)
        {
            WeaponLevelStats level = CurrentLevel;
            var shot = new ProjectileShot(
                Owner.transform.position,
                direction,
                level.ProjectileSpeed * Owner.Stats.ProjectileSpeed,
                level.Range,
                level.Pierce + Owner.BonusPierce,
                level.Size * Owner.Stats.Area,
                level.ImpactRadius * Owner.Stats.Area,
                CreateDamage(direction),
                new HitEffect(level.SlowAmount, level.SlowDuration * Owner.Stats.Duration),
                GameLayers.PlayerProjectile);
            ProjectileManager.Instance.Fire(Data.ProjectilePrefab, shot);
        }
    }
}
