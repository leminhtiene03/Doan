using Game.Player;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Character Data", fileName = "CharacterData")]
    public class CharacterData : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite portrait;
        [SerializeField] private float baseMoveSpeed = 5f;
        [SerializeField, Range(0f, 1f)] private float revivalHealthRatio = 0.5f;
        [SerializeField, Range(0f, 0.2f)] private float armorReductionPerPoint = 0.08f;
        [SerializeField, Range(0f, 0.9f)] private float maxArmorReduction = 0.6f;
        [SerializeField] private PlayerStats baseStats = new();
        [SerializeField] private WeaponData startingWeapon;
        [SerializeField] private Game.Player.Player playerPrefab;
        [SerializeField] private WeaponData[] weaponPool = new WeaponData[0];
        [SerializeField] private PassiveItemData[] passivePool = new PassiveItemData[0];

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Portrait => portrait;
        public float BaseMoveSpeed => baseMoveSpeed;
        public float RevivalHealthRatio => revivalHealthRatio;
        public float ArmorReductionPerPoint => armorReductionPerPoint;
        public float MaxArmorReduction => maxArmorReduction;
        public PlayerStats BaseStats => baseStats;
        public WeaponData StartingWeapon => startingWeapon;
        public Game.Player.Player PlayerPrefab => playerPrefab;
        public WeaponData[] WeaponPool => weaponPool;
        public PassiveItemData[] PassivePool => passivePool;
    }
}
