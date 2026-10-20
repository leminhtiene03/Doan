using System;
using System.Threading.Tasks;
using Game.Progression;
using UnityEngine;

namespace Game.Online
{
    public class RunResultUploader : MonoBehaviour
    {
        [SerializeField] private RunScoreTracker scoreTracker;

        public event Action OnUploadStarted;
        public event Action<long, bool> OnRankReady;
        public event Action OnSavedOffline;
        public event Action OnUploadFailed;

        private void OnEnable() => scoreTracker.OnRunFinished += HandleRunFinished;

        private void OnDisable() => scoreTracker.OnRunFinished -= HandleRunFinished;

        private void HandleRunFinished(RunResult result) => _ = UploadAsync(result);

        private async Task UploadAsync(RunResult result)
        {
            OnUploadStarted?.Invoke();
            if (!await AuthService.InitializeAsync() || !AuthService.IsSignedIn)
            {
                ReportFailure(AuthMessages.NotSignedIn);
                return;
            }

            try
            {
                (bool isNewBest, bool isSynced) = await RunResultService.SubmitAsync(result);
                if (!isSynced)
                {
                    if (this != null)
                        OnSavedOffline?.Invoke();
                    return;
                }

                long rank = await LeaderboardService.GetRankAsync(result.DifficultyId, result.Score);
                if (this != null)
                    OnRankReady?.Invoke(rank, isNewBest);
            }
            catch (Exception exception)
            {
                ReportFailure(exception.Message);
            }
        }

        private void ReportFailure(string reason)
        {
            Debug.LogWarning($"Run result upload failed: {reason}");
            if (this != null)
                OnUploadFailed?.Invoke();
        }
    }
}
