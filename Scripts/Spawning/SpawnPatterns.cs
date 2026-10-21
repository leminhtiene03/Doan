using Game.Data;
using Game.Enemies;
using UnityEngine;

namespace Game.Spawning
{
    public static class SpawnPatterns
    {
        public static void Ring(SpawnEvent spawnEvent, Vector2 center)
        {
            EnemyManager enemies = EnemyManager.Instance;
            float radius = enemies.OffscreenRadius;
            float step = 360f / Mathf.Max(1, spawnEvent.Count);
            float startAngle = Random.Range(0f, step);
            for (int i = 0; i < spawnEvent.Count; i++)
            {
                Vector2 direction = EnemyManager.DirectionFromAngle(startAngle + i * step);
                enemies.Spawn(spawnEvent.Enemy, enemies.ClampToMap(center + direction * radius));
            }
        }

        public static void Line(SpawnEvent spawnEvent)
        {
            EnemyManager enemies = EnemyManager.Instance;
            Vector2 center = enemies.CameraCenter;
            bool fromLeft = Random.value < 0.5f;
            if (!enemies.IsInsideMap(new Vector2(GetLineStartX(enemies, fromLeft), center.y)))
                fromLeft = !fromLeft;
            Vector2 direction = fromLeft ? Vector2.right : Vector2.left;
            float startX = GetLineStartX(enemies, fromLeft);
            float halfLength = (spawnEvent.Count - 1) * 0.5f;
            for (int i = 0; i < spawnEvent.Count; i++)
            {
                Vector2 position = new(startX, center.y + (i - halfLength) * spawnEvent.Spread);
                if (enemies.IsInsideMap(position))
                    enemies.SpawnTraveling(spawnEvent.Enemy, position, direction);
            }
        }

        public static void Swarm(SpawnEvent spawnEvent, Vector2 target)
        {
            EnemyManager enemies = EnemyManager.Instance;
            Vector2 edge = enemies.GetScreenEdgeInsideMap();
            Vector2 direction = (target - edge).normalized;
            Vector2 center = edge - direction * spawnEvent.Spread;
            for (int i = 0; i < spawnEvent.Count; i++)
            {
                Vector2 position = center + Random.insideUnitCircle * spawnEvent.Spread;
                enemies.SpawnTraveling(spawnEvent.Enemy, enemies.ClampToMap(position), direction);
            }
        }

        private static float GetLineStartX(EnemyManager enemies, bool fromLeft)
            => enemies.CameraCenter.x + (fromLeft ? -enemies.SpawnHalfExtents.x : enemies.SpawnHalfExtents.x);
    }
}
