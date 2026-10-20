using System;
using Game.Data;
using Game.Shop;
using UnityEngine;

namespace Game.Progression
{
    public class PlayerProgression : MonoBehaviour, IGoldWallet
    {
        [SerializeField] private ProgressionConfig config;

        public event Action<float, float> OnXPChanged;
        public event Action<int> OnLevelChanged;
        public event Action<int> OnGoldChanged;
        public event Action OnLevelUpReady;
        public event Action OnChestReady;

        public int Level { get; private set; } = 1;
        public float CurrentXP { get; private set; }
        public float RequiredXP { get; private set; }
        public int Gold { get; private set; }
        public int PendingLevelUps { get; private set; }
        public int PendingChests { get; private set; }

        private void Awake() => RequiredXP = config.GetRequiredXP(Level);

        public void AddXP(float amount)
        {
            CurrentXP += amount;
            int gainedLevels = 0;
            while (CurrentXP >= RequiredXP)
            {
                CurrentXP -= RequiredXP;
                Level++;
                RequiredXP = config.GetRequiredXP(Level);
                gainedLevels++;
            }

            OnXPChanged?.Invoke(CurrentXP, RequiredXP);
            if (gainedLevels == 0)
                return;

            PendingLevelUps += gainedLevels;
            OnLevelChanged?.Invoke(Level);
            OnLevelUpReady?.Invoke();
        }

        public void AddGold(int amount)
        {
            Gold += amount;
            OnGoldChanged?.Invoke(Gold);
        }

        public void Spend(int amount) => AddGold(-amount);

        public void ConsumePendingLevelUp()
        {
            if (PendingLevelUps > 0)
                PendingLevelUps--;
        }

        public void AddChest()
        {
            PendingChests++;
            OnChestReady?.Invoke();
        }

        public void ConsumePendingChest()
        {
            if (PendingChests > 0)
                PendingChests--;
        }
    }
}
