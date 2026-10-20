using System;
using Game.Data;
using Game.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class ShopSlotView : MonoBehaviour
    {
        private const string SoldOutText = "SOLD OUT";
        private const string OwnedText = "Owned";

        [SerializeField] private Button buyButton;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text detailsText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private string priceFormat = "{0:0}";
        [SerializeField] private string levelFormat = "Lv {0:0}/{1:0}";
        [SerializeField] private Color unaffordablePriceColor = Color.red;

        private ShopItemData item;
        private ShopSession session;
        private Color affordablePriceColor;

        public event Action<ShopItemData> OnBuyClicked;

        private void Awake()
        {
            affordablePriceColor = priceText.color;
            buyButton.onClick.AddListener(() => OnBuyClicked?.Invoke(item));
        }

        public void Bind(ShopItemData shopItem, ShopSession shopSession)
        {
            item = shopItem;
            session = shopSession;
            nameText.SetText(item.DisplayName);
            descriptionText.SetText(item.Description);
            ShowDetails();
            iconImage.sprite = item.Icon;
            iconImage.enabled = item.Icon != null;
            gameObject.SetActive(true);
            Refresh();
        }

        public void Hide() => gameObject.SetActive(false);

        public void Refresh()
        {
            if (item == null)
                return;

            bool isSoldOut = session.IsSoldOut(item);
            if (isSoldOut)
                priceText.SetText(item.Kind == ShopItemKind.UnlockWeapon ? OwnedText : SoldOutText);
            else
                priceText.SetText(priceFormat, (float)session.GetPrice(item));
            priceText.color = isSoldOut || session.CanAfford(item) ? affordablePriceColor : unaffordablePriceColor;
            ShowLevel();
            buyButton.interactable = !isSoldOut;
        }

        private void ShowDetails()
        {
            if (detailsText == null)
                return;
            bool hasDetails = !string.IsNullOrEmpty(item.Details);
            detailsText.gameObject.SetActive(hasDetails);
            if (hasDetails)
                detailsText.SetText(item.Details);
        }

        private void ShowLevel()
        {
            if (levelText == null)
                return;
            bool hasLevels = item.MaxPurchases != 1;
            levelText.gameObject.SetActive(hasLevels);
            if (hasLevels)
                levelText.SetText(levelFormat, (float)session.Ledger.GetCount(item), (float)item.MaxPurchases);
        }
    }
}
