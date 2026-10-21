using System;
using System.Threading.Tasks;
using Game.Core;
using Game.Data;
using Game.Online;
using Game.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class PermanentShopView : MonoBehaviour
    {
        private const string LoadingText = "Loading...";
        private const string LoadFailedText = "Could not load your account. Upgrades are disabled for this run.";

        [SerializeField] private RunSelection runSelection;
        [SerializeField] private ShopConfig catalog;
        [SerializeField] private ShopView shopView;
        [SerializeField] private Button startButton;
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text statusText;

        private ShopSession session;

        private void Awake()
        {
            startButton.onClick.AddListener(SceneLoader.LoadRun);
            backButton.onClick.AddListener(SceneLoader.LoadDifficultySelect);
        }

        private void Start() => _ = LoadAsync();

        private void OnDestroy()
        {
            if (session != null)
                session.OnPurchased -= SavePurchase;
        }

        private async Task LoadAsync()
        {
            startButton.interactable = false;
            SetStatus(LoadingText);

            AccountProgress account = await TryLoadAccountAsync();
            if (this == null)
                return;

            SetStatus(account != null ? string.Empty : LoadFailedText);
            account ??= new AccountProgress(0, new ShopLedger());
            runSelection.SetAccount(account);
            session = new ShopSession(account, account.Ledger);
            session.OnPurchased += SavePurchase;
            shopView.Show(session, catalog.Items);
            startButton.interactable = true;
        }

        private static async Task<AccountProgress> TryLoadAccountAsync()
        {
            try
            {
                if (await AuthService.InitializeAsync() && AuthService.IsSignedIn)
                    return await AccountService.LoadAsync();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Account load failed: {exception.Message}");
            }
            return null;
        }

        private static void SavePurchase(ShopItemData item, int price) => AccountService.RecordPurchase(item.Id, price);

        private void SetStatus(string text)
        {
            if (statusText != null)
                statusText.SetText(text);
        }
    }
}
