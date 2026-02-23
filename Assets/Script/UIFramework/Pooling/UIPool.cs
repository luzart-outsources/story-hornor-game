using System.Collections.Generic;
using UnityEngine;
using UIFramework.Core;

namespace UIFramework.Pooling
{
    /// <summary>
    /// Object pool for frequently used UI elements
    /// Prevents GC spikes from instantiate/destroy
    /// </summary>
    public class UIPool
    {
        private readonly Dictionary<string, Queue<UIBase>> pools = new Dictionary<string, Queue<UIBase>>();
        private readonly Dictionary<UIBase, string> activeInstances = new Dictionary<UIBase, string>();
        private readonly Transform poolRoot;
        
        public UIPool(Transform poolRoot)
        {
            this.poolRoot = poolRoot;
        }
        
        /// <summary>
        /// Get or create a UI instance from pool
        /// </summary>
        public UIBase Get(string viewId, UIBase prefab, Transform parent)
        {
            if (pools.TryGetValue(viewId, out var pool) && pool.Count > 0)
            {
                var instance = pool.Dequeue();
                instance.transform.SetParent(parent);
                instance.gameObject.SetActive(false);
                activeInstances[instance] = viewId;
                return instance;
            }
            
            // Create new instance if pool is empty
            var newInstance = Object.Instantiate(prefab, parent);
            newInstance.gameObject.SetActive(false);
            activeInstances[newInstance] = viewId;
            return newInstance;
        }
        
        /// <summary>
        /// Return instance to pool
        /// </summary>
        public void Return(UIBase instance)
        {
            if (instance == null)
                return;
                
            if (!activeInstances.TryGetValue(instance, out var viewId))
            {
                Debug.LogWarning("[UIPool] Trying to return instance that wasn't from pool");
                Object.Destroy(instance.gameObject);
                return;
            }
            
            activeInstances.Remove(instance);
            
            if (!pools.ContainsKey(viewId))
            {
                pools[viewId] = new Queue<UIBase>();
            }
            
            // Reset instance
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(poolRoot);
            
            pools[viewId].Enqueue(instance);
        }
        
        /// <summary>
        /// Pre-warm pool with instances
        /// </summary>
        public void Prewarm(string viewId, UIBase prefab, int count)
        {
            if (!pools.ContainsKey(viewId))
            {
                pools[viewId] = new Queue<UIBase>();
            }
            
            for (int i = 0; i < count; i++)
            {
                var instance = Object.Instantiate(prefab, poolRoot);
                instance.gameObject.SetActive(false);
                pools[viewId].Enqueue(instance);
            }
        }
        
        /// <summary>
        /// Clear specific pool
        /// </summary>
        public void ClearPool(string viewId)
        {
            if (pools.TryGetValue(viewId, out var pool))
            {
                while (pool.Count > 0)
                {
                    var instance = pool.Dequeue();
                    if (instance != null)
                    {
                        Object.Destroy(instance.gameObject);
                    }
                }
                
                pools.Remove(viewId);
            }
        }
        
        /// <summary>
        /// Clear all pools
        /// </summary>
        public void ClearAll()
        {
            foreach (var pool in pools.Values)
            {
                while (pool.Count > 0)
                {
                    var instance = pool.Dequeue();
                    if (instance != null)
                    {
                        Object.Destroy(instance.gameObject);
                    }
                }
            }
            
            pools.Clear();
            activeInstances.Clear();
        }
        
        public int GetPoolSize(string viewId)
        {
            return pools.TryGetValue(viewId, out var pool) ? pool.Count : 0;
        }
    }
}
