using Game.Data;
using Game.Online;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class LeaderboardRowView : MonoBehaviour
    {
        private const int SecondsPerMinute = 60;

        [SerializeField] private TMP_Text rankText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Image characterImage;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private GameObject currentUserHighlight;
        [SerializeField] private string rankFormat = "{0:0}";
        [SerializeField] private string scoreFormat = "{0:0}";

        public void Show(long rank, LeaderboardEntry entry, CharacterData character)
        {
            if (rankText != null)
                rankText.SetText(rankFormat, (float)rank);
            nameText.SetText(entry.DisplayName);
            scoreText.SetText(scoreFormat, (float)entry.Score);
            ShowPortrait(character);
            if (timeText != null)
                ShowTime(entry.RunTime);
            if (currentUserHighlight != null)
                currentUserHighlight.SetActive(entry.IsCurrentUser);
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);

        private void ShowPortrait(CharacterData character)
        {
            bool hasPortrait = character != null && character.Portrait != null;
            characterImage.enabled = hasPortrait;
            if (hasPortrait)
                characterImage.sprite = character.Portrait;
        }

        private void ShowTime(float runTime)
        {
            int totalSeconds = Mathf.FloorToInt(runTime);
            timeText.SetText("{0:00}:{1:00}", (float)(totalSeconds / SecondsPerMinute), (float)(totalSeconds % SecondsPerMinute));
        }
    }
}
