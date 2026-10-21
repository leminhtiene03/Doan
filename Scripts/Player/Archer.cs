using System.Collections.Generic;
using Game.Data;
using UnityEngine;

namespace Game.Player
{
    public class Archer : Player
    {
        [SerializeField] private int bonusPierce = 1;
        [SerializeField] private int levelsPerPassiveMultishot = 20;
        [SerializeField] private int maxPassiveMultishot = 2;

        private int passiveMultishot;

        public override int BonusPierce => bonusPierce;

        public override void OnLevelChanged(int level)
        {
            int earned = Mathf.Min(maxPassiveMultishot, level / levelsPerPassiveMultishot);
            if (earned <= passiveMultishot)
                return;

            ApplyStatModifier(StatType.Multishot, earned - passiveMultishot, StatModifierKind.Flat);
            passiveMultishot = earned;
        }

        public override void GetClassStats(List<(string Label, string Value)> results)
        {
            results.Add(("Bonus Pierce", $"+{bonusPierce}"));
            results.Add(("Eagle Eye", $"+{passiveMultishot} Multishot"));
        }
    }
}
