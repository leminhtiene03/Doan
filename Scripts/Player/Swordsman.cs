using System.Collections.Generic;
using UnityEngine;

namespace Game.Player
{
    public class Swordsman : Player
    {
        [SerializeField] private float critChance = 0.10f;
        [SerializeField] private float critDamage = 1.5f;

        public override float ModifyOutgoingDamage(float damage)
            => Random.value < critChance ? damage * critDamage : damage;

        public override void GetClassStats(List<(string Label, string Value)> results)
        {
            results.Add(("Crit Chance", $"{critChance * 100f:0}%"));
            results.Add(("Crit Damage", $"x{critDamage:0.0}"));
        }
    }
}
