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
        [SerializeField] private GameObject exitWarning;

        private bool isExitArmed;

        private void Awake()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(gameManager.TogglePause);
            if (exitButton != null)
                exitButton.onClick.AddListener(HandleExitClicked);
            DisarmExit();
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

        private void HandleStateChanged(GameState state)
        {
            DisarmExit();
            panelRoot.SetActive(state == GameState.Paused);
        }

        private void HandleExitClicked()
        {
            if (isExitArmed || exitWarning == null)
            {
                gameManager.ExitToMainMenu();
                return;
            }

            isExitArmed = true;
            exitWarning.SetActive(true);
        }

        private void DisarmExit()
        {
            isExitArmed = false;
            if (exitWarning != null)
                exitWarning.SetActive(false);
        }
    }
}
