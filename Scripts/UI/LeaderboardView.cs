using System;
using Game.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class LeaderboardView : MonoBehaviour
    {
        [Serializable]
        private class DifficultyTab
        {
            [SerializeField] private Button button;
            [SerializeField] private DifficultyData difficulty;

            public Button Button => button;
            public DifficultyData Difficulty => difficulty;
        }

        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private LeaderboardListView list;
        [SerializeField] private DifficultyTab[] tabs;
        [SerializeField] private int defaultTabIndex = 1;

        private void Awake()
        {
            openButton.onClick.AddListener(Open);
            closeButton.onClick.AddListener(Close);
            for (int i = 0; i < tabs.Length; i++)
            {
                int tabIndex = i;
                tabs[i].Button.onClick.AddListener(() => SelectTab(tabIndex));
            }
            panelRoot.SetActive(false);
        }

        private void Open()
        {
            panelRoot.SetActive(true);
            SelectTab(defaultTabIndex);
        }

        private void Close()
        {
            list.CancelLoad();
            panelRoot.SetActive(false);
        }

        private void SelectTab(int tabIndex)
        {
            for (int i = 0; i < tabs.Length; i++)
                tabs[i].Button.interactable = i != tabIndex;
            list.Load(tabs[tabIndex].Difficulty.Id);
        }
    }
}
