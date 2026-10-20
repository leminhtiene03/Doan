using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Sound Data", fileName = "Sfx_")]
    public class SoundData : ScriptableObject
    {
        [SerializeField] private AudioClip[] clips = new AudioClip[0];
        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [SerializeField, Range(0f, 0.5f)] private float pitchVariance = 0.05f;
        [SerializeField, Min(0f)] private float minInterval;

        public float Volume => volume;
        public float PitchVariance => pitchVariance;
        public float MinInterval => minInterval;
        public bool HasClips => clips.Length > 0;

        public AudioClip PickClip() => clips.Length == 0 ? null : clips[Random.Range(0, clips.Length)];
    }
}
