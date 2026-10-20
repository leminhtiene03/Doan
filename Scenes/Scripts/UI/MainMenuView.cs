using Game.Core;
using Game.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private RunSelection runSelection;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button exitButton;

        private void Awake()
        {
            runSelection.Clear();
            newGameButton.onClick.AddListener(SceneLoader.LoadCharacterSelect);
            exitButton.onClick.AddListener(SceneLoader.QuitGame);
        }
    }
}
