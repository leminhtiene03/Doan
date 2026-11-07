using System.Collections.Generic;
using Game.Core;
using Game.Data;
using Game.Weapons;
using UnityEngine;

namespace Game.UI
{
    using Game.Player;

    public class InventoryHudView : MonoBehaviour
    {
        private Player player;
        [SerializeField] private ProgressionConfig config;
        [SerializeField] private InventorySlotView slotPrefab;
        [SerializeField] private Transform weaponRow;
        [SerializeField] private Transform passiveRow;
        [SerializeField] private InventoryTooltipView tooltip;

        private readonly List<InventorySlotView> weaponSlots = new();
        private readonly List<InventorySlotView> passiveSlots = new();
        private InventorySlotView hoveredSlot;

        private void Awake()
        {
            player = GameManager.Instance.Player;
            CreateSlots(weaponRow, config.MaxWeapons, weaponSlots);
            CreateSlots(passiveRow, config.MaxPassives, passiveSlots);
        }

        private void OnEnable() => player.Inventory.OnChanged += Refresh;

        private void OnDisable() => player.Inventory.OnChanged -= Refresh;

        private void Start() => Refresh();

        private void CreateSlots(Transform row, int count, List<InventorySlotView> slots)
        {
            for (int i = 0; i < count; i++)
            {
                InventorySlotView slot = Instantiate(slotPrefab, row);
                slot.OnHovered += HandleHovered;
                slot.OnUnhovered += HandleUnhovered;
                slots.Add(slot);
            }
        }

        private void Refresh()
        {
            IReadOnlyList<Weapon> weapons = player.Inventory.Weapons;
            for (int i = 0; i < weaponSlots.Count; i++)
            {
                if (i >= weapons.Count)
                {
                    weaponSlots[i].Clear();
                    continue;
                }

                Weapon weapon = weapons[i];
                WeaponData data = weapon.Data;
                bool isEvolved = data != weapon.SourceData;
                weaponSlots[i].Show(data.DisplayIcon, data.DisplayName, data.Description, weapon.Level, weapon.IsMaxLevel, isEvolved);
            }

            IReadOnlyList<PassiveItem> passives = player.Inventory.Passives;
            for (int i = 0; i < passiveSlots.Count; i++)
            {
                if (i >= passives.Count)
                {
                    passiveSlots[i].Clear();
                    continue;
                }

                PassiveItem passive = passives[i];
                PassiveItemData data = passive.Data;
                passiveSlots[i].Show(data.Icon, data.DisplayName, data.Description, passive.Level, passive.IsMaxLevel, false);
            }

            if (hoveredSlot != null)
                HandleHovered(hoveredSlot);
        }

        private void HandleHovered(InventorySlotView slot)
        {
            hoveredSlot = slot;
            if (slot.HasContent)
                tooltip.Show(slot);
            else
                tooltip.Hide();
        }

        private void HandleUnhovered(InventorySlotView slot)
        {
            if (hoveredSlot != slot)
                return;

            hoveredSlot = null;
            tooltip.Hide();
        }
    }
}
