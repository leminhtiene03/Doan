using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using Game.Shop;
using UnityEngine;

namespace Game.Online
{
    public static class AccountService
    {
        internal const string AccountGoldField = "accountGold";
        internal const string PurchasesField = "purchases";
        private const string UpdatedAtField = "updatedAt";

        public static async Task<AccountProgress> LoadAsync()
        {
            DocumentSnapshot user = await UserProfileService.CurrentUserDocument.GetSnapshotAsync();
            var ledger = new ShopLedger();
            if (!user.Exists)
                return new AccountProgress(0, ledger);

            if (user.TryGetValue(PurchasesField, out Dictionary<string, object> purchases))
            {
                foreach (KeyValuePair<string, object> purchase in purchases)
                    ledger.SetCount(purchase.Key, Convert.ToInt32(purchase.Value));
            }
            int gold = user.TryGetValue(AccountGoldField, out long storedGold) ? (int)storedGold : 0;
            return new AccountProgress(gold, ledger);
        }

        public static void RecordPurchase(string itemId, int price)
        {
            var update = new Dictionary<string, object>
            {
                [AccountGoldField] = FieldValue.Increment(-price),
                [$"{PurchasesField}.{itemId}"] = FieldValue.Increment(1),
                [UpdatedAtField] = FieldValue.ServerTimestamp
            };
            _ = LogIfFailedAsync(UserProfileService.CurrentUserDocument.UpdateAsync(update), $"Purchase {itemId}");
        }

        internal static async Task LogIfFailedAsync(Task write, string description)
        {
            try
            {
                await write;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"{description} was rejected by Firestore: {exception.Message}");
            }
        }
    }
}
