using Game.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game.UI
{
    public class PauseView : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private InputActionReference pauseAction;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button exitButton;

        private void Awake()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(gameManager.TogglePause);
            exitButton.onClick.AddListener(gameManager.ExitToMainMenu);
            panelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            gameManager.OnStateChanged += HandleStateChanged;
            pauseAction.action.performed += HandlePausePressed;
            pauseAction.action.Enable();
        }

        private void OnDisable()
        {
            gameManager.OnStateChanged -= HandleStateChanged;
            pauseAction.action.performed -= HandlePausePressed;
        }

        private void HandlePausePressed(InputAction.CallbackContext context) => gameManager.TogglePause();

        private void HandleStateChanged(GameState state) => panelRoot.SetActive(state == GameState.Paused);
    }
}
