using Game.Core;
using UnityEngine;

namespace Game.Weapons
{
    public readonly struct ProjectileShot
    {
        public ProjectileShot(
            Vector2 origin, Vector2 direction, float speed, float range, int pierce, float scale,
            float impactRadius, DamageInfo damage, HitEffect hitEffect, int layer)
        {
            Origin = origin;
            Direction = direction;
            Speed = speed;
            Range = range;
            Pierce = pierce;
            Scale = scale;
            ImpactRadius = impactRadius;
            Damage = damage;
            HitEffect = hitEffect;
            Layer = layer;
        }

        public Vector2 Origin { get; }
        public Vector2 Direction { get; }
        public float Speed { get; }
        public float Range { get; }
        public int Pierce { get; }
        public float Scale { get; }
        public float ImpactRadius { get; }
        public DamageInfo Damage { get; }
        public HitEffect HitEffect { get; }
        public int Layer { get; }
    }
}
