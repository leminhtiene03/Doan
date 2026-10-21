using UnityEngine;

namespace Game.Core
{
    public readonly struct DamageInfo
    {
        public DamageInfo(float amount, DamageType type, Vector2 knockback, Entity source)
        {
            Amount = amount;
            Type = type;
            Knockback = knockback;
            Source = source;
        }

        public float Amount { get; }
        public DamageType Type { get; }
        public Vector2 Knockback { get; }
        public Entity Source { get; }
    }
}
