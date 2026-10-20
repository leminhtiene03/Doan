using System.Collections.Generic;
using Game.Core;
using Game.Data;
using Game.Enemies;
using UnityEngine;

namespace Game.Weapons
{
    public class SwordSlash : Weapon
    {
        private const float MaxBodyReach = 3f;

        protected override void Attack(Vector2 direction)
        {
            WeaponLevelStats level = CurrentLevel;
            float range = level.Size * Owner.Stats.Area;
            HitEnemiesInArc(Owner.transform.position, direction, range, level.ArcAngle);
            ShowEffect(direction, range);
        }

        private void HitEnemiesInArc(Vector2 origin, Vector2 direction, float range, float arcAngle)
        {
            float minDot = Mathf.Cos(arcAngle * 0.5f * Mathf.Deg2Rad);
            IReadOnlyList<Enemy> enemies = EnemyManager.Instance.Active;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = enemies[i];
                if (!enemy.CanBeHit)
                    continue;

                Vector2 offset = (Vector2)enemy.transform.position - origin;
                float sqrDistance = offset.sqrMagnitude;
                float coarseReach = range + MaxBodyReach;
                if (sqrDistance > coarseReach * coarseReach)
                    continue;

                Vector2 closest = enemy.HurtboxCollider.ClosestPoint(origin);
                if ((closest - origin).sqrMagnitude > range * range)
                    continue;

                Vector2 toEnemy = sqrDistance > 0f ? offset / Mathf.Sqrt(sqrDistance) : direction;
                if (Vector2.Dot(toEnemy, direction) >= minDot)
                    enemy.TakeDamage(CreateDamage(toEnemy));
            }
        }

        private void ShowEffect(Vector2 direction, float range)
        {
            if (Data.EffectPrefab == null)
                return;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            PooledEffect effect = PoolManager.Instance.Get(
                Data.EffectPrefab, Owner.transform.position, Quaternion.Euler(0f, 0f, angle));
            effect.transform.localScale = Vector3.one * range;
        }
    }
}
