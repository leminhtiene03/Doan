using System.Collections.Generic;
using Game.Core;
using Game.Data;
using UnityEngine;

namespace Game.Player
{
    public class Mage : Player
    {
        [SerializeField, Range(0f, 1f)] private float spellEchoChance = 0.1f;
        [SerializeField] private int levelsPerPassiveStep = 10;
        [SerializeField] private float attackSpeedPerPassiveStep = 0.02f;
        [SerializeField] private int maxPassiveSteps = 5;

        private int passiveSteps;

        public override bool RollSpellEcho(DamageType damageType)
            => damageType == DamageType.Magic && Random.value < spellEchoChance;

        public override void OnLevelChanged(int level)
        {
            int earned = Mathf.Min(maxPassiveSteps, level / levelsPerPassiveStep);
            if (earned <= passiveSteps)
                return;

            ApplyStatModifier(StatType.AttackSpeed, (earned - passiveSteps) * attackSpeedPerPassiveStep, StatModifierKind.Flat);
            passiveSteps = earned;
        }

        public override void GetClassStats(List<(string Label, string Value)> results)
        {
            results.Add(("Spell Echo", $"{spellEchoChance * 100f:0}%"));
            results.Add(("Mastery", $"+{passiveSteps * attackSpeedPerPassiveStep * 100f:0}% Attack Speed"));
        }
    }
}
