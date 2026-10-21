using System.Collections.Generic;
using Game.Core;
using Game.Data;
using Game.Enemies;
using UnityEngine;

namespace Game.Weapons
{
    public class LightningStrike : Weapon
    {
        [SerializeField] private float effectScalePerRadius = 2f;

        private readonly List<Enemy> visibleEnemies = new(128);
        private readonly List<Enemy> chainCandidates = new(32);
        private readonly HashSet<Enemy> struck = new();

        protected override void Attack(Vector2 direction)
        {
            EnemyManager.Instance.CollectVisible(visibleEnemies);
            WeaponLevelStats level = CurrentLevel;
            int strikes = Mathf.Min(visibleEnemies.Count, Mathf.Max(1, level.Projectiles + Owner.Stats.Multishot));
            float radius = level.ImpactRadius * Owner.Stats.Area;
            struck.Clear();

            for (int i = 0; i < strikes; i++)
            {
                int pick = Random.Range(i, visibleEnemies.Count);
                (visibleEnemies[i], visibleEnemies[pick]) = (visibleEnemies[pick], visibleEnemies[i]);
                Enemy target = visibleEnemies[i];
                Strike(target, radius);
                for (int chain = 0; chain < level.ChainCount; chain++)
                {
                    target = FindChainTarget(target.transform.position, level.Range);
                    if (target == null)
                        break;
                    Strike(target, radius);
                }
            }
        }

        private void Strike(Enemy target, float radius)
        {
            struck.Add(target);
            Vector2 position = target.transform.position;
            AreaDamage.Apply(position, radius, 1 << GameLayers.EnemyHurtbox, CreateDamage(Vector2.zero), default);
            ShowStrike(position, radius);
        }

        private Enemy FindChainTarget(Vector2 from, float chainRange)
        {
            EnemyManager.Instance.CollectInRange(from, chainRange, chainCandidates);
            Enemy best = null;
            float bestSqr = float.MaxValue;
            for (int i = 0; i < chainCandidates.Count; i++)
            {
                Enemy candidate = chainCandidates[i];
                if (struck.Contains(candidate))
                    continue;
                float sqr = ((Vector2)candidate.transform.position - from).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = candidate;
                }
            }
            return best;
        }

        private void ShowStrike(Vector2 position, float radius)
        {
            if (Data.EffectPrefab == null)
                return;

            PooledEffect effect = PoolManager.Instance.Get(Data.EffectPrefab, position, Quaternion.identity);
            effect.transform.localScale = Vector3.one * (radius * effectScalePerRadius);
        }
    }
}
