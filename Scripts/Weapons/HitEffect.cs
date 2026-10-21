using Game.Core;

namespace Game.Weapons
{
    public readonly struct HitEffect
    {
        public HitEffect(float slowAmount, float slowDuration)
        {
            SlowAmount = slowAmount;
            SlowDuration = slowDuration;
        }

        public float SlowAmount { get; }
        public float SlowDuration { get; }

        public void ApplyTo(Entity target)
        {
            if (SlowAmount > 0f)
                target.ApplySlow(SlowAmount, SlowDuration);
        }
    }
}
