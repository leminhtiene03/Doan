using System.Collections.Generic;
using Game.Core;
using Game.Data;
using Game.Enemies;
using UnityEngine;

namespace Game.Weapons
{
    public class OrbitingOrbs : Weapon
    {
        private static readonly List<Collider2D> Hits = new(32);

        [SerializeField] private SpriteRenderer orbPrefab;
        [SerializeField] private float hitCooldown = 0.5f;

        private readonly List<SpriteRenderer> orbs = new();

        public override Sprite PreviewSprite => orbPrefab != null ? orbPrefab.sprite : null;
        private readonly List<Dictionary<Entity, float>> nextHitTimes = new();
        private float activeTimer;
        private float orbitAngle;

        protected override void Attack(Vector2 direction)
            => activeTimer = CurrentLevel.Duration * Owner.Stats.Duration;

        public override void Unequip() => SetOrbCount(0);

        protected override void TickActive(float deltaTime)
        {
            if (activeTimer <= 0f)
                return;

            activeTimer -= deltaTime;
            if (activeTimer <= 0f)
            {
                SetOrbCount(0);
                return;
            }

            WeaponLevelStats level = CurrentLevel;
            int count = Mathf.Max(1, level.Projectiles + Owner.Stats.Multishot);
            SetOrbCount(count);
            orbitAngle += level.ProjectileSpeed * Owner.Stats.ProjectileSpeed * deltaTime;

            Vector2 center = Owner.transform.position;
            float orbitRadius = level.Range * Owner.Stats.Area;
            float hitRadius = level.ImpactRadius * Owner.Stats.Area;
            Vector3 scale = Vector3.one * (level.Size * Owner.Stats.Area);
            for (int i = 0; i < count; i++)
            {
                Vector2 position = center + EnemyManager.DirectionFromAngle(orbitAngle + i * 360f / count) * orbitRadius;
                orbs[i].transform.position = position;
                orbs[i].transform.localScale = scale;
                HitAround(i, position, hitRadius);
            }
        }

        private void HitAround(int orbIndex, Vector2 position, float radius)
        {
            var filter = new ContactFilter2D();
            filter.SetLayerMask(1 << GameLayers.EnemyHurtbox);
            Physics2D.OverlapCircle(position, radius, filter, Hits);

            Dictionary<Entity, float> cooldowns = nextHitTimes[orbIndex];
            for (int i = 0; i < Hits.Count; i++)
            {
                if (!Hits[i].TryGetComponent(out Hurtbox hurtbox) || !hurtbox.Owner.CanBeHit)
                    continue;
                if (cooldowns.TryGetValue(hurtbox.Owner, out float readyTime) && readyTime > Time.time)
                    continue;

                cooldowns[hurtbox.Owner] = Time.time + hitCooldown;
                Vector2 away = ((Vector2)hurtbox.transform.position - position).normalized;
                hurtbox.Owner.TakeDamage(CreateDamage(away));
            }
        }

        private void SetOrbCount(int count)
        {
            while (orbs.Count < count)
            {
                orbs.Add(PoolManager.Instance.Get(orbPrefab, Owner.transform.position, Quaternion.identity));
                if (nextHitTimes.Count < orbs.Count)
                    nextHitTimes.Add(new Dictionary<Entity, float>());
            }

            while (orbs.Count > count)
            {
                PoolManager.Instance.Release(orbs[^1]);
                orbs.RemoveAt(orbs.Count - 1);
            }
        }
    }
}
