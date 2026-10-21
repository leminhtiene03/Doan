using Game.Core;
using Game.Online;
using Game.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class GameOverView : MonoBehaviour
    {
        private const string SavingText = "Saving...";
        private const string SaveFailedText = "Could not save result";
        private const string SavedOfflineText = "Offline - result will sync later";
        private const string AccountGoldFormat = "+{0:0} gold";
        private const string RankFormat = "Rank {0:0}";
        private const string NewBestRankFormat = "Rank {0:0}  New best!";

        [SerializeField] private GameManager gameManager;
        [SerializeField] private RunScoreTracker scoreTracker;
        [SerializeField] private RunResultUploader resultUploader;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private TMP_Text accountGoldText;
        [SerializeField] private LeaderboardListView leaderboardList;

        private void Awake()
        {
            retryButton.onClick.AddListener(gameManager.RestartRun);
            exitButton.onClick.AddListener(gameManager.ExitToMainMenu);
            panelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            gameManager.OnStateChanged += HandleStateChanged;
            scoreTracker.OnRunFinished += ShowResult;
            resultUploader.OnUploadStarted += ShowSaving;
            resultUploader.OnRankReady += ShowRank;
            resultUploader.OnUploadFailed += ShowSaveFailed;
            resultUploader.OnSavedOffline += ShowSavedOffline;
        }

        private void OnDisable()
        {
            gameManager.OnStateChanged -= HandleStateChanged;
            scoreTracker.OnRunFinished -= ShowResult;
            resultUploader.OnUploadStarted -= ShowSaving;
            resultUploader.OnRankReady -= ShowRank;
            resultUploader.OnUploadFailed -= ShowSaveFailed;
            resultUploader.OnSavedOffline -= ShowSavedOffline;
        }

        private void HandleStateChanged(GameState state) => panelRoot.SetActive(state == GameState.GameOver);

        private void ShowResult(RunResult result)
        {
            scoreText.SetText("{0:0}", (float)result.Score);
            if (accountGoldText != null)
                accountGoldText.SetText(AccountGoldFormat, (float)result.AccountGold);
        }

        private void ShowSaving() => rankText.SetText(SavingText);

        private void ShowRank(long rank, bool isNewBest)
        {
            rankText.SetText(isNewBest ? NewBestRankFormat : RankFormat, (float)rank);
            leaderboardList.Load(gameManager.Difficulty.Id);
        }

        private void ShowSaveFailed()
        {
            rankText.SetText(SaveFailedText);
            leaderboardList.Load(gameManager.Difficulty.Id);
        }

        private void ShowSavedOffline()
        {
            rankText.SetText(SavedOfflineText);
            leaderboardList.Load(gameManager.Difficulty.Id);
        }
    }
}
