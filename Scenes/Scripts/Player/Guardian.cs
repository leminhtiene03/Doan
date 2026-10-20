using System.Collections.Generic;
using Game.Core;
using Game.Data;
using UnityEngine;

namespace Game.Player
{
    public class Guardian : Player
    {
        [SerializeField, Range(0f, 1f)] private float thornsRatio = 0.05f;
        [SerializeField] private float maxThornsDamage = 10f;
        [SerializeField] private int levelsPerPassiveStep = 10;
        [SerializeField] private float maxHPPerPassiveStep = 10f;

        private int passiveSteps;

        public override void OnLevelChanged(int level)
        {
            int earned = level / levelsPerPassiveStep;
            if (earned <= passiveSteps)
                return;

            ApplyStatModifier(StatType.MaxHP, (earned - passiveSteps) * maxHPPerPassiveStep, StatModifierKind.Flat);
            passiveSteps = earned;
        }

        public override void GetClassStats(List<(string Label, string Value)> results)
        {
            results.Add(("Thorns", $"{thornsRatio * 100f:0}% (max {maxThornsDamage:0})"));
            results.Add(("Bulwark", $"+{passiveSteps * maxHPPerPassiveStep:0} Max HP"));
        }

        protected override void OnDamaged(DamageInfo info, float damageTaken)
        {
            Entity attacker = info.Source;
            if (info.Type != DamageType.Physical || attacker == null || attacker == this || !attacker.CanBeHit)
                return;

            float reflected = Mathf.Min(damageTaken * thornsRatio, maxThornsDamage);
            attacker.TakeDamage(new DamageInfo(reflected, DamageType.Physical, Vector2.zero, this));
        }
    }
}
