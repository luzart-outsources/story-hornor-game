using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework.Utils
{
    /// <summary>
    /// Reusable object pool for any type
    /// Generic implementation for flexibility
    /// </summary>
    public class ObjectPool<T> where T : class
    {
        private readonly Queue<T> pool = new Queue<T>();
        private readonly HashSet<T> activeObjects = new HashSet<T>();
        private readonly Func<T> createFunc;
        private readonly Action<T> onGet;
        private readonly Action<T> onReturn;
        private readonly int maxSize;
        
        public int PoolSize => pool.Count;
        public int ActiveCount => activeObjects.Count;
        
        public ObjectPool(Func<T> createFunc, Action<T> onGet = null, Action<T> onReturn = null, int maxSize = 100)
        {
            this.createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            this.onGet = onGet;
            this.onReturn = onReturn;
            this.maxSize = maxSize;
        }
        
        public T Get()
        {
            T item;
            
            if (pool.Count > 0)
            {
                item = pool.Dequeue();
            }
            else
            {
                item = createFunc();
            }
            
            activeObjects.Add(item);
            onGet?.Invoke(item);
            
            return item;
        }
        
        public void Return(T item)
        {
            if (item == null)
                return;
            
            if (!activeObjects.Contains(item))
            {
                Debug.LogWarning("[ObjectPool] Trying to return item that wasn't from pool");
                return;
            }
            
            activeObjects.Remove(item);
            
            if (pool.Count < maxSize)
            {
                onReturn?.Invoke(item);
                pool.Enqueue(item);
            }
            else
            {
                // Pool full, discard
                if (item is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
        
        public void Clear()
        {
            while (pool.Count > 0)
            {
                var item = pool.Dequeue();
                if (item is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            
            activeObjects.Clear();
        }
        
        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var item = createFunc();
                onReturn?.Invoke(item);
                pool.Enqueue(item);
            }
        }
    }
}
