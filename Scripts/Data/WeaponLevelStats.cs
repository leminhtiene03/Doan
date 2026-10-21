using System;
using UnityEngine;

namespace Game.Data
{
    [Serializable]
    public struct WeaponLevelStats
    {
        [SerializeField] private float damage;
        [SerializeField] private float attackInterval;
        [SerializeField] private float size;
        [SerializeField] private int projectiles;
        [SerializeField] private float arcAngle;
        [SerializeField] private float knockback;
        [SerializeField] private float projectileSpeed;
        [SerializeField] private float range;
        [SerializeField] private int pierce;
        [SerializeField] private float impactRadius;
        [SerializeField] private float burstInterval;
        [SerializeField, Range(0f, 1f)] private float slowAmount;
        [SerializeField] private float slowDuration;
        [SerializeField] private float duration;
        [SerializeField] private int chainCount;
        [SerializeField] private float pullStrength;
        [SerializeField, TextArea] private string upgradeDescription;

        public float Damage => damage;
        public float AttackInterval => attackInterval;
        public float Size => size;
        public int Projectiles => projectiles;
        public float ArcAngle => arcAngle;
        public float Knockback => knockback;
        public float ProjectileSpeed => projectileSpeed;
        public float Range => range;
        public int Pierce => pierce;
        public float ImpactRadius => impactRadius;
        public float BurstInterval => burstInterval;
        public float SlowAmount => slowAmount;
        public float SlowDuration => slowDuration;
        public float Duration => duration;
        public int ChainCount => chainCount;
        public float PullStrength => pullStrength;
        public string UpgradeDescription => upgradeDescription;
    }
}
