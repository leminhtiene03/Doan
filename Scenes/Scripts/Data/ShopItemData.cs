using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Shop Item Data", fileName = "ShopItemData")]
    public class ShopItemData : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private ShopItemKind kind;

        [Header("Stat")]
        [SerializeField] private StatType stat;
        [SerializeField] private StatModifierKind modifierKind;
        [SerializeField] private float value;

        [Header("Passive / Unlock weapon")]
        [SerializeField] private PassiveItemData passive;
        [SerializeField] private WeaponData weapon;

        [Header("Price")]
        [SerializeField, Min(0)] private int basePrice;
        [SerializeField, Min(0)] private int pricePerPurchase;
        [SerializeField, Min(0)] private int maxPurchases = 1;

        public string Id => id;
        public ShopItemKind Kind => kind;
        public StatType Stat => stat;
        public StatModifierKind ModifierKind => modifierKind;
        public float Value => value;
        public PassiveItemData Passive => passive;
        public WeaponData Weapon => weapon;
        public int BasePrice => basePrice;
        public int PricePerPurchase => pricePerPurchase;
        public int MaxPurchases => maxPurchases;

        public string DisplayName => !string.IsNullOrEmpty(displayName) ? displayName : ReferencedName;
        public string Description => !string.IsNullOrEmpty(description) ? description : ReferencedDescription;
        public Sprite Icon => icon != null ? icon : ReferencedIcon;
        public string Details => ReferencedDescription;

        private string ReferencedName => kind switch
        {
            ShopItemKind.Passive when passive != null => passive.DisplayName,
            ShopItemKind.UnlockWeapon when weapon != null => weapon.DisplayName,
            _ => id
        };

        private string ReferencedDescription => kind switch
        {
            ShopItemKind.Passive when passive != null => passive.Description,
            ShopItemKind.UnlockWeapon when weapon != null => weapon.Description,
            _ => string.Empty
        };

        private Sprite ReferencedIcon => kind switch
        {
            ShopItemKind.Passive when passive != null => passive.Icon,
            ShopItemKind.UnlockWeapon when weapon != null => weapon.DisplayIcon,
            _ => null
        };
    }
}
