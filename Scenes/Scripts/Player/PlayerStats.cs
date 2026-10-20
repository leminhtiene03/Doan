using System;
using Game.Data;
using UnityEngine;

namespace Game.Player
{
    [Serializable]
    public class PlayerStats
    {
        [SerializeField] private float maxHP = 100f;
        [SerializeField] private float recovery;
        [SerializeField] private float armor;
        [SerializeField] private float moveSpeed = 1f;
        [SerializeField] private float might = 1f;
        [SerializeField] private float attackSpeed = 1f;
        [SerializeField] private float area = 1f;
        [SerializeField] private int multishot;
        [SerializeField] private float magnet = 1.5f;
        [SerializeField] private float projectileSpeed = 1f;
        [SerializeField] private float duration = 1f;
        [SerializeField] private float growth = 1f;
        [SerializeField] private float greed = 1f;
        [SerializeField] private int revival;

        public float MaxHP => maxHP;
        public float Recovery => recovery;
        public float Armor => armor;
        public float MoveSpeed => moveSpeed;
        public float Might => might;
        public float AttackSpeed => attackSpeed;
        public float Area => area;
        public int Multishot => multishot;
        public float Magnet => magnet;
        public float ProjectileSpeed => projectileSpeed;
        public float Duration => duration;
        public float Growth => growth;
        public float Greed => greed;
        public int Revival => revival;

        public PlayerStats Clone() => (PlayerStats)MemberwiseClone();

        public float Get(StatType stat) => stat switch
        {
            StatType.MaxHP => maxHP,
            StatType.Recovery => recovery,
            StatType.Armor => armor,
            StatType.MoveSpeed => moveSpeed,
            StatType.Might => might,
            StatType.AttackSpeed => attackSpeed,
            StatType.Area => area,
            StatType.Multishot => multishot,
            StatType.Magnet => magnet,
            StatType.ProjectileSpeed => projectileSpeed,
            StatType.Duration => duration,
            StatType.Growth => growth,
            StatType.Greed => greed,
            StatType.Revival => revival,
            _ => 0f
        };

        public void Add(StatType stat, float amount)
        {
            switch (stat)
            {
                case StatType.MaxHP: maxHP += amount; break;
                case StatType.Recovery: recovery += amount; break;
                case StatType.Armor: armor += amount; break;
                case StatType.MoveSpeed: moveSpeed += amount; break;
                case StatType.Might: might += amount; break;
                case StatType.AttackSpeed: attackSpeed += amount; break;
                case StatType.Area: area += amount; break;
                case StatType.Multishot: multishot += Mathf.RoundToInt(amount); break;
                case StatType.Magnet: magnet += amount; break;
                case StatType.ProjectileSpeed: projectileSpeed += amount; break;
                case StatType.Duration: duration += amount; break;
                case StatType.Growth: growth += amount; break;
                case StatType.Greed: greed += amount; break;
                case StatType.Revival: revival += Mathf.RoundToInt(amount); break;
            }
        }
    }
}
