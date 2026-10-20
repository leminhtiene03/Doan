using Game.Core;
using System.Collections.Generic;
using Game.Data;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.UI
{
    using Game.Player;

    public class StatsPanelView : MonoBehaviour
    {
        private const float PercentScale = 100f;

        private static readonly StatType[] DisplayOrder =
        {
            StatType.MaxHP, StatType.Recovery, StatType.Armor, StatType.MoveSpeed, StatType.Might,
            StatType.AttackSpeed, StatType.Area, StatType.Multishot, StatType.Magnet, StatType.ProjectileSpeed,
            StatType.Duration, StatType.Growth, StatType.Greed, StatType.Revival
        };

        private Player player;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform rowContainer;

        private readonly List<(string Label, string Value)> classStats = new();
        private readonly List<GameObject> rows = new();
        private readonly List<TMP_Text> labelTexts = new();
        private readonly List<TMP_Text> valueTexts = new();
        private bool isOpen;

        private void Awake()
        {
            player = GameManager.Instance.Player;
            CollectRows();
            panelRoot.SetActive(false);
        }

        private void OnEnable() => player.OnStatsChanged += HandleStatsChanged;

        private void OnDisable() => player.OnStatsChanged -= HandleStatsChanged;

        private void Update()
        {
            bool shouldOpen = Keyboard.current != null && Keyboard.current.tabKey.isPressed;
            if (shouldOpen == isOpen)
                return;

            isOpen = shouldOpen;
            panelRoot.SetActive(isOpen);
            if (isOpen)
                Refresh();
        }

        private void HandleStatsChanged()
        {
            if (isOpen)
                Refresh();
        }

        private void CollectRows()
        {
            for (int i = 0; i < rowContainer.childCount; i++)
            {
                Transform row = rowContainer.GetChild(i);
                TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>(true);
                if (texts.Length < 2)
                    continue;
                rows.Add(row.gameObject);
                labelTexts.Add(texts[0]);
                valueTexts.Add(texts[1]);
            }
        }

        private void Refresh()
        {
            classStats.Clear();
            player.GetClassStats(classStats);
            int requiredRows = DisplayOrder.Length + classStats.Count;
            if (labelTexts.Count < requiredRows)
                Debug.LogWarning($"Stats panel has {labelTexts.Count} rows but needs {requiredRows}.");

            int row = 0;
            for (int i = 0; i < DisplayOrder.Length && row < labelTexts.Count; i++, row++)
            {
                labelTexts[row].SetText(GetLabel(DisplayOrder[i]));
                WriteValue(valueTexts[row], DisplayOrder[i]);
            }

            for (int i = 0; i < classStats.Count && row < labelTexts.Count; i++, row++)
            {
                labelTexts[row].SetText(classStats[i].Label);
                valueTexts[row].SetText(classStats[i].Value);
            }

            for (int i = 0; i < rows.Count; i++)
                rows[i].SetActive(i < row);
        }

        private void WriteValue(TMP_Text text, StatType stat)
        {
            PlayerStats stats = player.Stats;
            float value = stats.Get(stat);
            switch (stat)
            {
                case StatType.MaxHP:
                case StatType.Armor:
                case StatType.Revival:
                    text.SetText("{0:0}", value);
                    break;
                case StatType.Recovery:
                    text.SetText("{0:1}/s", value);
                    break;
                case StatType.Multishot:
                    text.SetText("+{0:0}", value);
                    break;
                case StatType.Magnet:
                    text.SetText("{0:1}", value);
                    break;
                default:
                    text.SetText("{0:0}%", value * PercentScale);
                    break;
            }
        }

        private static string GetLabel(StatType stat) => stat switch
        {
            StatType.MaxHP => "Max HP",
            StatType.Recovery => "Recovery",
            StatType.Armor => "Armor",
            StatType.MoveSpeed => "Move Speed",
            StatType.Might => "Might (ATK)",
            StatType.AttackSpeed => "Attack Speed",
            StatType.Area => "Area",
            StatType.Multishot => "Multishot",
            StatType.Magnet => "Magnet",
            StatType.ProjectileSpeed => "Projectile Speed",
            StatType.Duration => "Duration",
            StatType.Growth => "Growth",
            StatType.Greed => "Greed",
            StatType.Revival => "Revival",
            _ => stat.ToString()
        };
    }
}
