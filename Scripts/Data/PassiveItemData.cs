using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Passive Item Data", fileName = "PassiveItemData")]
    public class PassiveItemData : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private StatType stat;
        [SerializeField] private StatModifierKind modifierKind;
        [SerializeField] private float valuePerLevel;
        [SerializeField] private int maxLevel = 5;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public StatType Stat => stat;
        public StatModifierKind ModifierKind => modifierKind;
        public float ValuePerLevel => valuePerLevel;
        public int MaxLevel => maxLevel;
    }
}
