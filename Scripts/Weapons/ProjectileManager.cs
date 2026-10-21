using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Weapons
{
    public class ProjectileManager : MonoBehaviour
    {
        public static ProjectileManager Instance { get; private set; }

        private readonly List<Projectile> active = new(256);

        public int ActiveCount => active.Count;

        private void Awake() => Instance = this;

        private void Update()
        {
            if (!GameManager.Instance.IsPlaying)
                return;

            float deltaTime = Time.deltaTime;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                Projectile projectile = active[i];
                projectile.Tick(deltaTime);
                if (projectile.IsFinished)
                    Release(projectile);
            }
        }

        public Projectile Fire(Projectile prefab, in ProjectileShot shot)
        {
            Projectile projectile = PoolManager.Instance.Get(prefab, shot.Origin, Quaternion.identity);
            projectile.Launch(shot);
            projectile.ListIndex = active.Count;
            active.Add(projectile);
            return projectile;
        }

        private void Release(Projectile projectile)
        {
            Projectile last = active[^1];
            active[projectile.ListIndex] = last;
            last.ListIndex = projectile.ListIndex;
            active.RemoveAt(active.Count - 1);
            PoolManager.Instance.Release(projectile);
        }
    }
}
