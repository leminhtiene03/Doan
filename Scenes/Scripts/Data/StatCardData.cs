using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Stat Card Data", fileName = "StatCardData")]
    public class StatCardData : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private StatType stat;
        [SerializeField] private StatModifierKind modifierKind;
        [SerializeField] private float value;
        [SerializeField] private int maxPicks;
        [SerializeField] private float healAmount;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public StatType Stat => stat;
        public StatModifierKind ModifierKind => modifierKind;
        public float Value => value;
        public int MaxPicks => maxPicks;
        public float HealAmount => healAmount;
    }
}
