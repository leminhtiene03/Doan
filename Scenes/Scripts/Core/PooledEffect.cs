using UnityEngine;

namespace Game.Core
{
    public class PooledEffect : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.3f;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite[] frames;

        private float remainingTime;

        private void OnEnable()
        {
            remainingTime = lifetime;
            ShowFrame();
        }

        private void Update()
        {
            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0f)
            {
                PoolManager.Instance.Release(this);
                return;
            }
            ShowFrame();
        }

        private void ShowFrame()
        {
            if (frames == null || frames.Length == 0)
                return;

            float progress = 1f - remainingTime / lifetime;
            spriteRenderer.sprite = frames[Mathf.Min(frames.Length - 1, (int)(progress * frames.Length))];
        }
    }
}
