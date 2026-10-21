using System;
using UnityEngine;

namespace Game.Data
{
    [Serializable]
    public class WaveData
    {
        [SerializeField, Min(0f)] private float startTime;
        [SerializeField, Min(0.05f)] private float spawnInterval = 1.5f;
        [SerializeField, Min(1)] private int spawnPerTick = 2;
        [SerializeField, Min(0)] private int minAlive = 10;
        [SerializeField, Min(0)] private int maxAlive = 30;
        [SerializeField] private WeightedEnemy[] enemies;

        public float StartTime => startTime;
        public float SpawnInterval => spawnInterval;
        public int SpawnPerTick => spawnPerTick;
        public int MinAlive => minAlive;
        public int MaxAlive => maxAlive;

        public EnemyData PickEnemy()
        {
            float totalWeight = 0f;
            for (int i = 0; i < enemies.Length; i++)
                totalWeight += enemies[i].Weight;
            if (totalWeight <= 0f)
                return null;

            float roll = UnityEngine.Random.Range(0f, totalWeight);
            for (int i = 0; i < enemies.Length; i++)
            {
                roll -= enemies[i].Weight;
                if (roll <= 0f)
                    return enemies[i].Enemy;
            }
            return enemies[^1].Enemy;
        }
    }
}
