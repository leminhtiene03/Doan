using System.Threading.Tasks;
using Game.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Online
{
    public class LoginView : MonoBehaviour
    {
        private const int MinNameLength = 3;
        private const int MaxNameLength = 16;

        [Header("Panels")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private GameObject registerPanel;

        [Header("Login")]
        [SerializeField] private TMP_InputField loginEmailInput;
        [SerializeField] private TMP_InputField loginPasswordInput;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button forgotPasswordButton;
        [SerializeField] private Button openRegisterButton;
        [SerializeField] private TMP_Text loginStatusText;

        [Header("Guest (inside login panel)")]
        [SerializeField] private TMP_InputField guestNameInput;
        [SerializeField] private Button guestLoginButton;

        [Header("Register")]
        [SerializeField] private TMP_InputField registerEmailInput;
        [SerializeField] private TMP_InputField registerPasswordInput;
        [SerializeField] private TMP_InputField registerConfirmInput;
        [SerializeField] private TMP_InputField registerNameInput;
        [SerializeField] private Button registerButton;
        [SerializeField] private Button registerBackButton;
        [SerializeField] private TMP_Text registerStatusText;

        private Button[] allButtons;
        private TMP_Text activeStatusText;

        private void Awake()
        {
            allButtons = new[]
            {
                loginButton, forgotPasswordButton, openRegisterButton, guestLoginButton,
                registerButton, registerBackButton
            };
            loginButton.onClick.AddListener(() => _ = SubmitLoginAsync());
            forgotPasswordButton.onClick.AddListener(() => _ = SubmitPasswordResetAsync());
            openRegisterButton.onClick.AddListener(() => ShowPanel(registerPanel));
            registerButton.onClick.AddListener(() => _ = SubmitRegisterAsync());
            registerBackButton.onClick.AddListener(() => ShowPanel(loginPanel));
            guestLoginButton.onClick.AddListener(() => _ = SubmitGuestAsync());
        }

        private async void Start()
        {
            ShowPanel(loginPanel);
            SetInteractable(false);
            SetStatus(AuthMessages.Connecting);
            if (!await AuthService.InitializeAsync())
            {
                SetStatus(AuthMessages.FirebaseUnavailable);
                return;
            }

            if (AuthService.IsSignedIn)
            {
                await AuthService.RefreshProfileAsync();
                SceneLoader.LoadMainMenu();
                return;
            }

            SetInteractable(true);
            SetStatus(string.Empty);
        }

        private Task SubmitLoginAsync()
        {
            string email = loginEmailInput.text.Trim();
            string password = loginPasswordInput.text;
            string error = ValidateEmail(email) ?? ValidatePassword(password);
            return error != null ? ShowError(error) : RunAsync(AuthService.SignInWithEmailAsync(email, password), true);
        }

        private Task SubmitPasswordResetAsync()
        {
            string email = loginEmailInput.text.Trim();
            string error = ValidateEmail(email);
            return error != null ? ShowError(error) : RunAsync(AuthService.SendPasswordResetAsync(email), false);
        }

        private Task SubmitRegisterAsync()
        {
            string email = registerEmailInput.text.Trim();
            string password = registerPasswordInput.text;
            string displayName = registerNameInput.text.Trim();
            string error = ValidateEmail(email)
                ?? ValidatePassword(password)
                ?? (password != registerConfirmInput.text ? AuthMessages.PasswordsDoNotMatch : null)
                ?? ValidateName(displayName);
            return error != null ? ShowError(error) : RunAsync(AuthService.RegisterAsync(email, password, displayName), true);
        }

        private Task SubmitGuestAsync()
        {
            string displayName = guestNameInput.text.Trim();
            string error = ValidateName(displayName);
            return error != null ? ShowError(error) : RunAsync(AuthService.SignInAsGuestAsync(displayName), true);
        }

        private async Task RunAsync(Task<AuthOutcome> request, bool enterMenuOnSuccess)
        {
            SetInteractable(false);
            SetStatus(AuthMessages.Working);
            AuthOutcome outcome = await request;
            if (this == null)
                return;

            SetInteractable(true);
            SetStatus(outcome.Message);
            if (outcome.Success && enterMenuOnSuccess)
                SceneLoader.LoadMainMenu();
        }

        private Task ShowError(string message)
        {
            SetStatus(message);
            return Task.CompletedTask;
        }

        private void ShowPanel(GameObject panel)
        {
            loginPanel.SetActive(panel == loginPanel);
            registerPanel.SetActive(panel == registerPanel);
            activeStatusText = panel == registerPanel ? registerStatusText : loginStatusText;
            SetStatus(string.Empty);
        }

        private void SetInteractable(bool interactable)
        {
            for (int i = 0; i < allButtons.Length; i++)
                allButtons[i].interactable = interactable;
        }

        private void SetStatus(string message) => activeStatusText.SetText(message);

        private static string ValidateEmail(string email)
            => string.IsNullOrEmpty(email) ? AuthMessages.EnterEmail : null;

        private static string ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return AuthMessages.EnterPassword;
            return password.Length < AuthService.MinPasswordLength ? AuthMessages.PasswordTooShort : null;
        }

        private static string ValidateName(string displayName)
            => displayName.Length < MinNameLength || displayName.Length > MaxNameLength ? AuthMessages.NameLength : null;
    }
}
