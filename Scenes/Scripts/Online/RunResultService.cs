using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using Game.Progression;

namespace Game.Online
{
    public static class RunResultService
    {
        private const string RecentRunsField = "recentRuns";
        private const string UpdatedAtField = "updatedAt";
        private const int MaxRecentRuns = 10;
        private const int SyncTimeoutMilliseconds = 5000;

        public static async Task<(bool IsNewBest, bool IsSynced)> SubmitAsync(RunResult result)
        {
            DocumentReference userDocument = UserProfileService.CurrentUserDocument;
            DocumentReference entryDocument = LeaderboardService.EntryOf(result.DifficultyId, AuthService.UserId);

            DocumentSnapshot user = await TryGetAsync(userDocument);
            DocumentSnapshot entry = await TryGetAsync(entryDocument);
            bool isNewBest = entry == null || !entry.Exists || result.Score > entry.GetValue<long>(LeaderboardEntry.ScoreField);

            var writes = new List<Task> { WriteUser(userDocument, user, result) };
            if (isNewBest)
                writes.Add(entryDocument.SetAsync(LeaderboardEntry.ToDocument(result, AuthService.DisplayName)));

            Task allWrites = Task.WhenAll(writes);
            if (await Task.WhenAny(allWrites, Task.Delay(SyncTimeoutMilliseconds)) != allWrites)
            {
                _ = AccountService.LogIfFailedAsync(allWrites, "Offline run result");
                return (isNewBest, false);
            }

            await allWrites;
            return (isNewBest, true);
        }

        private static Task WriteUser(DocumentReference userDocument, DocumentSnapshot user, RunResult result)
        {
            var update = new Dictionary<string, object>
            {
                [AccountService.AccountGoldField] = FieldValue.Increment(result.AccountGold),
                [UpdatedAtField] = FieldValue.ServerTimestamp
            };
            if (user != null)
                update[RecentRunsField] = PrependRun(user, result);
            return userDocument.SetAsync(update, SetOptions.MergeAll);
        }

        private static async Task<DocumentSnapshot> TryGetAsync(DocumentReference document)
        {
            try
            {
                return await document.GetSnapshotAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static List<object> PrependRun(DocumentSnapshot user, RunResult result)
        {
            var runs = new List<object>(MaxRecentRuns) { ToDocument(result) };
            if (!user.Exists || !user.TryGetValue(RecentRunsField, out List<object> previousRuns))
                return runs;

            for (int i = 0; i < previousRuns.Count && runs.Count < MaxRecentRuns; i++)
                runs.Add(previousRuns[i]);
            return runs;
        }

        private static Dictionary<string, object> ToDocument(RunResult result) => new()
        {
            ["score"] = result.Score,
            ["accountGold"] = result.AccountGold,
            ["runTime"] = result.RunTime,
            ["kills"] = result.Kills,
            ["level"] = result.Level,
            ["victory"] = result.Victory,
            ["difficultyId"] = result.DifficultyId,
            ["characterId"] = result.CharacterId,
            ["timestamp"] = result.Timestamp
        };
    }
}
