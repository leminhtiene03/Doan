using Game.Core;
using Game.Data;
using Game.Enemies;
using UnityEngine;

namespace Game.Spawning
{
    using Game.Player;

    public class SpawnManager : MonoBehaviour
    {
        [SerializeField] private RunScriptData runScript;
        private Player player;

        private int nextEventIndex;
        private float spawnTimer;
        private float pauseEndTime;

        public int CurrentWaveIndex { get; private set; } = -1;
        public int CurrentMaxAlive { get; private set; }
        public bool IsPaused => GameManager.Instance.RunTime < pauseEndTime;

        private void Awake() => player = GameManager.Instance.Player;

        private void Start() => GameManager.Instance.OnRunTimeSkipped += HandleRunTimeSkipped;

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnRunTimeSkipped -= HandleRunTimeSkipped;
        }

        private void Update()
        {
            GameManager game = GameManager.Instance;
            if (!game.IsPlaying)
                return;

            float runTime = game.RunTime;
            FireDueEvents(runTime);
            UpdateWave(runTime);
            if (CurrentWaveIndex < 0 || IsPaused)
                return;

            SpawnRegular(runScript.GetWave(CurrentWaveIndex), game.Difficulty, Time.deltaTime);
        }

        private void UpdateWave(float runTime)
        {
            int waveIndex = runScript.FindWaveIndex(runTime);
            if (waveIndex == CurrentWaveIndex)
                return;

            CurrentWaveIndex = waveIndex;
            spawnTimer = 0f;
        }

        private void SpawnRegular(WaveData wave, DifficultyData difficulty, float deltaTime)
        {
            EnemyManager enemies = EnemyManager.Instance;
            CurrentMaxAlive = Mathf.RoundToInt(wave.MaxAlive * difficulty.MaxAliveMultiplier);
            int minAlive = Mathf.Min(CurrentMaxAlive, Mathf.RoundToInt(wave.MinAlive * difficulty.MaxAliveMultiplier));

            if (enemies.AliveCount < minAlive)
                SpawnFromWave(wave, minAlive - enemies.AliveCount);

            spawnTimer -= deltaTime;
            if (spawnTimer > 0f)
                return;

            spawnTimer = wave.SpawnInterval / difficulty.SpawnRateMultiplier;
            SpawnFromWave(wave, Mathf.Min(wave.SpawnPerTick, CurrentMaxAlive - enemies.AliveCount));
        }

        private void SpawnFromWave(WaveData wave, int count)
        {
            EnemyManager enemies = EnemyManager.Instance;
            for (int i = 0; i < count; i++)
            {
                EnemyData data = wave.PickEnemy();
                if (data != null)
                    enemies.Spawn(data, enemies.GetRandomOffscreenPosition());
            }
        }

        private void FireDueEvents(float runTime)
        {
            while (nextEventIndex < runScript.EventCount && runScript.GetEvent(nextEventIndex).Time <= runTime)
            {
                FireEvent(runScript.GetEvent(nextEventIndex), runTime);
                nextEventIndex++;
            }
        }

        private void FireEvent(SpawnEvent spawnEvent, float runTime)
        {
            switch (spawnEvent.Pattern)
            {
                case SpawnPattern.Ring:
                    SpawnPatterns.Ring(spawnEvent, player.transform.position);
                    break;
                case SpawnPattern.Line:
                    SpawnPatterns.Line(spawnEvent);
                    break;
                case SpawnPattern.Swarm:
                    SpawnPatterns.Swarm(spawnEvent, player.transform.position);
                    break;
                case SpawnPattern.Pause:
                    pauseEndTime = runTime + spawnEvent.Duration;
                    break;
            }
        }

        private void HandleRunTimeSkipped(float runTime)
        {
            nextEventIndex = runScript.FindFirstEventIndexAtOrAfter(runTime);
            pauseEndTime = 0f;
            CurrentWaveIndex = -1;
        }
    }
}
