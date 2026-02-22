using System;
using System.Collections.Generic;
using UnityEngine;

namespace Luzart.UIFramework
{
    public class UIObjectPool<T> where T : Component
    {
        private readonly Stack<T> pool = new Stack<T>();
        private readonly T prefab;
        private readonly Transform parent;
        private readonly int maxSize;
        private readonly HashSet<T> activeInstances = new HashSet<T>();

        public int PooledCount => pool.Count;
        public int ActiveCount => activeInstances.Count;

        public UIObjectPool(T prefab, Transform parent, int initialSize = 0, int maxSize = 10)
        {
            this.prefab = prefab ?? throw new ArgumentNullException(nameof(prefab));
            this.parent = parent;
            this.maxSize = maxSize;

            Prewarm(initialSize);
        }

        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var instance = CreateInstance();
                instance.gameObject.SetActive(false);
                pool.Push(instance);
            }
        }

        public T Get()
        {
            T instance;

            if (pool.Count > 0)
            {
                instance = pool.Pop();
            }
            else
            {
                instance = CreateInstance();
            }

            instance.gameObject.SetActive(true);
            activeInstances.Add(instance);
            return instance;
        }

        public void Release(T instance)
        {
            if (instance == null)
                return;

            if (!activeInstances.Remove(instance))
            {
                Debug.LogWarning("UIObjectPool: Trying to release an instance that is not from this pool.");
                return;
            }

            instance.gameObject.SetActive(false);

            if (pool.Count < maxSize)
            {
                pool.Push(instance);
            }
            else
            {
                UnityEngine.Object.Destroy(instance.gameObject);
            }
        }

        public void Clear()
        {
            foreach (var instance in activeInstances)
            {
                if (instance != null)
                {
                    UnityEngine.Object.Destroy(instance.gameObject);
                }
            }
            activeInstances.Clear();

            while (pool.Count > 0)
            {
                var instance = pool.Pop();
                if (instance != null)
                {
                    UnityEngine.Object.Destroy(instance.gameObject);
                }
            }
        }

        private T CreateInstance()
        {
            return UnityEngine.Object.Instantiate(prefab, parent);
        }
    }
}
