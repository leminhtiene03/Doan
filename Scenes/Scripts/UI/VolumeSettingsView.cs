using Game.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class VolumeSettingsView : MonoBehaviour
    {
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        private void Awake()
        {
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(SetSfxVolume);
        }

        private void OnEnable()
        {
            AudioManager manager = AudioManager.Instance;
            bool hasAudio = manager != null;
            musicSlider.interactable = hasAudio;
            sfxSlider.interactable = hasAudio;
            if (!hasAudio)
                return;
            musicSlider.SetValueWithoutNotify(manager.MusicVolume);
            sfxSlider.SetValueWithoutNotify(manager.SfxVolume);
        }

        private static void SetMusicVolume(float value)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.SetMusicVolume(value);
        }

        private static void SetSfxVolume(float value)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.SetSfxVolume(value);
        }
    }
}
