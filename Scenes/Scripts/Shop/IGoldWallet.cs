using System;

namespace Game.Shop
{
    public interface IGoldWallet
    {
        event Action<int> OnGoldChanged;
        int Gold { get; }
        void Spend(int amount);
    }
}
