using System;
using Game.Core;
using Game.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class DifficultySelectView : MonoBehaviour
    {
        [Serializable]
        private class DifficultyOption
        {
            [SerializeField] private Button button;
            [SerializeField] private DifficultyData difficulty;

            public Button Button => button;
            public DifficultyData Difficulty => difficulty;
        }

        [SerializeField] private RunSelection runSelection;
        [SerializeField] private DifficultyOption[] options;
        [SerializeField] private Button backButton;

        private void Awake()
        {
            for (int i = 0; i < options.Length; i++)
            {
                DifficultyData difficulty = options[i].Difficulty;
                options[i].Button.onClick.AddListener(() => Choose(difficulty));
            }

            if (backButton != null)
                backButton.onClick.AddListener(SceneLoader.LoadCharacterSelect);
        }

        private void Choose(DifficultyData difficulty)
        {
            runSelection.SelectDifficulty(difficulty);
            SceneLoader.LoadAccountShop();
        }
    }
}
