using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

#if ADDRESSABLES_ENABLED
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace Luzart.UIFramework
{
    public class UIAddressableLoader : IUIResourceLoader
    {
#if ADDRESSABLES_ENABLED
        private readonly Dictionary<string, AsyncOperationHandle> loadedHandles = new Dictionary<string, AsyncOperationHandle>();
        private readonly Dictionary<string, int> referenceCount = new Dictionary<string, int>();

        public async System.Threading.Tasks.Task<T> LoadAsync<T>(string address, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogError("UIAddressableLoader: Address is null or empty.");
                return null;
            }

            if (loadedHandles.TryGetValue(address, out var existingHandle))
            {
                if (existingHandle.IsValid() && existingHandle.IsDone)
                {
                    referenceCount[address]++;
                    return existingHandle.Result as T;
                }
            }

            var handle = Addressables.LoadAssetAsync<T>(address);
            
            try
            {
                while (!handle.IsDone)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        if (handle.IsValid())
                        {
                            Addressables.Release(handle);
                        }
                        throw new OperationCanceledException();
                    }
                    
                    await System.Threading.Tasks.Task.Yield();
                }

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    loadedHandles[address] = handle;
                    referenceCount[address] = 1;
                    return handle.Result;
                }
                else
                {
                    Debug.LogError($"UIAddressableLoader: Failed to load asset at '{address}'. Error: {handle.OperationException}");
                    if (handle.IsValid())
                    {
                        Addressables.Release(handle);
                    }
                    return null;
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"UIAddressableLoader: Loading '{address}' was cancelled.");
                throw;
            }
        }

        public void Release(string address)
        {
            if (!loadedHandles.TryGetValue(address, out var handle))
                return;

            if (!referenceCount.TryGetValue(address, out var count))
                return;

            count--;

            if (count <= 0)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
                loadedHandles.Remove(address);
                referenceCount.Remove(address);
            }
            else
            {
                referenceCount[address] = count;
            }
        }

        public void ReleaseAll()
        {
            foreach (var kvp in loadedHandles)
            {
                if (kvp.Value.IsValid())
                {
                    Addressables.Release(kvp.Value);
                }
            }

            loadedHandles.Clear();
            referenceCount.Clear();
        }
#else
        public System.Threading.Tasks.Task<T> LoadAsync<T>(string address, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            Debug.LogError("UIAddressableLoader: Addressables package is not installed. Please install it or use UIPrefabLoader instead.");
            return System.Threading.Tasks.Task.FromResult<T>(null);
        }

        public void Release(string address)
        {
            Debug.LogWarning("UIAddressableLoader: Addressables package is not installed.");
        }

        public void ReleaseAll()
        {
            Debug.LogWarning("UIAddressableLoader: Addressables package is not installed.");
        }
#endif
    }
}
