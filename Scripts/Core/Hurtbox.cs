using UnityEngine;

namespace Game.Core
{
    public class Hurtbox : MonoBehaviour
    {
        public Entity Owner { get; private set; }
        public Collider2D Collider { get; private set; }

        public static Hurtbox Create(Entity owner, SpriteRenderer spriteRenderer, Vector2 sizeScale, int layer)
        {
            var hurtboxObject = new GameObject("Hurtbox") { layer = layer };
            hurtboxObject.transform.SetParent(owner.transform, false);

            Bounds spriteBounds = spriteRenderer.sprite.bounds;
            Vector2 size = Vector2.Scale(spriteBounds.size, sizeScale);
            var capsule = hurtboxObject.AddComponent<CapsuleCollider2D>();
            capsule.direction = size.y >= size.x ? CapsuleDirection2D.Vertical : CapsuleDirection2D.Horizontal;
            capsule.offset = spriteBounds.center;
            capsule.size = size;

            var hurtbox = hurtboxObject.AddComponent<Hurtbox>();
            hurtbox.Owner = owner;
            hurtbox.Collider = capsule;
            return hurtbox;
        }
    }
}
