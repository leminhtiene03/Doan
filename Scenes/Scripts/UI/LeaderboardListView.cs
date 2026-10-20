using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Data;
using Game.Online;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class LeaderboardListView : MonoBehaviour
    {
        private const string LoadingText = "Loading...";
        private const string EmptyText = "No results yet.";
        private const string LoadFailedText = "Could not load the leaderboard.";

        [SerializeField] private CharacterData[] characters;
        [SerializeField] private LeaderboardRowView rowPrefab;
        [SerializeField] private Transform rowContainer;
        [SerializeField, Min(1)] private int topCount = 10;
        [SerializeField] private bool showCurrentUserRow = true;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private LeaderboardRowView[] podium;

        private LeaderboardRowView[] rows;
        private int requestVersion;

        public void Load(string difficultyId) => _ = LoadAsync(difficultyId);

        public void CancelLoad() => requestVersion++;

        private async Task LoadAsync(string difficultyId)
        {
            int version = ++requestVersion;
            EnsureRows();
            HideRows();
            SetStatus(LoadingText);

            List<LeaderboardEntry> entries = null;
            LeaderboardEntry ownEntry = null;
            long ownRank = 0;
            try
            {
                if (await AuthService.InitializeAsync())
                {
                    entries = await LeaderboardService.GetTopAsync(difficultyId, topCount);
                    if (ShouldAddOwnRow(entries))
                        ownEntry = await LeaderboardService.GetEntryAsync(difficultyId, AuthService.UserId);
                    if (ownEntry != null)
                        ownRank = await LeaderboardService.GetRankAsync(difficultyId, ownEntry.Score);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Leaderboard load failed: {exception.Message}");
                entries = null;
            }

            if (this == null || version != requestVersion)
                return;
            if (entries == null)
            {
                SetStatus(LoadFailedText);
                return;
            }
            ShowEntries(entries, ownEntry, ownRank);
        }

        private bool ShouldAddOwnRow(List<LeaderboardEntry> entries)
        {
            if (!showCurrentUserRow || !AuthService.IsSignedIn)
                return false;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].IsCurrentUser)
                    return false;
            }
            return true;
        }

        private void ShowEntries(List<LeaderboardEntry> entries, LeaderboardEntry ownEntry, long ownRank)
        {
            for (int i = 0; i < entries.Count; i++)
                SlotAt(i).Show(i + 1, entries[i], FindCharacter(entries[i].CharacterId));
            if (ownEntry != null)
                SlotAt(Mathf.Max(entries.Count, podium.Length)).Show(ownRank, ownEntry, FindCharacter(ownEntry.CharacterId));
            SetStatus(entries.Count == 0 ? EmptyText : string.Empty);
        }

        private LeaderboardRowView SlotAt(int index) => index < podium.Length ? podium[index] : rows[index - podium.Length];

        private CharacterData FindCharacter(string characterId)
        {
            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i].Id == characterId)
                    return characters[i];
            }
            return null;
        }

        private void EnsureRows()
        {
            if (rows != null)
                return;
            rows = new LeaderboardRowView[topCount + 1];
            for (int i = 0; i < rows.Length; i++)
                rows[i] = Instantiate(rowPrefab, rowContainer);
        }

        private void HideRows()
        {
            for (int i = 0; i < rows.Length; i++)
                rows[i].Hide();
            for (int i = 0; i < podium.Length; i++)
                podium[i].Hide();
        }

        private void SetStatus(string text)
        {
            if (statusText != null)
                statusText.SetText(text);
        }
    }
}
