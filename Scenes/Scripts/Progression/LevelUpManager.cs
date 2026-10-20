using System;
using System.Collections.Generic;
using Game.Core;
using Game.Data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Progression
{
    using Game.Player;

    public class LevelUpManager : MonoBehaviour
    {
        [SerializeField] private ProgressionConfig config;
        private Player player;
        [SerializeField] private PlayerProgression progression;

        private readonly List<UpgradeOption> candidates = new();
        private readonly List<UpgradeOption> evolutions = new();
        private readonly List<UpgradeOption> options = new();
        private readonly Dictionary<StatCardData, int> statCardPicks = new();
        private bool isChoosing;

        public event Action<IReadOnlyList<UpgradeOption>> OnOptionsReady;
        public event Action OnClosed;

        public bool IsChestReward { get; private set; }

        private void Awake() => player = GameManager.Instance.Player;

        private void OnEnable()
        {
            progression.OnLevelUpReady += HandleRewardReady;
            progression.OnChestReady += HandleRewardReady;
        }

        private void OnDisable()
        {
            progression.OnLevelUpReady -= HandleRewardReady;
            progression.OnChestReady -= HandleRewardReady;
        }

        public void ChooseUpgrade(int index)
        {
            if (!isChoosing || index < 0 || index >= options.Count)
                return;

            Apply(options[index]);
            if (IsChestReward)
                progression.ConsumePendingChest();
            else
                progression.ConsumePendingLevelUp();
            if (HasPendingReward)
            {
                RollOptions();
                return;
            }

            isChoosing = false;
            OnClosed?.Invoke();
            GameManager.Instance.ResumePlaying();
        }

        private bool HasPendingReward => progression.PendingLevelUps > 0 || progression.PendingChests > 0;

        private void HandleRewardReady()
        {
            if (isChoosing)
                return;

            isChoosing = true;
            GameManager.Instance.EnterLevelUp();
            RollOptions();
        }

        private void RollOptions()
        {
            IsChestReward = progression.PendingLevelUps == 0;
            BuildCandidates();
            Shuffle(candidates);
            options.Clear();
            if (evolutions.Count > 0)
                options.Add(evolutions[Random.Range(0, evolutions.Count)]);

            int statCardCount = 0;
            for (int i = 0; i < candidates.Count && options.Count < config.ChoiceCount; i++)
            {
                UpgradeOption candidate = candidates[i];
                if (candidate.Kind == UpgradeKind.StatCard)
                {
                    if (statCardCount >= config.MaxStatCardsPerRoll)
                        continue;
                    statCardCount++;
                }
                options.Add(candidate);
            }

            AddFallbackOptions();
            OnOptionsReady?.Invoke(options);
        }

        private void BuildCandidates()
        {
            candidates.Clear();
            evolutions.Clear();
            AddEvolutionCandidates(player.Inventory);
            AddWeaponCandidates(player.Inventory);
            AddPassiveCandidates(player.Inventory);
            AddStatCardCandidates();
        }

        private void AddEvolutionCandidates(PlayerInventory inventory)
        {
            for (int i = 0; i < inventory.Weapons.Count; i++)
            {
                if (inventory.Weapons[i].CanEvolve(inventory))
                    evolutions.Add(UpgradeOption.ForEvolution(inventory.Weapons[i]));
            }
        }

        private void AddWeaponCandidates(PlayerInventory inventory)
        {
            for (int i = 0; i < inventory.Weapons.Count; i++)
            {
                if (!inventory.Weapons[i].IsMaxLevel)
                    candidates.Add(UpgradeOption.ForWeaponLevel(inventory.Weapons[i]));
            }

            if (inventory.Weapons.Count >= config.MaxWeapons)
                return;
            foreach (WeaponData weapon in player.Character.WeaponPool)
            {
                if (!inventory.HasWeapon(weapon) && GameManager.Instance.IsWeaponUnlocked(weapon))
                    candidates.Add(UpgradeOption.ForNewWeapon(weapon));
            }
        }

        private void AddPassiveCandidates(PlayerInventory inventory)
        {
            for (int i = 0; i < inventory.Passives.Count; i++)
            {
                if (!inventory.Passives[i].IsMaxLevel)
                    candidates.Add(UpgradeOption.ForPassiveLevel(inventory.Passives[i]));
            }

            if (inventory.Passives.Count >= config.MaxPassives)
                return;
            foreach (PassiveItemData passive in player.Character.PassivePool)
            {
                if (!inventory.HasPassive(passive))
                    candidates.Add(UpgradeOption.ForNewPassive(passive));
            }
        }

        private void AddStatCardCandidates()
        {
            foreach (StatCardData card in config.StatCards)
            {
                statCardPicks.TryGetValue(card, out int picks);
                if (card.MaxPicks == 0 || picks < card.MaxPicks)
                    candidates.Add(UpgradeOption.ForStatCard(card));
            }
        }

        private void AddFallbackOptions()
        {
            if (options.Count < config.ChoiceCount)
            {
                options.Add(UpgradeOption.ForFallback(
                    FallbackReward.Heal, config.FallbackHealTitle, config.FallbackHealDescription, config.FallbackHealIcon));
            }
            if (options.Count < config.ChoiceCount)
            {
                options.Add(UpgradeOption.ForFallback(
                    FallbackReward.Gold, config.FallbackGoldTitle, config.FallbackGoldDescription, config.FallbackGoldIcon));
            }
        }

        private void Apply(UpgradeOption option)
        {
            switch (option.Kind)
            {
                case UpgradeKind.NewWeapon:
                    player.Inventory.AddWeapon(option.WeaponData);
                    break;
                case UpgradeKind.WeaponLevel:
                    option.Weapon.LevelUp();
                    break;
                case UpgradeKind.Evolution:
                    player.Inventory.EvolveWeapon(option.Weapon);
                    break;
                case UpgradeKind.NewPassive:
                    player.Inventory.AddPassive(option.PassiveData);
                    break;
                case UpgradeKind.PassiveLevel:
                    player.Inventory.LevelUpPassive(option.Passive);
                    break;
                case UpgradeKind.StatCard:
                    ApplyStatCard(option.StatCard);
                    break;
                case UpgradeKind.Fallback:
                    ApplyFallback(option.Fallback);
                    break;
            }
        }

        private void ApplyStatCard(StatCardData card)
        {
            player.ApplyStatModifier(card.Stat, card.Value, card.ModifierKind);
            if (card.HealAmount > 0f)
                player.Heal(card.HealAmount);
            statCardPicks.TryGetValue(card, out int picks);
            statCardPicks[card] = picks + 1;
        }

        private void ApplyFallback(FallbackReward reward)
        {
            if (reward == FallbackReward.Heal)
                player.Heal(config.FallbackHealAmount);
            else
                progression.AddGold(config.FallbackGoldAmount);
        }

        private static void Shuffle(List<UpgradeOption> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
