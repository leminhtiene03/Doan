using System.Collections.Generic;
using Game.Core;
using Game.Data;
using Game.Enemies;
using UnityEngine;

namespace Game.Weapons
{
    public class SoulBlades : Weapon
    {
        private static readonly List<Collider2D> Hits = new(16);

        [SerializeField] private SpriteRenderer bladePrefab;
        [SerializeField] private float orbitRadius = 1.4f;
        [SerializeField] private float orbitSpeed = 120f;

        private readonly List<Blade> blades = new();

        public override Sprite PreviewSprite => bladePrefab != null ? bladePrefab.sprite : null;
        private float orbitAngle;

        private class Blade
        {
            public SpriteRenderer Renderer;
            public Enemy Target;
            public bool IsReturning;
            public float Cooldown;
            public readonly HashSet<Entity> HitThisDash = new();
        }

        protected override void Attack(Vector2 direction) { }

        public override void Unequip()
        {
            for (int i = 0; i < blades.Count; i++)
                PoolManager.Instance.Release(blades[i].Renderer);
            blades.Clear();
        }

        protected override void TickActive(float deltaTime)
        {
            WeaponLevelStats level = CurrentLevel;
            int count = Mathf.Max(1, level.Projectiles + Owner.Stats.Multishot);
            EnsureBladeCount(count);
            orbitAngle += orbitSpeed * deltaTime;

            Vector2 center = Owner.transform.position;
            for (int i = 0; i < blades.Count; i++)
            {
                Vector2 slot = center + EnemyManager.DirectionFromAngle(orbitAngle + i * 360f / blades.Count) * orbitRadius;
                TickBlade(blades[i], slot, level, deltaTime);
            }
        }

        private void TickBlade(Blade blade, Vector2 slot, WeaponLevelStats level, float deltaTime)
        {
            Transform bladeTransform = blade.Renderer.transform;
            Vector2 position = bladeTransform.position;
            float speed = level.ProjectileSpeed * Owner.Stats.ProjectileSpeed;
            blade.Cooldown -= deltaTime;

            if (blade.Target == null && !blade.IsReturning)
            {
                bladeTransform.position = slot;
                if (blade.Cooldown <= 0f)
                    StartDash(blade, slot, level.Range);
                return;
            }

            if (blade.Target != null && !blade.Target.CanBeHit)
            {
                blade.Target = null;
                blade.IsReturning = true;
            }

            Vector2 destination = blade.IsReturning ? slot : (Vector2)blade.Target.transform.position;
            Vector2 next = Vector2.MoveTowards(position, destination, speed * deltaTime);
            Vector2 heading = next - position;
            bladeTransform.position = next;
            if (heading != Vector2.zero)
                bladeTransform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(heading.y, heading.x) * Mathf.Rad2Deg);

            if (!blade.IsReturning)
            {
                HitAround(blade, next, level.ImpactRadius * Owner.Stats.Area);
                if ((destination - next).sqrMagnitude < 0.01f)
                {
                    blade.Target = null;
                    blade.IsReturning = true;
                }
            }
            else if ((slot - next).sqrMagnitude < 0.01f)
            {
                blade.IsReturning = false;
                blade.Cooldown = level.AttackInterval / Owner.Stats.AttackSpeed;
            }
        }

        private void StartDash(Blade blade, Vector2 from, float range)
        {
            Enemy target = EnemyManager.Instance.FindNearest(from, range);
            if (target == null)
                return;
            blade.Target = target;
            blade.HitThisDash.Clear();
        }

        private void HitAround(Blade blade, Vector2 position, float radius)
        {
            var filter = new ContactFilter2D();
            filter.SetLayerMask(1 << GameLayers.EnemyHurtbox);
            Physics2D.OverlapCircle(position, radius, filter, Hits);
            for (int i = 0; i < Hits.Count; i++)
            {
                if (!Hits[i].TryGetComponent(out Hurtbox hurtbox) || !hurtbox.Owner.CanBeHit)
                    continue;
                if (!blade.HitThisDash.Add(hurtbox.Owner))
                    continue;
                Vector2 away = ((Vector2)hurtbox.transform.position - position).normalized;
                hurtbox.Owner.TakeDamage(CreateDamage(away));
            }
        }

        private void EnsureBladeCount(int count)
        {
            while (blades.Count < count)
            {
                SpriteRenderer renderer = PoolManager.Instance.Get(bladePrefab, Owner.transform.position, Quaternion.identity);
                blades.Add(new Blade { Renderer = renderer, Cooldown = blades.Count * 0.2f });
            }
        }
    }
}
