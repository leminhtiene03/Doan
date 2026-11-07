using UnityEngine;

namespace Game.Enemies
{
    public class ChargerEnemy : Enemy
    {
        private enum ChargePhase { Approach, Windup, Charge }

        [SerializeField] private float chargeRange = 7f;
        [SerializeField] private float windupTime = 0.6f;
        [SerializeField] private float chargeSpeed = 9f;
        [SerializeField] private float chargeDuration = 1f;
        [SerializeField] private float chargeCooldown = 3f;
        [SerializeField] private float chargeHitReach = 0.25f;
        [SerializeField] private bool passThroughEnemies = true;
        [SerializeField] private GameObject windupIndicator;

        private ChargePhase phase;
        private float phaseTimer;
        private float cooldownTimer;
        private Vector2 chargeDirection;

        protected override void ResetBehavior()
        {
            phase = ChargePhase.Approach;
            phaseTimer = 0f;
            cooldownTimer = 0f;
            chargeDirection = Vector2.zero;
            SetIndicator(false);
            if (!IsTraveling)
                Body.excludeLayers = 0;
        }

        protected override void TickChase(Vector2 playerPosition, Collider2D playerCollider, float deltaTime)
        {
            switch (phase)
            {
                case ChargePhase.Windup:
                    TickWindup(playerPosition, deltaTime);
                    break;
                case ChargePhase.Charge:
                    TickCharge(playerPosition, playerCollider, deltaTime);
                    break;
                default:
                    TickApproach(playerPosition, playerCollider, deltaTime);
                    break;
            }
        }

        private void TickApproach(Vector2 playerPosition, Collider2D playerCollider, float deltaTime)
        {
            cooldownTimer -= deltaTime;
            bool canCharge = !IsTraveling && cooldownTimer <= 0f
                && (playerPosition - Body.position).sqrMagnitude <= chargeRange * chargeRange;
            if (canCharge)
                EnterWindup(deltaTime);
            else
                base.TickChase(playerPosition, playerCollider, deltaTime);
        }

        private void EnterWindup(float deltaTime)
        {
            phase = ChargePhase.Windup;
            phaseTimer = windupTime;
            SetVelocity(Vector2.zero, deltaTime);
            SetLocomotionAnimation(HasIdleAnimation ? IdleHash : MoveHash);
            SetIndicator(true);
        }

        private void TickWindup(Vector2 playerPosition, float deltaTime)
        {
            Vector2 toPlayer = playerPosition - Body.position;
            FaceToward(toPlayer);
            SetVelocity(Vector2.zero, deltaTime);
            phaseTimer -= deltaTime;
            if (phaseTimer <= 0f)
                EnterCharge(toPlayer);
        }

        private void EnterCharge(Vector2 toPlayer)
        {
            phase = ChargePhase.Charge;
            phaseTimer = chargeDuration;
            chargeDirection = toPlayer.sqrMagnitude > 0f ? toPlayer.normalized : Vector2.right;
            SetIndicator(false);
            if (passThroughEnemies)
                Body.excludeLayers = 1 << gameObject.layer;
            PlayAnimation(MoveHash);
        }

        private void TickCharge(Vector2 playerPosition, Collider2D playerCollider, float deltaTime)
        {
            FaceToward(chargeDirection);
            SetVelocity(chargeDirection * (chargeSpeed * SlowFactor), deltaTime);
            phaseTimer -= deltaTime;
            if (IsWithinReach(playerPosition, playerCollider, chargeHitReach))
            {
                LandAttack();
                EndCharge();
            }
            else if (phaseTimer <= 0f)
            {
                EndCharge();
            }
        }

        private void EndCharge()
        {
            phase = ChargePhase.Approach;
            cooldownTimer = chargeCooldown;
            Body.excludeLayers = 0;
        }

        private void SetIndicator(bool visible)
        {
            if (windupIndicator != null)
                windupIndicator.SetActive(visible);
        }
    }
}
