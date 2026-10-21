using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Progression Config", fileName = "ProgressionConfig")]
    public class ProgressionConfig : ScriptableObject
    {
        [Header("Experience")]
        [SerializeField] private float baseRequiredXP = 5f;
        [SerializeField] private float requiredXPPerLevel = 10f;

        [Header("Level up")]
        [SerializeField] private int choiceCount = 3;
        [SerializeField] private int maxStatCardsPerRoll = 1;
        [SerializeField] private int maxWeapons = 4;
        [SerializeField] private int maxPassives = 4;
        [SerializeField] private StatCardData[] statCards = new StatCardData[0];

        [Header("Fallback rewards")]
        [SerializeField] private float fallbackHealAmount = 30f;
        [SerializeField] private string fallbackHealTitle = "Hồi máu";
        [SerializeField] private string fallbackHealDescription = "Hồi 30 HP";
        [SerializeField] private Sprite fallbackHealIcon;
        [SerializeField] private int fallbackGoldAmount = 20;
        [SerializeField] private string fallbackGoldTitle = "Túi vàng";
        [SerializeField] private string fallbackGoldDescription = "+20 vàng";
        [SerializeField] private Sprite fallbackGoldIcon;

        [Header("Pickups")]
        [SerializeField] private float attractSpeed = 12f;
        [SerializeField] private float collectRadius = 0.3f;
        [SerializeField] private int maxGemsOnMap = 300;
        [SerializeField] private GemTier[] gemTiers = new GemTier[0];

        public int ChoiceCount => choiceCount;
        public int MaxStatCardsPerRoll => maxStatCardsPerRoll;
        public int MaxWeapons => maxWeapons;
        public int MaxPassives => maxPassives;
        public StatCardData[] StatCards => statCards;
        public float FallbackHealAmount => fallbackHealAmount;
        public string FallbackHealTitle => fallbackHealTitle;
        public string FallbackHealDescription => fallbackHealDescription;
        public Sprite FallbackHealIcon => fallbackHealIcon;
        public int FallbackGoldAmount => fallbackGoldAmount;
        public string FallbackGoldTitle => fallbackGoldTitle;
        public string FallbackGoldDescription => fallbackGoldDescription;
        public Sprite FallbackGoldIcon => fallbackGoldIcon;
        public float AttractSpeed => attractSpeed;
        public float CollectRadius => collectRadius;
        public int MaxGemsOnMap => maxGemsOnMap;

        public float GetRequiredXP(int level) => baseRequiredXP + level * requiredXPPerLevel;

        public bool TryGetGemTier(int value, out GemTier tier)
        {
            tier = default;
            bool found = false;
            for (int i = 0; i < gemTiers.Length; i++)
            {
                if (value < gemTiers[i].MinValue)
                    continue;
                if (found && gemTiers[i].MinValue < tier.MinValue)
                    continue;
                tier = gemTiers[i];
                found = true;
            }
            return found;
        }
    }
}
