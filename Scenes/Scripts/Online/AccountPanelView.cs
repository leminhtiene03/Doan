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

        private void Awake()
        {
            accountButton.onClick.AddListener(TogglePanel);
            saveAccountButton.onClick.AddListener(() => _ = SubmitLinkAsync());
            guestCloseButton.onClick.AddListener(ClosePanels);
            guestSignOutButton.onClick.AddListener(HandleGuestSignOut);
            memberSignOutButton.onClick.AddListener(SignOut);
            changePasswordButton.interactable = false;
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
            bool isOpen = guestPanel.activeSelf || memberPanel.activeSelf;
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
