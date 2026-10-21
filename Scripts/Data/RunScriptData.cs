using System;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Run Script Data", fileName = "RunScriptData")]
    public class RunScriptData : ScriptableObject
    {
        [SerializeField, Min(1f)] private float runDuration = 1800f;
        [SerializeField] private WaveData[] waves;
        [SerializeField] private SpawnEvent[] events;

        public float RunDuration => runDuration;
        public int WaveCount => waves.Length;
        public int EventCount => events.Length;

        public SpawnEvent GetEvent(int index) => events[index];

        public int FindWaveIndex(float runTime)
        {
            for (int i = waves.Length - 1; i >= 0; i--)
            {
                if (waves[i].StartTime <= runTime)
                    return i;
            }
            return -1;
        }

        public WaveData GetWave(int index) => waves[index];

        public int FindFirstEventIndexAtOrAfter(float runTime)
        {
            for (int i = 0; i < events.Length; i++)
            {
                if (events[i].Time >= runTime)
                    return i;
            }
            return events.Length;
        }

        private void OnValidate()
        {
            if (waves != null)
                Array.Sort(waves, (a, b) => a.StartTime.CompareTo(b.StartTime));
            if (events != null)
                Array.Sort(events, (a, b) => a.Time.CompareTo(b.Time));
        }
    }
}
