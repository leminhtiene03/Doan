using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;

namespace Game.Online
{
    public static class LeaderboardService
    {
        private const string LeaderboardsCollection = "leaderboards";
        private const string EntriesCollection = "entries";

        public static DocumentReference EntryOf(string difficultyId, string userId)
            => EntriesOf(difficultyId).Document(userId);

        public static async Task<List<LeaderboardEntry>> GetTopAsync(string difficultyId, int count)
        {
            QuerySnapshot snapshot = await EntriesOf(difficultyId)
                .OrderByDescending(LeaderboardEntry.ScoreField)
                .Limit(count)
                .GetSnapshotAsync();

            var entries = new List<LeaderboardEntry>(snapshot.Count);
            foreach (DocumentSnapshot document in snapshot.Documents)
                entries.Add(new LeaderboardEntry(document));
            return entries;
        }

        public static async Task<LeaderboardEntry> GetEntryAsync(string difficultyId, string userId)
        {
            DocumentSnapshot snapshot = await EntryOf(difficultyId, userId).GetSnapshotAsync();
            return snapshot.Exists ? new LeaderboardEntry(snapshot) : null;
        }

        public static async Task<long> GetRankAsync(string difficultyId, long score)
        {
            AggregateQuerySnapshot snapshot = await EntriesOf(difficultyId)
                .WhereGreaterThan(LeaderboardEntry.ScoreField, score)
                .Count
                .GetSnapshotAsync(AggregateSource.Server);
            return snapshot.Count + 1;
        }

        private static CollectionReference EntriesOf(string difficultyId)
            => FirebaseFirestore.DefaultInstance
                .Collection(LeaderboardsCollection)
                .Document(difficultyId)
                .Collection(EntriesCollection);
    }
}
