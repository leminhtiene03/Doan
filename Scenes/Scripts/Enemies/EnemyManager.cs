using System;
using System.Collections.Generic;
using Game.Core;
using Game.Data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Enemies
{
    using Game.Player;

    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance { get; private set; }

        private Player player;
        [SerializeField] private EnemyData[] preloadedEnemies;
        [SerializeField] private int preloadCountPerType = 300;
        [SerializeField] private float spawnMargin = 1f;
        [SerializeField] private float spawnDepth = 12f;
        [SerializeField] private float relocateBuffer = 4f;
        [SerializeField] private float relocateSpreadAngle = 45f;
        [SerializeField] private MapBounds mapBounds;
        [SerializeField] private int spawnAttempts = 12;

        private readonly List<Enemy> active = new(300);
        private Action<Enemy> enemyDeathHandler;
        private Action<Enemy> enemyDamagedHandler;
        private Camera mainCamera;
        private Vector2 spawnHalfExtents;

        public event Action<Enemy> OnEnemyDied;
        public event Action<Enemy> OnEnemyDamaged;

        public IReadOnlyList<Enemy> Active => active;
        public int AliveCount { get; private set; }
        public Vector2 CameraCenter => mainCamera.transform.position;
        public Vector2 SpawnHalfExtents => spawnHalfExtents;
        public float OffscreenRadius => spawnHalfExtents.magnitude;

        private void Awake()
        {
            player = GameManager.Instance.Player;
            Instance = this;
            mainCamera = Camera.main;
            enemyDeathHandler = HandleEnemyDeath;
            enemyDamagedHandler = HandleEnemyDamaged;
            UpdateSpawnBounds();
        }

        private void Start()
        {
            for (int i = 0; i < preloadedEnemies.Length; i++)
                PoolManager.Instance.Prewarm(preloadedEnemies[i].Prefab, preloadCountPerType);
        }

        private void Update()
        {
            if (!GameManager.Instance.IsPlaying)
                return;

            UpdateSpawnBounds();
            Vector2 playerPosition = player.transform.position;
            Collider2D playerCollider = player.BodyCollider;
            float relocateDistance = OffscreenRadius + spawnDepth + relocateBuffer;
            float relocateSqr = relocateDistance * relocateDistance;
            float deltaTime = Time.deltaTime;

            for (int i = active.Count - 1; i >= 0; i--)
                TickEnemy(active[i], playerPosition, playerCollider, relocateSqr, deltaTime);
        }

        public Enemy Spawn(EnemyData data, Vector2 position)
        {
            Enemy enemy = PoolManager.Instance.Get(data.Prefab, position, Quaternion.identity);
            enemy.Init(data, GameManager.Instance.Difficulty);
            enemy.OnDeath -= enemyDeathHandler;
            enemy.OnDeath += enemyDeathHandler;
            enemy.OnDamaged -= enemyDamagedHandler;
            enemy.OnDamaged += enemyDamagedHandler;
            Register(enemy);
            AliveCount++;
            return enemy;
        }

        public Enemy SpawnTraveling(EnemyData data, Vector2 position, Vector2 direction)
        {
            Enemy enemy = Spawn(data, position);
            enemy.SetTravelDirection(direction);
            return enemy;
        }

        public void Despawn(Enemy enemy)
        {
            if (!enemy.IsDead)
                AliveCount--;
            Unregister(enemy);
            PoolManager.Instance.Release(enemy);
        }

        public void DespawnAll()
        {
            for (int i = active.Count - 1; i >= 0; i--)
                Despawn(active[i]);
        }

        public Enemy FindNearest(Vector2 from, float maxRange)
        {
            Enemy nearest = null;
            float bestSqr = maxRange * maxRange;
            for (int i = 0; i < active.Count; i++)
            {
                Enemy enemy = active[i];
                if (!enemy.CanBeHit)
                    continue;
                float sqr = ((Vector2)enemy.transform.position - from).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearest = enemy;
                }
            }
            return nearest;
        }

        public void CollectInRange(Vector2 origin, float range, List<Enemy> results)
        {
            results.Clear();
            float rangeSqr = range * range;
            for (int i = 0; i < active.Count; i++)
            {
                Enemy enemy = active[i];
                if (enemy.CanBeHit && ((Vector2)enemy.transform.position - origin).sqrMagnitude <= rangeSqr)
                    results.Add(enemy);
            }
        }

        public void CollectVisible(List<Enemy> results)
        {
            results.Clear();
            Vector2 center = CameraCenter;
            float halfHeight = mainCamera.orthographicSize;
            float halfWidth = halfHeight * mainCamera.aspect;
            for (int i = 0; i < active.Count; i++)
            {
                Enemy enemy = active[i];
                Vector2 offset = (Vector2)enemy.transform.position - center;
                if (enemy.CanBeHit && Mathf.Abs(offset.x) <= halfWidth && Mathf.Abs(offset.y) <= halfHeight)
                    results.Add(enemy);
            }
        }

        public Vector2 GetRandomOffscreenPosition() => FindSpawnPosition(Random.Range(0f, 360f), 180f);

        public bool IsInsideMap(Vector2 position) => mapBounds == null || mapBounds.IsInside(position);

        public Vector2 ClampToMap(Vector2 position) => mapBounds != null ? mapBounds.ClampInside(position) : position;

        public Vector2 GetScreenEdgeInsideMap()
        {
            Vector2 edge = GetScreenEdgePosition(Random.Range(0f, 360f));
            for (int i = 0; i < spawnAttempts && !IsInsideMap(edge); i++)
                edge = GetScreenEdgePosition(Random.Range(0f, 360f));
            return IsInsideMap(edge) ? edge : ClampToMap(GetScreenEdgePosition(AngleToMapCenter()));
        }

        public Vector2 GetSpawnBandPosition(float degrees)
            => GetScreenEdgePosition(degrees) + DirectionFromAngle(degrees) * Random.Range(0f, spawnDepth);

        public Vector2 GetScreenEdgePosition(float degrees)
        {
            Vector2 direction = DirectionFromAngle(degrees);
            float toVerticalEdge = direction.x != 0f ? spawnHalfExtents.x / Mathf.Abs(direction.x) : float.MaxValue;
            float toHorizontalEdge = direction.y != 0f ? spawnHalfExtents.y / Mathf.Abs(direction.y) : float.MaxValue;
            return CameraCenter + direction * Mathf.Min(toVerticalEdge, toHorizontalEdge);
        }

        private void TickEnemy(
            Enemy enemy, Vector2 playerPosition, Collider2D playerCollider, float relocateSqr, float deltaTime)
        {
            enemy.Tick(playerPosition, playerCollider, deltaTime);
            if (enemy.IsReadyToDespawn)
            {
                Despawn(enemy);
                return;
            }

            if (enemy.ConsumeLandedAttack())
                player.TakeDamage(enemy.CreateAttackDamage());

            Vector2 position = enemy.transform.position;
            if (enemy.IsTraveling && !enemy.IsDead && !IsInsideMap(position))
            {
                Despawn(enemy);
                return;
            }

            if (enemy.IsDead || (position - playerPosition).sqrMagnitude <= relocateSqr)
                return;

            if (enemy.IsTraveling)
                Despawn(enemy);
            else
                enemy.Teleport(GetRelocatePosition());
        }

        private void HandleEnemyDamaged(Enemy enemy) => OnEnemyDamaged?.Invoke(enemy);

        private void HandleEnemyDeath(Enemy enemy)
        {
            AliveCount--;
            OnEnemyDied?.Invoke(enemy);
        }

        private void Register(Enemy enemy)
        {
            enemy.ListIndex = active.Count;
            active.Add(enemy);
        }

        private void Unregister(Enemy enemy)
        {
            Enemy last = active[^1];
            active[enemy.ListIndex] = last;
            last.ListIndex = enemy.ListIndex;
            active.RemoveAt(active.Count - 1);
        }

        private Vector2 GetRelocatePosition()
        {
            Vector2 moveDirection = player.MoveDirection;
            float baseAngle = moveDirection == Vector2.zero
                ? Random.Range(0f, 360f)
                : Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            return FindSpawnPosition(baseAngle, relocateSpreadAngle);
        }

        private Vector2 FindSpawnPosition(float baseAngle, float spreadAngle)
        {
            for (int i = 0; i < spawnAttempts; i++)
            {
                Vector2 position = GetSpawnBandPosition(baseAngle + Random.Range(-spreadAngle, spreadAngle));
                if (IsSpawnable(position))
                    return position;
            }
            return ClampToMap(GetSpawnBandPosition(AngleToMapCenter()));
        }

        private bool IsSpawnable(Vector2 position) => mapBounds == null || mapBounds.IsFree(position);

        private float AngleToMapCenter()
        {
            if (mapBounds == null)
                return Random.Range(0f, 360f);
            Vector2 toCenter = mapBounds.Center - CameraCenter;
            return Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg;
        }

        private void UpdateSpawnBounds()
        {
            float halfHeight = mainCamera.orthographicSize;
            float halfWidth = halfHeight * mainCamera.aspect;
            spawnHalfExtents = new Vector2(halfWidth + spawnMargin, halfHeight + spawnMargin);
        }

        public static Vector2 DirectionFromAngle(float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }
    }
}
