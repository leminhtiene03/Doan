using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    public class InventorySlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private const string MaxText = "MAX";
        private const string EvolvedText = "EVO";

        [SerializeField] private Image frameImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private string levelFormat = "{0:0}";
        [SerializeField] private Color levelColor = Color.white;
        [SerializeField] private Color maxColor = new(1f, 0.85f, 0.2f);
        [SerializeField] private Color evolvedColor = new(1f, 0.35f, 0.35f);
        [SerializeField] private Color emptyFrameColor = new(1f, 1f, 1f, 0.35f);

        private Color filledFrameColor;

        public event Action<InventorySlotView> OnHovered;
        public event Action<InventorySlotView> OnUnhovered;

        public bool HasContent { get; private set; }
        public string Title { get; private set; }
        public string LevelLabel { get; private set; }
        public string Description { get; private set; }
        public RectTransform Rect => (RectTransform)transform;

        private void Awake() => filledFrameColor = frameImage.color;

        public void Show(Sprite icon, string title, string description, int level, bool isMaxLevel, bool isEvolved)
        {
            HasContent = true;
            Title = title;
            Description = description;
            frameImage.color = filledFrameColor;
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;

            levelText.gameObject.SetActive(true);
            if (isEvolved)
            {
                levelText.SetText(EvolvedText);
                levelText.color = evolvedColor;
                LevelLabel = EvolvedText;
            }
            else if (isMaxLevel)
            {
                levelText.SetText(MaxText);
                levelText.color = maxColor;
                LevelLabel = MaxText;
            }
            else
            {
                levelText.SetText(levelFormat, (float)level);
                levelText.color = levelColor;
                LevelLabel = $"Lv {level}";
            }
        }

        public void Clear()
        {
            HasContent = false;
            frameImage.color = emptyFrameColor;
            iconImage.enabled = false;
            levelText.gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData) => OnHovered?.Invoke(this);

        public void OnPointerExit(PointerEventData eventData) => OnUnhovered?.Invoke(this);
    }
}
