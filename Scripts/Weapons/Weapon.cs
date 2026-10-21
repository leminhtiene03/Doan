using Game.Audio;
using Game.Core;
using Game.Data;
using Game.Enemies;
using UnityEngine;

namespace Game.Weapons
{
    using Game.Player;

    public abstract class Weapon : MonoBehaviour
    {
        private float cooldownTimer;

        public WeaponData Data { get; private set; }
        public WeaponData SourceData { get; private set; }
        public virtual Sprite PreviewSprite => null;
        public int Level { get; private set; }
        public bool IsMaxLevel => Level >= Data.MaxLevel;
        protected Player Owner { get; private set; }
        protected WeaponLevelStats CurrentLevel => Data.GetLevel(Level);

        public bool CanEvolve(PlayerInventory inventory)
            => IsMaxLevel && Data.Evolution != null && Data.EvolutionItem != null && inventory.HasPassive(Data.EvolutionItem);

        public void Equip(Player owner, WeaponData data, WeaponData sourceData = null)
        {
            Owner = owner;
            Data = data;
            SourceData = sourceData != null ? sourceData : data;
            Level = 1;
            cooldownTimer = 0f;
        }

        public void LevelUp()
        {
            if (!IsMaxLevel)
                Level++;
        }

        public void Tick(float deltaTime)
        {
            TickActive(deltaTime);
            cooldownTimer -= deltaTime;
            if (cooldownTimer > 0f || !TryGetAimDirection(out Vector2 direction))
                return;

            float attackInterval = CurrentLevel.AttackInterval / Owner.Stats.AttackSpeed;
            cooldownTimer = attackInterval;
            Attack(direction);
            AudioManager.Play(Data.AttackSound);
            if (Owner.RollSpellEcho(Data.DamageType))
                Attack(direction);
            if (Data.PlayOwnerAttackAnim)
                Owner.PlayAttackAnimation(attackInterval);
        }

        protected DamageInfo CreateDamage(Vector2 knockbackDirection)
        {
            WeaponLevelStats level = CurrentLevel;
            float amount = Owner.ModifyOutgoingDamage(level.Damage * Owner.Stats.Might);
            return new DamageInfo(amount, Data.DamageType, knockbackDirection * level.Knockback, Owner);
        }

        protected bool TryGetAimDirection(out Vector2 direction)
        {
            direction = Owner.AimDirection;
            switch (Data.AimMode)
            {
                case AimMode.NearestEnemy:
                    Vector2 origin = Owner.transform.position;
                    Enemy target = EnemyManager.Instance.FindNearest(origin, CurrentLevel.Range);
                    if (target == null)
                        return false;
                    direction = ((Vector2)target.transform.position - origin).normalized;
                    return true;
                case AimMode.RandomEnemy:
                    return EnemyManager.Instance.AliveCount > 0;
                default:
                    return true;
            }
        }

        public virtual void Unequip() { }

        protected virtual void TickActive(float deltaTime) { }

        protected abstract void Attack(Vector2 direction);
    }
}
