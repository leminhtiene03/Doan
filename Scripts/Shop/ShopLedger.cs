using System.Collections.Generic;
using Game.Data;

namespace Game.Shop
{
    public class ShopLedger
    {
        private readonly Dictionary<string, int> purchaseCounts = new();

        public int GetCount(ShopItemData item) => purchaseCounts.TryGetValue(item.Id, out int count) ? count : 0;

        public void SetCount(string itemId, int count) => purchaseCounts[itemId] = count;

        public void Record(ShopItemData item) => purchaseCounts[item.Id] = GetCount(item) + 1;

        public bool IsWeaponLocked(ShopConfig catalog, WeaponData weapon)
        {
            for (int i = 0; i < catalog.Items.Length; i++)
            {
                ShopItemData item = catalog.Items[i];
                if (item.Kind == ShopItemKind.UnlockWeapon && item.Weapon == weapon && GetCount(item) == 0)
                    return true;
            }
            return false;
        }
    }
}
