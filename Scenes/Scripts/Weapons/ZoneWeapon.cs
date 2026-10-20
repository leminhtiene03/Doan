using System.Collections.Generic;
using Game.Core;
using Game.Data;
using Game.Enemies;
using UnityEngine;

namespace Game.Weapons
{
    public class ZoneWeapon : Weapon
    {
        [SerializeField] private DamageZone zonePrefab;
        [SerializeField] private ZonePlacement placement;
        [SerializeField] private float ownerScatterRadius = 0.6f;

        private readonly List<DamageZone> zones = new();

        public override Sprite PreviewSprite => zonePrefab != null ? zonePrefab.Sprite : null;
        private readonly List<Enemy> candidates = new(64);

        protected override void Attack(Vector2 direction)
        {
            WeaponLevelStats level = CurrentLevel;
            int count = Mathf.Max(1, level.Projectiles + Owner.Stats.Multishot);
            if (placement == ZonePlacement.RandomEnemy)
            {
                EnemyManager.Instance.CollectInRange(Owner.transform.position, level.Range, candidates);
                count = Mathf.Min(count, candidates.Count);
            }

            for (int i = 0; i < count; i++)
                SpawnZone(GetZonePosition(i, count), level);
        }

        public override void Unequip()
        {
            for (int i = 0; i < zones.Count; i++)
                PoolManager.Instance.Release(zones[i]);
            zones.Clear();
        }

        protected override void TickActive(float deltaTime)
        {
            for (int i = zones.Count - 1; i >= 0; i--)
            {
                DamageZone zone = zones[i];
                zone.Tick(deltaTime);
                if (!zone.IsFinished)
                    continue;

                zones[i] = zones[^1];
                zones.RemoveAt(zones.Count - 1);
                PoolManager.Instance.Release(zone);
            }
        }

        private Vector2 GetZonePosition(int index, int count)
        {
            if (placement == ZonePlacement.RandomEnemy)
            {
                int pick = Random.Range(index, candidates.Count);
                (candidates[index], candidates[pick]) = (candidates[pick], candidates[index]);
                return candidates[index].transform.position;
            }

            Vector2 origin = Owner.transform.position;
            if (count == 1)
                return origin;
            return origin + EnemyManager.DirectionFromAngle(index * 360f / count) * ownerScatterRadius;
        }

        private void SpawnZone(Vector2 position, WeaponLevelStats level)
        {
            DamageZone zone = PoolManager.Instance.Get(zonePrefab, position, Quaternion.identity);
            zone.Activate(
                position,
                level.ImpactRadius * Owner.Stats.Area,
                level.Duration * Owner.Stats.Duration,
                level.PullStrength,
                CreateDamage(Vector2.zero),
                new HitEffect(level.SlowAmount, level.SlowDuration * Owner.Stats.Duration));
            zones.Add(zone);
        }
    }
}
