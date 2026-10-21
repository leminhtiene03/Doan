using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Difficulty Data", fileName = "DifficultyData")]
    public class DifficultyData : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private float scoreMultiplier = 1f;
        [SerializeField] private float hpMultiplier = 1f;
        [SerializeField] private float damageMultiplier = 1f;
        [SerializeField] private float speedMultiplier = 1f;
        [SerializeField] private float spawnRateMultiplier = 1f;
        [SerializeField] private float maxAliveMultiplier = 1f;

        public string Id => id;
        public string DisplayName => displayName;
        public float ScoreMultiplier => scoreMultiplier;
        public float HpMultiplier => hpMultiplier;
        public float DamageMultiplier => damageMultiplier;
        public float SpeedMultiplier => speedMultiplier;
        public float SpawnRateMultiplier => spawnRateMultiplier;
        public float MaxAliveMultiplier => maxAliveMultiplier;
    }
}
