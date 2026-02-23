using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace UIFramework.Loaders
{
    /// <summary>
    /// Addressable-based UI loader
    /// Handles async loading, memory tracking, and release
    /// Note: Requires Unity Addressables package
    /// </summary>
    public class AddressableUILoader : Core.IUILoader
    {
        private readonly Dictionary<GameObject, object> loadedHandles = new Dictionary<GameObject, object>();
        private readonly Dictionary<string, int> memoryUsage = new Dictionary<string, int>();
        
        public GameObject Load(string address, Transform parent)
        {
            Debug.LogWarning("[AddressableUILoader] Synchronous Load is not recommended for Addressables. Use LoadAsync instead.");
            
            #if ADDRESSABLES_SUPPORT
            var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(address);
            var prefab = handle.WaitForCompletion();
            
            if (prefab == null)
            {
                Debug.LogError($"[AddressableUILoader] Failed to load addressable: {address}");
                return null;
            }
            
            var instance = UnityEngine.Object.Instantiate(prefab, parent);
            instance.SetActive(false);
            
            loadedHandles[instance] = handle;
            TrackMemory(address, prefab);
            
            return instance;
            #else
            Debug.LogError("[AddressableUILoader] Addressables support not enabled. Define ADDRESSABLES_SUPPORT.");
            return null;
            #endif
        }
        
        public void Unload(GameObject instance)
        {
            if (instance == null)
                return;
            
            #if ADDRESSABLES_SUPPORT
            if (loadedHandles.TryGetValue(instance, out var handle))
            {
                UnityEngine.AddressableAssets.Addressables.Release(handle);
                loadedHandles.Remove(instance);
            }
            #endif
            
            UnityEngine.Object.Destroy(instance);
        }
        
        #if UNITASK_SUPPORT
        public async Cysharp.Threading.Tasks.UniTask<GameObject> LoadAsync(string address, Transform parent, CancellationToken cancellationToken = default)
        {
            #if ADDRESSABLES_SUPPORT
            try
            {
                var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(address);
                
                // Wait for completion with cancellation support
                while (!handle.IsDone)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        UnityEngine.AddressableAssets.Addressables.Release(handle);
                        throw new OperationCanceledException();
                    }
                    
                    await Cysharp.Threading.Tasks.UniTask.Yield();
                }
                
                var prefab = handle.Result;
                
                if (prefab == null)
                {
                    Debug.LogError($"[AddressableUILoader] Failed to load addressable: {address}");
                    UnityEngine.AddressableAssets.Addressables.Release(handle);
                    return null;
                }
                
                var instance = UnityEngine.Object.Instantiate(prefab, parent);
                instance.SetActive(false);
                
                loadedHandles[instance] = handle;
                TrackMemory(address, prefab);
                
                return instance;
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[AddressableUILoader] Load cancelled: {address}");
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AddressableUILoader] Error loading {address}: {ex.Message}");
                return null;
            }
            #else
            Debug.LogError("[AddressableUILoader] Addressables support not enabled. Define ADDRESSABLES_SUPPORT.");
            await Cysharp.Threading.Tasks.UniTask.Yield();
            return null;
            #endif
        }
        #endif
        
        private void TrackMemory(string address, GameObject prefab)
        {
            #if UNITY_EDITOR
            var size = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(prefab);
            if (memoryUsage.ContainsKey(address))
            {
                memoryUsage[address]++;
            }
            else
            {
                memoryUsage[address] = 1;
            }
            
            Debug.Log($"[AddressableUILoader] Loaded {address}, Memory: {size / 1024}KB, Instances: {memoryUsage[address]}");
            #endif
        }
        
        public Dictionary<string, int> GetMemoryUsage()
        {
            return new Dictionary<string, int>(memoryUsage);
        }
        
        public void ClearMemoryTracking()
        {
            memoryUsage.Clear();
        }
    }
}
