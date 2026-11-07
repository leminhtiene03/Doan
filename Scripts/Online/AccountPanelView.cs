using System.Threading.Tasks;
using Game.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Online
{
    public class AccountPanelView : MonoBehaviour
    {
        [Header("Account button")]
        [SerializeField] private Button accountButton;
        [SerializeField] private TMP_Text nameText;

        [Header("Guest panel")]
        [SerializeField] private GameObject guestPanel;
        [SerializeField] private TMP_InputField linkEmailInput;
        [SerializeField] private TMP_InputField linkPasswordInput;
        [SerializeField] private TMP_InputField linkConfirmInput;
        [SerializeField] private Button saveAccountButton;
        [SerializeField] private Button guestCloseButton;
        [SerializeField] private Button guestSignOutButton;
        [SerializeField] private GameObject signOutWarning;
        [SerializeField] private TMP_Text statusText;

        [Header("Member panel")]
        [SerializeField] private GameObject memberPanel;
        [SerializeField] private TMP_Text emailText;
        [SerializeField] private Button memberSignOutButton;
        [SerializeField] private Button changePasswordButton;

        [Header("Change password panel")]
        [SerializeField] private GameObject changePasswordPanel;
        [SerializeField] private TMP_InputField currentPasswordInput;
        [SerializeField] private TMP_InputField newPasswordInput;
        [SerializeField] private TMP_InputField confirmNewPasswordInput;
        [SerializeField] private Button submitPasswordButton;
        [SerializeField] private Button closePasswordButton;
        [SerializeField] private TMP_Text passwordStatusText;

        private bool HasChangePasswordPanel => changePasswordPanel != null;

        private void Awake()
        {
            accountButton.onClick.AddListener(TogglePanel);
            saveAccountButton.onClick.AddListener(() => _ = SubmitLinkAsync());
            guestCloseButton.onClick.AddListener(ClosePanels);
            guestSignOutButton.onClick.AddListener(HandleGuestSignOut);
            memberSignOutButton.onClick.AddListener(SignOut);
            SetUpChangePassword();
            ClosePanels();
        }

        private async void Start()
        {
            accountButton.interactable = false;
            await AuthService.InitializeAsync();
            if (this == null)
                return;
            if (!AuthService.IsSignedIn)
            {
                SceneLoader.LoadLogin();
                return;
            }
            accountButton.interactable = true;
            nameText.SetText(AuthService.DisplayName);
        }

        private void TogglePanel()
        {
            bool isOpen = guestPanel.activeSelf || memberPanel.activeSelf
                || (HasChangePasswordPanel && changePasswordPanel.activeSelf);
            ClosePanels();
            if (isOpen)
                return;

            if (AuthService.IsGuest)
            {
                guestPanel.SetActive(true);
                return;
            }
            emailText.SetText(AuthService.CurrentUser.Email);
            memberPanel.SetActive(true);
        }

        private void ClosePanels()
        {
            guestPanel.SetActive(false);
            memberPanel.SetActive(false);
            signOutWarning.SetActive(false);
            statusText.SetText(string.Empty);
            CloseChangePassword();
        }

        private void SetUpChangePassword()
        {
            changePasswordButton.interactable = HasChangePasswordPanel;
            if (!HasChangePasswordPanel)
                return;
            changePasswordButton.onClick.AddListener(OpenChangePassword);
            submitPasswordButton.onClick.AddListener(() => _ = SubmitChangePasswordAsync());
            closePasswordButton.onClick.AddListener(BackToMemberPanel);
        }

        private void OpenChangePassword()
        {
            currentPasswordInput.text = string.Empty;
            newPasswordInput.text = string.Empty;
            confirmNewPasswordInput.text = string.Empty;
            passwordStatusText.SetText(string.Empty);
            submitPasswordButton.interactable = true;
            memberPanel.SetActive(false);
            changePasswordPanel.SetActive(true);
        }

        private void CloseChangePassword()
        {
            if (HasChangePasswordPanel)
                changePasswordPanel.SetActive(false);
        }

        private void BackToMemberPanel()
        {
            CloseChangePassword();
            memberPanel.SetActive(true);
        }

        private async Task SubmitChangePasswordAsync()
        {
            string currentPassword = currentPasswordInput.text;
            string newPassword = newPasswordInput.text;
            string error = string.IsNullOrEmpty(currentPassword) ? AuthMessages.EnterCurrentPassword
                : newPassword.Length < AuthService.MinPasswordLength ? AuthMessages.PasswordTooShort
                : newPassword != confirmNewPasswordInput.text ? AuthMessages.PasswordsDoNotMatch
                : newPassword == currentPassword ? AuthMessages.SamePassword
                : null;
            if (error != null)
            {
                passwordStatusText.SetText(error);
                return;
            }

            submitPasswordButton.interactable = false;
            passwordStatusText.SetText(AuthMessages.Working);
            AuthOutcome outcome = await AuthService.ChangePasswordAsync(currentPassword, newPassword);
            if (this == null)
                return;

            submitPasswordButton.interactable = true;
            passwordStatusText.SetText(outcome.Message);
            if (outcome.Success)
            {
                currentPasswordInput.text = string.Empty;
                newPasswordInput.text = string.Empty;
                confirmNewPasswordInput.text = string.Empty;
            }
        }

        private void HandleGuestSignOut()
        {
            if (signOutWarning.activeSelf)
                SignOut();
            else
                signOutWarning.SetActive(true);
        }

        private void SignOut()
        {
            AuthService.SignOut();
            SceneLoader.LoadLogin();
        }

        private async Task SubmitLinkAsync()
        {
            string email = linkEmailInput.text.Trim();
            string password = linkPasswordInput.text;
            string error = string.IsNullOrEmpty(email) ? AuthMessages.EnterEmail
                : password.Length < AuthService.MinPasswordLength ? AuthMessages.PasswordTooShort
                : password != linkConfirmInput.text ? AuthMessages.PasswordsDoNotMatch
                : null;
            if (error != null)
            {
                statusText.SetText(error);
                return;
            }

            saveAccountButton.interactable = false;
            statusText.SetText(AuthMessages.Working);
            AuthOutcome outcome = await AuthService.LinkGuestToEmailAsync(email, password);
            if (this == null)
                return;

            saveAccountButton.interactable = true;
            statusText.SetText(outcome.Message);
            if (!outcome.Success)
                return;

            ClosePanels();
            emailText.SetText(AuthService.CurrentUser.Email);
            memberPanel.SetActive(true);
        }
    }
}
