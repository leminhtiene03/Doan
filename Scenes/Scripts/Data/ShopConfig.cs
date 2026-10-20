using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Shop Config", fileName = "ShopConfig")]
    public class ShopConfig : ScriptableObject
    {
        [SerializeField] private ShopItemData[] items = new ShopItemData[0];

        public ShopItemData[] Items => items;
    }
}
