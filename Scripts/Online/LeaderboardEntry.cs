using System.Collections.Generic;
using Firebase.Firestore;
using Game.Progression;

namespace Game.Online
{
    public class LeaderboardEntry
    {
        public const string ScoreField = "score";
        private const string DisplayNameField = "displayName";
        private const string CharacterIdField = "characterId";
        private const string RunTimeField = "runTime";
        private const string KillsField = "kills";
        private const string LevelField = "level";
        private const string VictoryField = "victory";
        private const string AchievedAtField = "achievedAt";

        public LeaderboardEntry(DocumentSnapshot snapshot)
        {
            UserId = snapshot.Id;
            DisplayName = snapshot.GetValue<string>(DisplayNameField);
            Score = snapshot.GetValue<long>(ScoreField);
            CharacterId = snapshot.GetValue<string>(CharacterIdField);
            RunTime = (float)snapshot.GetValue<double>(RunTimeField);
            Kills = (int)snapshot.GetValue<long>(KillsField);
            Level = (int)snapshot.GetValue<long>(LevelField);
            Victory = snapshot.GetValue<bool>(VictoryField);
        }

        public string UserId { get; }
        public string DisplayName { get; }
        public long Score { get; }
        public string CharacterId { get; }
        public float RunTime { get; }
        public int Kills { get; }
        public int Level { get; }
        public bool Victory { get; }
        public bool IsCurrentUser => UserId == AuthService.UserId;

        public static Dictionary<string, object> ToDocument(RunResult result, string displayName) => new()
        {
            [DisplayNameField] = displayName ?? string.Empty,
            [ScoreField] = result.Score,
            [CharacterIdField] = result.CharacterId,
            [RunTimeField] = result.RunTime,
            [KillsField] = result.Kills,
            [LevelField] = result.Level,
            [VictoryField] = result.Victory,
            [AchievedAtField] = FieldValue.ServerTimestamp
        };
    }
}
