using System;

namespace Game.Shop
{
    public class AccountProgress : IGoldWallet
    {
        public AccountProgress(int gold, ShopLedger ledger)
        {
            Gold = gold;
            Ledger = ledger;
        }

        public event Action<int> OnGoldChanged;

        public int Gold { get; private set; }
        public ShopLedger Ledger { get; }

        public void Spend(int amount)
        {
            Gold -= amount;
            OnGoldChanged?.Invoke(Gold);
        }
    }
}
