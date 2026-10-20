using System;
using Game.Core;
using Game.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class CharacterSelectView : MonoBehaviour
    {
        [Serializable]
        private class CharacterOption
        {
            [SerializeField] private Button button;
            [SerializeField] private CharacterData character;

            public Button Button => button;
            public CharacterData Character => character;
        }

        [SerializeField] private RunSelection runSelection;
        [SerializeField] private CharacterOption[] options;
        [SerializeField] private Button backButton;

        private void Awake()
        {
            for (int i = 0; i < options.Length; i++)
            {
                CharacterData character = options[i].Character;
                options[i].Button.onClick.AddListener(() => Choose(character));
            }

            if (backButton != null)
                backButton.onClick.AddListener(SceneLoader.LoadMainMenu);
        }

        private void Choose(CharacterData character)
        {
            runSelection.SelectCharacter(character);
            SceneLoader.LoadDifficultySelect();
        }
    }
}
