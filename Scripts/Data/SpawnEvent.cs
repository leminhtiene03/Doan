using System;
using UnityEngine;

namespace Game.Data
{
    [Serializable]
    public class SpawnEvent
    {
        [SerializeField, Min(0f)] private float time;
        [SerializeField] private SpawnPattern pattern;
        [SerializeField] private EnemyData enemy;
        [SerializeField, Min(0)] private int count = 20;
        [SerializeField, Min(0f)] private float duration;
        [SerializeField, Min(0f)] private float spread = 1f;

        public float Time => time;
        public SpawnPattern Pattern => pattern;
        public EnemyData Enemy => enemy;
        public int Count => count;
        public float Duration => duration;
        public float Spread => spread;
    }
}
