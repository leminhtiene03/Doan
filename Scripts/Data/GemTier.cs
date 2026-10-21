using System;
using UnityEngine;

namespace Game.Data
{
    [Serializable]
    public struct GemTier
    {
        [SerializeField] private int minValue;
        [SerializeField] private Color color;
        [SerializeField] private float scale;

        public int MinValue => minValue;
        public Color Color => color;
        public float Scale => scale;
    }
}
