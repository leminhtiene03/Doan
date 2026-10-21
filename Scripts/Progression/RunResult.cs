using System;
using UnityEngine;

namespace Game.Progression
{
    [Serializable]
    public class RunResult
    {
        [SerializeField] private int score;
        [SerializeField] private int accountGold;
        [SerializeField] private float runTime;
        [SerializeField] private int kills;
        [SerializeField] private int level;
        [SerializeField] private bool victory;
        [SerializeField] private string difficultyId;
        [SerializeField] private string characterId;
        [SerializeField] private string timestamp;

        public RunResult(int score, int accountGold, float runTime, int kills, int level, bool victory, string difficultyId, string characterId)
        {
            this.score = score;
            this.accountGold = accountGold;
            this.runTime = runTime;
            this.kills = kills;
            this.level = level;
            this.victory = victory;
            this.difficultyId = difficultyId;
            this.characterId = characterId;
            timestamp = DateTime.UtcNow.ToString("o");
        }

        public int Score => score;
        public int AccountGold => accountGold;
        public float RunTime => runTime;
        public int Kills => kills;
        public int Level => level;
        public bool Victory => victory;
        public string DifficultyId => difficultyId;
        public string CharacterId => characterId;
        public string Timestamp => timestamp;
    }
}
