using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Weapons
{
    public class DamageZone : MonoBehaviour
    {
        private static readonly List<Collider2D> Hits = new(32);

        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float spriteScalePerRadius = 2f;
        [SerializeField] private float hitInterval = 0.5f;
        [SerializeField] private float fadeDuration = 0.5f;

        private readonly Dictionary<Entity, float> nextHitTimes = new();
        private DamageInfo damage;
        private HitEffect hitEffect;
        private float radius;
        private float pullStrength;
        private float remainingTime;
        private Color baseColor;
        private bool hasBaseColor;

        public bool IsFinished => remainingTime <= 0f;
        public Sprite Sprite => spriteRenderer.sprite;

        public void Activate(
            Vector2 position, float zoneRadius, float lifetime, float pull, in DamageInfo zoneDamage, in HitEffect effect)
        {
            if (!hasBaseColor)
            {
                baseColor = spriteRenderer.color;
                hasBaseColor = true;
            }
            transform.position = position;
            transform.localScale = Vector3.one * (zoneRadius * spriteScalePerRadius);
            spriteRenderer.color = baseColor;
            radius = zoneRadius;
            pullStrength = pull;
            remainingTime = lifetime;
            damage = zoneDamage;
            hitEffect = effect;
            nextHitTimes.Clear();
        }

        public void Tick(float deltaTime)
        {
            remainingTime -= deltaTime;
            if (IsFinished)
                return;

            HitEnemiesInside();
            if (remainingTime < fadeDuration)
            {
                Color color = baseColor;
                color.a *= remainingTime / fadeDuration;
                spriteRenderer.color = color;
            }
        }

        private void HitEnemiesInside()
        {
            var filter = new ContactFilter2D();
            filter.SetLayerMask(1 << GameLayers.EnemyHurtbox);
            Physics2D.OverlapCircle(transform.position, radius, filter, Hits);

            for (int i = 0; i < Hits.Count; i++)
            {
                if (!Hits[i].TryGetComponent(out Hurtbox hurtbox) || !hurtbox.Owner.CanBeHit)
                    continue;
                if (pullStrength > 0f)
                    hurtbox.Owner.AddPull(((Vector2)transform.position - (Vector2)hurtbox.transform.position).normalized * pullStrength);
                if (nextHitTimes.TryGetValue(hurtbox.Owner, out float readyTime) && readyTime > Time.time)
                    continue;

                nextHitTimes[hurtbox.Owner] = Time.time + hitInterval;
                hurtbox.Owner.TakeDamage(damage);
                hitEffect.ApplyTo(hurtbox.Owner);
            }
        }
    }
}
