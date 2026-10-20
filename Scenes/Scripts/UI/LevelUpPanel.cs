using System;
using System.Collections.Generic;
using Game.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class LevelUpPanel : MonoBehaviour
    {
        [Serializable]
        private class UpgradeCardView
        {
            [SerializeField] private Button button;
            [SerializeField] private Image iconImage;
            [SerializeField] private TMP_Text titleText;
            [SerializeField] private TMP_Text descriptionText;

            public Button Button => button;

            public void Bind(UpgradeOption option)
            {
                titleText.SetText(option.Title);
                descriptionText.SetText(option.Description);
                iconImage.sprite = option.Icon;
                iconImage.enabled = option.Icon != null;
            }
        }

        [SerializeField] private LevelUpManager levelUpManager;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private UpgradeCardView[] cards;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private string levelUpTitle = "LEVEL UP!";
        [SerializeField] private string chestTitle = "TREASURE CHEST!";

        private void Awake()
        {
            for (int i = 0; i < cards.Length; i++)
            {
                int index = i;
                cards[i].Button.onClick.AddListener(() => levelUpManager.ChooseUpgrade(index));
            }
            panelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            levelUpManager.OnOptionsReady += Show;
            levelUpManager.OnClosed += Hide;
        }

        private void OnDisable()
        {
            levelUpManager.OnOptionsReady -= Show;
            levelUpManager.OnClosed -= Hide;
        }

        private void Show(IReadOnlyList<UpgradeOption> options)
        {
            panelRoot.SetActive(true);
            if (titleText != null)
                titleText.SetText(levelUpManager.IsChestReward ? chestTitle : levelUpTitle);
            for (int i = 0; i < cards.Length; i++)
            {
                bool hasOption = i < options.Count;
                cards[i].Button.gameObject.SetActive(hasOption);
                if (hasOption)
                    cards[i].Bind(options[i]);
            }
        }

        private void Hide() => panelRoot.SetActive(false);
    }
}
