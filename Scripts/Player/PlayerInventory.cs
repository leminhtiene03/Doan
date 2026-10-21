using System.Collections.Generic;
using Game.Data;
using Game.Weapons;
using UnityEngine;

namespace Game.Player
{
    public class PlayerInventory
    {
        private readonly Player owner;
        private readonly List<Weapon> weapons = new();
        private readonly List<PassiveItem> passives = new();

        public PlayerInventory(Player owner) => this.owner = owner;

        public IReadOnlyList<Weapon> Weapons => weapons;
        public IReadOnlyList<PassiveItem> Passives => passives;

        public void AddWeapon(WeaponData weaponData)
        {
            Weapon weapon = Object.Instantiate(weaponData.WeaponPrefab, owner.transform);
            weapon.Equip(owner, weaponData);
            weapons.Add(weapon);
        }

        public void EvolveWeapon(Weapon weapon)
        {
            int index = weapons.IndexOf(weapon);
            WeaponData evolution = weapon.Data.Evolution;
            Weapon evolved = Object.Instantiate(evolution.WeaponPrefab, owner.transform);
            evolved.Equip(owner, evolution, weapon.SourceData);
            weapon.Unequip();
            Object.Destroy(weapon.gameObject);
            weapons[index] = evolved;
        }

        public void AddPassive(PassiveItemData passiveData)
        {
            passives.Add(new PassiveItem(passiveData));
            ApplyPassiveLevel(passiveData);
        }

        public void LevelUpPassive(PassiveItem passive)
        {
            passive.LevelUp();
            ApplyPassiveLevel(passive.Data);
        }

        public bool HasWeapon(WeaponData weaponData)
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                if (weapons[i].SourceData == weaponData)
                    return true;
            }
            return false;
        }

        public bool HasPassive(PassiveItemData passiveData)
        {
            for (int i = 0; i < passives.Count; i++)
            {
                if (passives[i].Data == passiveData)
                    return true;
            }
            return false;
        }

        public void TickWeapons(float deltaTime)
        {
            for (int i = 0; i < weapons.Count; i++)
                weapons[i].Tick(deltaTime);
        }

        private void ApplyPassiveLevel(PassiveItemData passiveData)
            => owner.ApplyStatModifier(passiveData.Stat, passiveData.ValuePerLevel, passiveData.ModifierKind);
    }
}
