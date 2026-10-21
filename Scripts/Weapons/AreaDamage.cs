using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Weapons
{
    public static class AreaDamage
    {
        private static readonly List<Collider2D> Hits = new(64);

        public static void Apply(Vector2 center, float radius, int layerMask, in DamageInfo damage, in HitEffect effect)
        {
            var filter = new ContactFilter2D();
            filter.SetLayerMask(layerMask);
            Physics2D.OverlapCircle(center, radius, filter, Hits);

            float knockback = damage.Knockback.magnitude;
            for (int i = 0; i < Hits.Count; i++)
            {
                if (!Hits[i].TryGetComponent(out Hurtbox hurtbox) || !hurtbox.Owner.CanBeHit)
                    continue;

                Vector2 away = ((Vector2)hurtbox.transform.position - center).normalized;
                hurtbox.Owner.TakeDamage(new DamageInfo(damage.Amount, damage.Type, away * knockback, damage.Source));
                effect.ApplyTo(hurtbox.Owner);
            }
        }
    }
}
