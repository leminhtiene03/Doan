using UnityEngine;

namespace Game.Core
{
    public abstract class Entity : MonoBehaviour
    {
        public float CurrentHP { get; protected set; }
        public virtual bool CanBeHit => true;
        public bool IsFacingLeft { get; private set; }

        public virtual void ApplySlow(float amount, float duration) { }

        public virtual void AddPull(Vector2 velocity) { }

        public abstract void TakeDamage(DamageInfo info);

        protected abstract void Die();

        protected void Face(bool faceLeft)
        {
            if (faceLeft == IsFacingLeft)
                return;

            IsFacingLeft = faceLeft;
            Vector3 scale = transform.localScale;
            scale.x = faceLeft ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }
}
