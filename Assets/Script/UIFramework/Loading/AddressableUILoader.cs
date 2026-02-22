#if ADDRESSABLES_SUPPORT
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
using System.Threading;
#endif

namespace UIFramework.Loading
{
    public class AddressableUILoader : IUILoader
    {
        private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _preloadedHandles = new Dictionary<string, AsyncOperationHandle<GameObject>>();
        private readonly Dictionary<string, List<GameObject>> _instances = new Dictionary<string, List<GameObject>>();
        private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _loadOperations = new Dictionary<string, AsyncOperationHandle<GameObject>>();
        private readonly bool _useCaching;
        private long _totalMemoryUsed;

        public long TotalMemoryUsed => _totalMemoryUsed;

        public AddressableUILoader(bool useCaching = true)
        {
            _useCaching = useCaching;
        }

#if UNITASK_SUPPORT
        public async UniTask<T> LoadAsync<T>(string address, Transform parent, CancellationToken cancellationToken = default) where T : Component
        {
            var go = await LoadAsync(address, parent, cancellationToken);
            if (go == null)
                return null;

            return go.GetComponent<T>();
        }

        public async UniTask<GameObject> LoadAsync(string address, Transform parent, CancellationToken cancellationToken = default)
        {
            if (_loadOperations.ContainsKey(address))
            {
                await UniTask.WaitUntil(() => _loadOperations[address].IsDone, cancellationToken: cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return null;
            }

            AsyncOperationHandle<GameObject> handle;

            if (_preloadedHandles.TryGetValue(address, out var preloadedHandle))
            {
                handle = preloadedHandle;
            }
            else
            {
                _loadOperations[address] = Addressables.LoadAssetAsync<GameObject>(address);
                handle = _loadOperations[address];
                
                await handle.ToUniTask(cancellationToken: cancellationToken);
                
                _loadOperations.Remove(address);

                if (cancellationToken.IsCancellationRequested)
                {
                    Addressables.Release(handle);
                    return null;
                }

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"[AddressableUILoader] Failed to load: {address}");
                    return null;
                }

                if (_useCaching)
                {
                    _preloadedHandles[address] = handle;
                }
            }

            GameObject instance = UnityEngine.Object.Instantiate(handle.Result, parent);
            TrackInstance(address, instance);
            UpdateMemoryTracking();

            return instance;
        }

        public async UniTask PreloadAsync(string address, CancellationToken cancellationToken = default)
        {
            if (_preloadedHandles.ContainsKey(address))
                return;

            var handle = Addressables.LoadAssetAsync<GameObject>(address);
            await handle.ToUniTask(cancellationToken: cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                Addressables.Release(handle);
                return;
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _preloadedHandles[address] = handle;
                UpdateMemoryTracking();
            }
            else
            {
                Debug.LogError($"[AddressableUILoader] Preload failed: {address}");
                Addressables.Release(handle);
            }
        }
#else
        public T Load<T>(string address, Transform parent) where T : Component
        {
            var go = Load(address, parent);
            if (go == null)
                return null;

            return go.GetComponent<T>();
        }

        public GameObject Load(string address, Transform parent)
        {
            Debug.LogWarning("[AddressableUILoader] Synchronous loading is not recommended with Addressables. Consider using UniTask.");
            
            var handle = Addressables.LoadAssetAsync<GameObject>(address);
            handle.WaitForCompletion();

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[AddressableUILoader] Failed to load: {address}");
                return null;
            }

            if (_useCaching && !_preloadedHandles.ContainsKey(address))
            {
                _preloadedHandles[address] = handle;
            }

            GameObject instance = UnityEngine.Object.Instantiate(handle.Result, parent);
            TrackInstance(address, instance);
            UpdateMemoryTracking();

            return instance;
        }
#endif

        public void Release(string address, GameObject instance)
        {
            if (instance == null)
                return;

            UntrackInstance(address, instance);
            UnityEngine.Object.Destroy(instance);

            if (!_useCaching && (!_instances.ContainsKey(address) || _instances[address].Count == 0))
            {
                if (_preloadedHandles.TryGetValue(address, out var handle))
                {
                    Addressables.Release(handle);
                    _preloadedHandles.Remove(address);
                }
            }

            UpdateMemoryTracking();
        }

        public void ReleaseAll()
        {
            foreach (var kvp in _instances)
            {
                foreach (var instance in kvp.Value)
                {
                    if (instance != null)
                    {
                        UnityEngine.Object.Destroy(instance);
                    }
                }
            }

            _instances.Clear();

            if (!_useCaching)
            {
                foreach (var handle in _preloadedHandles.Values)
                {
                    Addressables.Release(handle);
                }
                _preloadedHandles.Clear();
            }

            UpdateMemoryTracking();
        }

        private void TrackInstance(string address, GameObject instance)
        {
            if (!_instances.ContainsKey(address))
            {
                _instances[address] = new List<GameObject>();
            }
            _instances[address].Add(instance);
        }

        private void UntrackInstance(string address, GameObject instance)
        {
            if (_instances.ContainsKey(address))
            {
                _instances[address].Remove(instance);
            }
        }

        private void UpdateMemoryTracking()
        {
            _totalMemoryUsed = 0;
            foreach (var handle in _preloadedHandles.Values)
            {
                if (handle.IsValid() && handle.Result != null)
                {
                    _totalMemoryUsed += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(handle.Result);
                }
            }
        }
    }
}
#endif
