using System;
using UnityEngine;

namespace Game.Data
{
    [Serializable]
    public class WeightedEnemy
    {
        [SerializeField] private EnemyData enemy;
        [SerializeField, Min(0f)] private float weight = 1f;

        public EnemyData Enemy => enemy;
        public float Weight => weight;
    }
}
