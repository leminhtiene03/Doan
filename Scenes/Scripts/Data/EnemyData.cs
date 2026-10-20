using Game.Enemies;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Enemy Data", fileName = "EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private Enemy prefab;
        [SerializeField] private float maxHP;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float mass = 1f;
        [SerializeField] private float attackDamage;
        [SerializeField] private float attackInterval = 1f;
        [SerializeField] private float attackRange = 0.15f;
        [SerializeField] private float attackHitTime = 0.25f;
        [SerializeField, Range(0f, 0.8f)] private float armorPercent;
        [SerializeField, Range(0f, 0.8f)] private float magicResistPercent;
        [SerializeField, Range(0f, 1f)] private float knockbackResist;
        [SerializeField] private int xpDrop;
        [SerializeField, Range(0f, 1f)] private float coinDropChance;
        [SerializeField] private int coinAmount;
        [SerializeField] private bool dropsChest;
        [SerializeField] private float spawnFadeDuration = 0.3f;
        [SerializeField] private float corpseDuration = 3f;
        [SerializeField] private float corpseFadeDuration = 0.5f;

        public string Id => id;
        public string DisplayName => displayName;
        public Enemy Prefab => prefab;
        public float MaxHP => maxHP;
        public float MoveSpeed => moveSpeed;
        public float Mass => mass;
        public float AttackDamage => attackDamage;
        public float AttackInterval => attackInterval;
        public float AttackRange => attackRange;
        public float AttackHitTime => attackHitTime;
        public float ArmorPercent => armorPercent;
        public float MagicResistPercent => magicResistPercent;
        public float KnockbackResist => knockbackResist;
        public int XpDrop => xpDrop;
        public float CoinDropChance => coinDropChance;
        public int CoinAmount => coinAmount;
        public bool DropsChest => dropsChest;
        public float SpawnFadeDuration => spawnFadeDuration;
        public float CorpseDuration => corpseDuration;
        public float CorpseFadeDuration => corpseFadeDuration;
    }
}
