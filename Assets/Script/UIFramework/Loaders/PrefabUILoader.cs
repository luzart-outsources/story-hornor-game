using System;
using System.Threading;
using UnityEngine;

namespace UIFramework.Loaders
{
    /// <summary>
    /// Prefab-based UI loader (synchronous/asynchronous)
    /// </summary>
    public class PrefabUILoader : Core.IUILoader
    {
        public GameObject Load(string address, Transform parent)
        {
            // In this mode, address is not used, we pass prefab directly via UIConfig
            return null; // Should be called with LoadWithPrefab instead
        }
        
        public GameObject LoadWithPrefab(GameObject prefab, Transform parent)
        {
            if (prefab == null)
            {
                Debug.LogError("[PrefabUILoader] Prefab is null");
                return null;
            }
            
            var instance = UnityEngine.Object.Instantiate(prefab, parent);
            instance.SetActive(false);
            return instance;
        }
        
        public void Unload(GameObject instance)
        {
            if (instance != null)
            {
                UnityEngine.Object.Destroy(instance);
            }
        }
        
        #if UNITASK_SUPPORT
        public async Cysharp.Threading.Tasks.UniTask<GameObject> LoadAsync(string address, Transform parent, CancellationToken cancellationToken = default)
        {
            // Simulate async for consistency
            await Cysharp.Threading.Tasks.UniTask.Yield(cancellationToken);
            return Load(address, parent);
        }
        
        public async Cysharp.Threading.Tasks.UniTask<GameObject> LoadWithPrefabAsync(GameObject prefab, Transform parent, CancellationToken cancellationToken = default)
        {
            // Simulate async loading for consistency with Addressable version
            await Cysharp.Threading.Tasks.UniTask.Yield(cancellationToken);
            return LoadWithPrefab(prefab, parent);
        }
        #endif
    }
}
