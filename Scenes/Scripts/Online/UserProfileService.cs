using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;

namespace Game.Online
{
    public static class UserProfileService
    {
        private const string UsersCollection = "users";
        private const int ProfileVersion = 1;

        internal static DocumentReference CurrentUserDocument
            => FirebaseFirestore.DefaultInstance.Collection(UsersCollection).Document(AuthService.UserId);

        public static async Task EnsureProfileAsync(string displayName)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("[Firebase] Creating Firestore and reading profile...");
#endif
            DocumentReference document = CurrentUserDocument;
            DocumentSnapshot snapshot = await document.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                await document.UpdateAsync("updatedAt", FieldValue.ServerTimestamp);
                return;
            }

            var profile = new Dictionary<string, object>
            {
                ["displayName"] = displayName ?? string.Empty,
                ["createdAt"] = FieldValue.ServerTimestamp,
                ["updatedAt"] = FieldValue.ServerTimestamp,
                ["version"] = ProfileVersion,
                [AccountService.AccountGoldField] = 0,
                [AccountService.PurchasesField] = new Dictionary<string, object>(),
                ["recentRuns"] = new List<object>()
            };
            await document.SetAsync(profile);
        }
    }
}
