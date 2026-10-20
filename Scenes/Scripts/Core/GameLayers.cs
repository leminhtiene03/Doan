using UnityEngine;

namespace Game.Core
{
    public static class GameLayers
    {
        public static readonly int PlayerProjectile = LayerMask.NameToLayer("PlayerProjectile");
        public static readonly int EnemyProjectile = LayerMask.NameToLayer("EnemyProjectile");
        public static readonly int EnemyHurtbox = LayerMask.NameToLayer("EnemyHurtbox");
        public static readonly int PlayerHurtbox = LayerMask.NameToLayer("PlayerHurtbox");
        public static readonly int Obstacle = LayerMask.NameToLayer("Obstacle");
    }
}
