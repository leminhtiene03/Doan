using System;
using Game.Core;
using Game.Data;
using Game.Enemies;
using UnityEngine;

namespace Game.Progression
{
    using Game.Player;

    public class RunScoreTracker : MonoBehaviour
    {
        [SerializeField] private ScoreConfig config;
        [SerializeField] private GameManager gameManager;
        private Player player;
        [SerializeField] private PlayerProgression progression;

        public event Action<RunResult> OnRunFinished;

        public int Kills { get; private set; }
        public int KilledXP { get; private set; }
        public RunResult LastResult { get; private set; }

        private void Awake() => player = GameManager.Instance.Player;

        private void OnEnable() => gameManager.OnStateChanged += HandleStateChanged;

        private void OnDisable() => gameManager.OnStateChanged -= HandleStateChanged;

        private void Start() => EnemyManager.Instance.OnEnemyDied += HandleEnemyDied;

        private void OnDestroy()
        {
            if (EnemyManager.Instance != null)
                EnemyManager.Instance.OnEnemyDied -= HandleEnemyDied;
        }

        public int CalculateScore(float runTime, bool victory)
        {
            float score = runTime * config.PointsPerSecond + KilledXP * config.PointsPerKillXP;
            if (victory)
                score += config.VictoryBonus;
            return Mathf.RoundToInt(score * gameManager.Difficulty.ScoreMultiplier);
        }

        private void HandleEnemyDied(Enemy enemy)
        {
            Kills++;
            KilledXP += enemy.Data.XpDrop;
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
                FinishRun(false);
        }

        private void FinishRun(bool victory)
        {
            float runTime = gameManager.RunTime;
            int score = CalculateScore(runTime, victory);
            LastResult = new RunResult(
                score, config.ToAccountGold(score) + progression.Gold, runTime, Kills, progression.Level,
                victory, gameManager.Difficulty.Id, player.CharacterId);
            OnRunFinished?.Invoke(LastResult);
        }
    }
}
