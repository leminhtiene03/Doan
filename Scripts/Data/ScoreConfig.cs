using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Score Config", fileName = "ScoreConfig")]
    public class ScoreConfig : ScriptableObject
    {
        [SerializeField] private float pointsPerSecond = 10f;
        [SerializeField] private float pointsPerKillXP = 5f;
        [SerializeField] private int victoryBonus = 5000;
        [SerializeField, Min(1)] private int scorePerAccountGold = 100;

        public float PointsPerSecond => pointsPerSecond;
        public float PointsPerKillXP => pointsPerKillXP;
        public int VictoryBonus => victoryBonus;

        public int ToAccountGold(int score) => score / scorePerAccountGold;
    }
}
