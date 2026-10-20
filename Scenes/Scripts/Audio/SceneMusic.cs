using Game.Data;
using UnityEngine;

namespace Game.Audio
{
    public class SceneMusic : MonoBehaviour
    {
        [SerializeField] private SoundData music;

        private void Start() => AudioManager.PlayMusic(music);
    }
}
