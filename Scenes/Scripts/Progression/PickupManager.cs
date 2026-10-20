using System;
using System.Collections.Generic;
using Game.Core;
using Game.Data;
using Game.Enemies;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Progression
{
    using Game.Player;

    public class PickupManager : MonoBehaviour
    {
        [SerializeField] private ProgressionConfig config;
        private Player player;
        [SerializeField] private PlayerProgression progression;
        [SerializeField] private Pickup gemPrefab;
        [SerializeField] private Pickup coinPrefab;
        [SerializeField] private Pickup chestPrefab;

        private readonly List<Pickup> active = new(400);
        private int gemCount;

        public event Action<PickupKind> OnCollected;

        private void Awake() => player = GameManager.Instance.Player;

        private void Start()
        {
            EnemyManager.Instance.OnEnemyDied += HandleEnemyDied;
            player.OnStatsChanged += WarnIfPlayerOutrunsPickups;
        }

        private void OnDestroy()
        {
            if (EnemyManager.Instance != null)
                EnemyManager.Instance.OnEnemyDied -= HandleEnemyDied;
            if (player != null)
                player.OnStatsChanged -= WarnIfPlayerOutrunsPickups;
        }

        private void Update()
        {
            if (!GameManager.Instance.IsPlaying)
                return;

            Vector2 playerPosition = player.transform.position;
            float magnetSqr = player.Stats.Magnet * player.Stats.Magnet;
            float collectSqr = config.CollectRadius * config.CollectRadius;
            float step = config.AttractSpeed * Time.deltaTime;

            for (int i = active.Count - 1; i >= 0; i--)
                TickPickup(active[i], playerPosition, magnetSqr, collectSqr, step);
        }

        private void TickPickup(Pickup pickup, Vector2 playerPosition, float magnetSqr, float collectSqr, float step)
        {
            Vector2 position = pickup.transform.position;
            float sqrDistance = (playerPosition - position).sqrMagnitude;
            if (!pickup.IsAttracted && sqrDistance <= magnetSqr)
                pickup.IsAttracted = true;
            if (!pickup.IsAttracted)
                return;

            if (sqrDistance <= collectSqr)
                Collect(pickup);
            else
                pickup.transform.position = Vector2.MoveTowards(position, playerPosition, step);
        }

        private void HandleEnemyDied(Enemy enemy)
        {
            Vector2 position = enemy.transform.position;
            EnemyData data = enemy.Data;
            if (data.XpDrop > 0)
                SpawnGem(position, data.XpDrop);
            if (data.CoinAmount > 0 && Random.value < data.CoinDropChance)
                SpawnPickup(coinPrefab, position, data.CoinAmount);
            if (data.DropsChest && chestPrefab != null)
                SpawnPickup(chestPrefab, position, 0);
        }

        private void SpawnGem(Vector2 position, int value)
        {
            if (gemCount >= config.MaxGemsOnMap && TryMergeIntoNearestGem(position, value))
                return;

            Pickup gem = SpawnPickup(gemPrefab, position, value);
            gemCount++;
            ApplyGemAppearance(gem);
        }

        private bool TryMergeIntoNearestGem(Vector2 position, int value)
        {
            Pickup nearest = null;
            float bestSqr = float.MaxValue;
            for (int i = 0; i < active.Count; i++)
            {
                if (active[i].Kind != PickupKind.Gem)
                    continue;
                float sqrDistance = ((Vector2)active[i].transform.position - position).sqrMagnitude;
                if (sqrDistance < bestSqr)
                {
                    bestSqr = sqrDistance;
                    nearest = active[i];
                }
            }

            if (nearest == null)
                return false;
            nearest.AddValue(value);
            ApplyGemAppearance(nearest);
            return true;
        }

        private Pickup SpawnPickup(Pickup prefab, Vector2 position, int value)
        {
            Pickup pickup = PoolManager.Instance.Get(prefab, position, Quaternion.identity);
            pickup.Init(value);
            pickup.ListIndex = active.Count;
            active.Add(pickup);
            return pickup;
        }

        private void Collect(Pickup pickup)
        {
            switch (pickup.Kind)
            {
                case PickupKind.Gem:
                    gemCount--;
                    progression.AddXP(pickup.Value * player.Stats.Growth);
                    break;
                case PickupKind.Coin:
                    progression.AddGold(Mathf.RoundToInt(pickup.Value * player.Stats.Greed));
                    break;
                case PickupKind.Chest:
                    progression.AddChest();
                    break;
            }
            OnCollected?.Invoke(pickup.Kind);

            Pickup last = active[^1];
            active[pickup.ListIndex] = last;
            last.ListIndex = pickup.ListIndex;
            active.RemoveAt(active.Count - 1);
            PoolManager.Instance.Release(pickup);
        }

        private void ApplyGemAppearance(Pickup gem)
        {
            if (config.TryGetGemTier(gem.Value, out GemTier tier))
                gem.SetAppearance(tier.Color, tier.Scale);
        }

        private void WarnIfPlayerOutrunsPickups()
        {
            if (player.CurrentMoveSpeed >= config.AttractSpeed)
                Debug.LogWarning($"Player speed {player.CurrentMoveSpeed} reached pickup attract speed {config.AttractSpeed}.");
        }
    }
}
