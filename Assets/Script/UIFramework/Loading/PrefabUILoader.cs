using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
using System.Threading;
#endif

namespace UIFramework.Loading
{
    public class PrefabUILoader : IUILoader
    {
        private readonly Dictionary<string, GameObject> _prefabCache = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, List<GameObject>> _instances = new Dictionary<string, List<GameObject>>();
        private readonly bool _useCaching;

        public PrefabUILoader(bool useCaching = true)
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
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            
            GameObject prefab = GetPrefab(address);
            if (prefab == null)
            {
                Debug.LogError($"[PrefabUILoader] Prefab not found at path: {address}");
                return null;
            }

            GameObject instance = UnityEngine.Object.Instantiate(prefab, parent);
            TrackInstance(address, instance);

            return instance;
        }

        public async UniTask PreloadAsync(string address, CancellationToken cancellationToken = default)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            
            if (_useCaching && !_prefabCache.ContainsKey(address))
            {
                GameObject prefab = Resources.Load<GameObject>(address);
                if (prefab != null)
                {
                    _prefabCache[address] = prefab;
                }
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
            GameObject prefab = GetPrefab(address);
            if (prefab == null)
            {
                Debug.LogError($"[PrefabUILoader] Prefab not found at path: {address}");
                return null;
            }

            GameObject instance = UnityEngine.Object.Instantiate(prefab, parent);
            TrackInstance(address, instance);

            return instance;
        }
#endif

        public void Release(string address, GameObject instance)
        {
            if (instance == null)
                return;

            UntrackInstance(address, instance);
            UnityEngine.Object.Destroy(instance);

            if (!_useCaching && _instances.ContainsKey(address) && _instances[address].Count == 0)
            {
                if (_prefabCache.ContainsKey(address))
                {
                    _prefabCache.Remove(address);
                }
            }
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
                _prefabCache.Clear();
            }
        }

        private GameObject GetPrefab(string address)
        {
            if (_useCaching && _prefabCache.TryGetValue(address, out GameObject cachedPrefab))
            {
                return cachedPrefab;
            }

            GameObject prefab = Resources.Load<GameObject>(address);
            if (prefab != null && _useCaching)
            {
                _prefabCache[address] = prefab;
            }

            return prefab;
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
    }
}
