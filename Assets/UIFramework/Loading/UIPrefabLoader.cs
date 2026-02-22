using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Luzart.UIFramework
{
    public class UIPrefabLoader : IUIResourceLoader
    {
        private readonly Dictionary<string, UnityEngine.Object> loadedAssets = new Dictionary<string, UnityEngine.Object>();

        public int PooledCount => loadedAssets.Count;

        public async System.Threading.Tasks.Task<T> LoadAsync<T>(string path, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("UIPrefabLoader: Path is null or empty.");
                return null;
            }

            if (loadedAssets.TryGetValue(path, out var cached) && cached != null)
            {
                return cached as T;
            }

            var asset = Resources.Load<T>(path);
            
            if (asset != null)
            {
                loadedAssets[path] = asset;
            }

            await System.Threading.Tasks.Task.Yield();

            return asset;
        }

        public void Release(string path)
        {
            if (loadedAssets.TryGetValue(path, out var asset))
            {
                loadedAssets.Remove(path);
                
                if (asset != null)
                {
                    Resources.UnloadAsset(asset);
                }
            }
        }

        public void ReleaseAll()
        {
            loadedAssets.Clear();
            Resources.UnloadUnusedAssets();
        }
    }
}
