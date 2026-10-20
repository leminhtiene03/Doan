using Game.Data;
using Game.Weapons;
using UnityEngine;

namespace Game.Progression
{
    using Game.Player;

    public class UpgradeOption
    {
        private UpgradeOption(UpgradeKind kind, string title, string description, Sprite icon)
        {
            Kind = kind;
            Title = title;
            Description = description;
            Icon = icon;
        }

        public UpgradeKind Kind { get; }
        public string Title { get; }
        public string Description { get; }
        public Sprite Icon { get; }
        public WeaponData WeaponData { get; private set; }
        public Weapon Weapon { get; private set; }
        public PassiveItemData PassiveData { get; private set; }
        public PassiveItem Passive { get; private set; }
        public StatCardData StatCard { get; private set; }
        public FallbackReward Fallback { get; private set; }

        public static UpgradeOption ForNewWeapon(WeaponData data)
            => new(UpgradeKind.NewWeapon, data.DisplayName, data.Description, data.DisplayIcon) { WeaponData = data };

        public static UpgradeOption ForWeaponLevel(Weapon weapon)
        {
            WeaponData data = weapon.Data;
            int nextLevel = weapon.Level + 1;
            string description = data.GetLevel(nextLevel).UpgradeDescription;
            return new(UpgradeKind.WeaponLevel, $"{data.DisplayName} Lv.{nextLevel}", description, data.DisplayIcon) { Weapon = weapon };
        }

        public static UpgradeOption ForEvolution(Weapon weapon)
        {
            WeaponData evolution = weapon.Data.Evolution;
            return new(UpgradeKind.Evolution, $"EVOLVE: {evolution.DisplayName}", evolution.Description, evolution.DisplayIcon)
            {
                Weapon = weapon
            };
        }

        public static UpgradeOption ForNewPassive(PassiveItemData data)
            => new(UpgradeKind.NewPassive, data.DisplayName, data.Description, data.Icon) { PassiveData = data };

        public static UpgradeOption ForPassiveLevel(PassiveItem passive)
        {
            PassiveItemData data = passive.Data;
            string title = $"{data.DisplayName} Lv.{passive.Level + 1}";
            return new(UpgradeKind.PassiveLevel, title, data.Description, data.Icon) { Passive = passive };
        }

        public static UpgradeOption ForStatCard(StatCardData card)
            => new(UpgradeKind.StatCard, card.DisplayName, card.Description, card.Icon) { StatCard = card };

        public static UpgradeOption ForFallback(FallbackReward reward, string title, string description, Sprite icon)
            => new(UpgradeKind.Fallback, title, description, icon) { Fallback = reward };
    }
}
