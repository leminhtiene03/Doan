using System.Collections;
using System.Collections.Generic;
using Game.Audio;
using Game.Data;
using Game.Shop;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class ShopView : MonoBehaviour
    {
        [SerializeField] private ShopSlotView slotPrefab;
        [SerializeField] private Transform slotContainer;
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private string notEnoughGoldText = "Not enough gold!";
        [SerializeField] private string purchasedText = "Purchase successful!";
        [SerializeField] private Color failureColor = Color.red;
        [SerializeField] private Color successColor = Color.green;
        [SerializeField] private SoundData purchaseSound;
        [SerializeField] private SoundData failureSound;
        [SerializeField, Min(0f)] private float messageDuration = 2f;

        private readonly List<ShopSlotView> slots = new();
        private ShopSession session;
        private Coroutine messageRoutine;

        private void Awake()
        {
            if (messageText != null)
                messageText.gameObject.SetActive(false);
        }

        private void OnDestroy() => Unbind();

        public void Show(ShopSession shopSession, IReadOnlyList<ShopItemData> items)
        {
            Unbind();
            session = shopSession;
            session.Wallet.OnGoldChanged += HandleGoldChanged;
            session.OnPurchased += HandlePurchased;

            EnsureSlots(items.Count);
            for (int i = 0; i < slots.Count; i++)
            {
                if (i < items.Count)
                    slots[i].Bind(items[i], session);
                else
                    slots[i].Hide();
            }
            HandleGoldChanged(session.Wallet.Gold);
        }

        private void EnsureSlots(int count)
        {
            while (slots.Count < count)
            {
                ShopSlotView slot = Instantiate(slotPrefab, slotContainer);
                slot.OnBuyClicked += Buy;
                slots.Add(slot);
            }
        }

        private void Buy(ShopItemData item)
        {
            if (session == null || session.IsSoldOut(item))
                return;
            if (!session.CanAfford(item))
            {
                ShowMessage(notEnoughGoldText, failureColor);
                AudioManager.Play(failureSound);
                return;
            }
            if (!session.TryBuy(item))
                return;
            ShowMessage(purchasedText, successColor);
            AudioManager.Play(purchaseSound);
        }

        private void ShowMessage(string text, Color color)
        {
            if (messageText == null)
                return;
            if (messageRoutine != null)
                StopCoroutine(messageRoutine);
            messageText.color = color;
            messageRoutine = StartCoroutine(ShowMessageRoutine(text));
        }

        private IEnumerator ShowMessageRoutine(string text)
        {
            messageText.SetText(text);
            messageText.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(messageDuration);
            messageText.gameObject.SetActive(false);
            messageRoutine = null;
        }

        private void HandlePurchased(ShopItemData item, int price) => RefreshSlots();

        private void HandleGoldChanged(int gold)
        {
            goldText.SetText("{0:0}", (float)gold);
            RefreshSlots();
        }

        private void RefreshSlots()
        {
            for (int i = 0; i < slots.Count; i++)
                slots[i].Refresh();
        }

        private void Unbind()
        {
            if (session == null)
                return;
            session.Wallet.OnGoldChanged -= HandleGoldChanged;
            session.OnPurchased -= HandlePurchased;
            session = null;
        }
    }
}
