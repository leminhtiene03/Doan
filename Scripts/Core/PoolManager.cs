using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Game.Core
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [SerializeField] private int defaultCapacity = 32;
        [SerializeField] private int maxSize = 1000;

        private readonly Dictionary<Component, ObjectPool<Component>> poolsByPrefab = new();
        private readonly Dictionary<Component, ObjectPool<Component>> poolsByInstance = new();

        private void Awake() => Instance = this;

        public T Get<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            T instance = (T)GetPool(prefab).Get();
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.gameObject.SetActive(true);
            return instance;
        }

        public void Release(Component instance)
        {
            if (poolsByInstance.TryGetValue(instance, out ObjectPool<Component> pool))
                pool.Release(instance);
        }

        public void Prewarm(Component prefab, int count)
        {
            ObjectPool<Component> pool = GetPool(prefab);
            var created = new List<Component>(count);
            for (int i = pool.CountInactive; i < count; i++)
                created.Add(pool.Get());
            for (int i = 0; i < created.Count; i++)
                pool.Release(created[i]);
        }

        private ObjectPool<Component> GetPool(Component prefab)
        {
            if (poolsByPrefab.TryGetValue(prefab, out ObjectPool<Component> pool))
                return pool;

            pool = new ObjectPool<Component>(
                () => CreateInstance(prefab, pool),
                actionOnGet: null,
                actionOnRelease: instance => instance.gameObject.SetActive(false),
                actionOnDestroy: DestroyInstance,
                collectionCheck: true,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize);
            poolsByPrefab.Add(prefab, pool);
            return pool;
        }

        private Component CreateInstance(Component prefab, ObjectPool<Component> pool)
        {
            Component instance = Instantiate(prefab, transform);
            instance.gameObject.SetActive(false);
            poolsByInstance.Add(instance, pool);
            return instance;
        }

        private void DestroyInstance(Component instance)
        {
            poolsByInstance.Remove(instance);
            if (instance != null)
                Destroy(instance.gameObject);
        }
    }
}
