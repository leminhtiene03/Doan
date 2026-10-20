using Game.Data;

namespace Game.Shop
{
    using Game.Player;

    public static class ShopItemEffects
    {
        public static void Apply(ShopItemData item, Player player, int times)
        {
            switch (item.Kind)
            {
                case ShopItemKind.Stat:
                    player.ApplyStatModifier(item.Stat, item.Value * times, item.ModifierKind);
                    break;
                case ShopItemKind.Passive:
                    if (!player.Inventory.HasPassive(item.Passive))
                        player.Inventory.AddPassive(item.Passive);
                    break;
            }
        }

        public static void ApplyAll(ShopConfig catalog, ShopLedger ledger, Player player)
        {
            for (int i = 0; i < catalog.Items.Length; i++)
            {
                ShopItemData item = catalog.Items[i];
                int count = ledger.GetCount(item);
                if (count > 0)
                    Apply(item, player, count);
            }
        }
    }
}
