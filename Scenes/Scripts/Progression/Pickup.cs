using UnityEngine;

namespace Game.Progression
{
    public class Pickup : MonoBehaviour
    {
        [SerializeField] private PickupKind kind;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public PickupKind Kind => kind;
        public int Value { get; private set; }
        public bool IsAttracted { get; set; }
        public int ListIndex { get; set; }

        public void Init(int value)
        {
            Value = value;
            IsAttracted = false;
        }

        public void AddValue(int amount) => Value += amount;

        public void SetAppearance(Color color, float scale)
        {
            spriteRenderer.color = color;
            transform.localScale = Vector3.one * scale;
        }
    }
}
