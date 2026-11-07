using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class InventoryTooltipView : MonoBehaviour
    {
        [SerializeField] private RectTransform panel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Vector2 offset = new(0f, -60f);

        private void Awake() => Hide();

        public void Show(InventorySlotView slot)
        {
            titleText.SetText(slot.Title);
            levelText.SetText(slot.LevelLabel);
            descriptionText.SetText(slot.Description);
            panel.position = slot.Rect.position;
            panel.anchoredPosition += offset;
            panel.gameObject.SetActive(true);
        }

        public void Hide() => panel.gameObject.SetActive(false);
    }
}
