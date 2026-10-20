using System;
using Game.Data;

namespace Game.Shop
{
    public class ShopSession
    {
        private readonly IGoldWallet wallet;

        public ShopSession(IGoldWallet wallet, ShopLedger ledger)
        {
            this.wallet = wallet;
            Ledger = ledger;
        }

        public event Action<ShopItemData, int> OnPurchased;

        public ShopLedger Ledger { get; }
        public IGoldWallet Wallet => wallet;

        public int GetPrice(ShopItemData item) => item.BasePrice + item.PricePerPurchase * Ledger.GetCount(item);

        public bool IsSoldOut(ShopItemData item) => item.MaxPurchases > 0 && Ledger.GetCount(item) >= item.MaxPurchases;

        public bool CanAfford(ShopItemData item) => wallet.Gold >= GetPrice(item);

        public bool CanBuy(ShopItemData item) => !IsSoldOut(item) && CanAfford(item);

        public bool TryBuy(ShopItemData item)
        {
            if (!CanBuy(item))
                return false;

            int price = GetPrice(item);
            wallet.Spend(price);
            Ledger.Record(item);
            OnPurchased?.Invoke(item, price);
            return true;
        }
    }
}
