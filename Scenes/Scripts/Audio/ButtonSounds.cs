using Game.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Audio
{
    public class ButtonSounds : MonoBehaviour
    {
        [SerializeField] private SoundData clickSound;

        private void Awake()
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
                buttons[i].onClick.AddListener(PlayClick);
        }

        private void PlayClick() => AudioManager.Play(clickSound);
    }
}
