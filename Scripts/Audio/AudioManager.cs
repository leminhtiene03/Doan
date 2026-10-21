using System.Collections.Generic;
using Game.Data;
using UnityEngine;

namespace Game.Audio
{
    public class AudioManager : MonoBehaviour
    {
        private const string MusicVolumeKey = "MusicVolume";
        private const string SfxVolumeKey = "SfxVolume";

        [SerializeField, Min(1)] private int sfxVoices = 16;
        [SerializeField, Range(0f, 1f)] private float defaultVolume = 0.8f;

        private readonly List<AudioSource> sfxSources = new();
        private readonly Dictionary<SoundData, float> nextPlayTimes = new();
        private AudioSource musicSource;
        private SoundData currentMusic;
        private int nextVoice;

        public static AudioManager Instance { get; private set; }

        public float MusicVolume { get; private set; }
        public float SfxVolume { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, defaultVolume);
            SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, defaultVolume);
            musicSource = CreateSource(true);
            for (int i = 0; i < sfxVoices; i++)
                sfxSources.Add(CreateSource(false));
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public static void Play(SoundData sound)
        {
            if (Instance != null && sound != null)
                Instance.PlaySfx(sound);
        }

        public static void PlayMusic(SoundData music)
        {
            if (Instance != null && music != null)
                Instance.StartMusic(music);
        }

        public static void StopMusic()
        {
            if (Instance == null)
                return;
            Instance.musicSource.Stop();
            Instance.currentMusic = null;
        }

        public void SetMusicVolume(float value)
        {
            MusicVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
            if (currentMusic != null)
                musicSource.volume = currentMusic.Volume * MusicVolume;
        }

        public void SetSfxVolume(float value)
        {
            SfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
        }

        private void PlaySfx(SoundData sound)
        {
            if (!sound.HasClips || !TryReserve(sound))
                return;

            AudioSource source = sfxSources[nextVoice];
            nextVoice = (nextVoice + 1) % sfxSources.Count;
            source.pitch = 1f + Random.Range(-sound.PitchVariance, sound.PitchVariance);
            source.PlayOneShot(sound.PickClip(), sound.Volume * SfxVolume);
        }

        private bool TryReserve(SoundData sound)
        {
            float now = Time.unscaledTime;
            if (nextPlayTimes.TryGetValue(sound, out float nextTime) && now < nextTime)
                return false;
            nextPlayTimes[sound] = now + sound.MinInterval;
            return true;
        }

        private void StartMusic(SoundData music)
        {
            AudioClip clip = music.PickClip();
            if (clip == null || (currentMusic == music && musicSource.isPlaying))
                return;

            currentMusic = music;
            musicSource.clip = clip;
            musicSource.volume = music.Volume * MusicVolume;
            musicSource.Play();
        }

        private AudioSource CreateSource(bool loop)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = true;
            return source;
        }
    }
}
