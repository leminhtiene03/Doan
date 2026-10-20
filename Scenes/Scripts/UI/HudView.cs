using Game.Core;
using Game.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    using Game.Player;

    public class HudView : MonoBehaviour
    {
        private const int SecondsPerMinute = 60;

        private Player player;
        [SerializeField] private PlayerProgression progression;
        [SerializeField] private Slider healthBar;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private Slider xpBar;
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text levelText;

        private int shownHealth = -1;
        private int shownMaxHealth = -1;
        private int shownSeconds = -1;

        private void Awake() => player = GameManager.Instance.Player;

        private void OnEnable()
        {
            player.OnHealthChanged += UpdateHealth;
            progression.OnXPChanged += UpdateXP;
            progression.OnGoldChanged += UpdateGold;
            progression.OnLevelChanged += UpdateLevel;
        }

        private void OnDisable()
        {
            player.OnHealthChanged -= UpdateHealth;
            progression.OnXPChanged -= UpdateXP;
            progression.OnGoldChanged -= UpdateGold;
            progression.OnLevelChanged -= UpdateLevel;
        }

        private void Start()
        {
            UpdateHealth(player.CurrentHP, player.Stats.MaxHP);
            UpdateXP(progression.CurrentXP, progression.RequiredXP);
            UpdateGold(progression.Gold);
            UpdateLevel(progression.Level);
        }

        private void Update() => UpdateTime(GameManager.Instance.RunTime);

        private void UpdateHealth(float current, float max)
        {
            healthBar.normalizedValue = max > 0f ? current / max : 0f;
            int health = Mathf.Max(0, Mathf.CeilToInt(current));
            int maxHealth = Mathf.CeilToInt(max);
            if (health == shownHealth && maxHealth == shownMaxHealth)
                return;

            shownHealth = health;
            shownMaxHealth = maxHealth;
            healthText.SetText("{0:0}/{1:0}", (float)health, (float)maxHealth);
        }

        private void UpdateXP(float current, float required) => xpBar.normalizedValue = required > 0f ? current / required : 0f;

        private void UpdateGold(int gold) => goldText.SetText("{0:0}", (float)gold);

        private void UpdateLevel(int level) => levelText.SetText("{0:0}", (float)level);

        private void UpdateTime(float runTime)
        {
            int seconds = Mathf.FloorToInt(runTime);
            if (seconds == shownSeconds)
                return;

            shownSeconds = seconds;
            int minutes = seconds / SecondsPerMinute;
            int remainingSeconds = seconds % SecondsPerMinute;
            if (remainingSeconds < 10)
                timeText.SetText("{0:0}:0{1:0}", (float)minutes, (float)remainingSeconds);
            else
                timeText.SetText("{0:0}:{1:0}", (float)minutes, (float)remainingSeconds);
        }
    }
}
