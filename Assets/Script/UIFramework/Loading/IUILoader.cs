using System;
using UnityEngine;
#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
using System.Threading;
#endif

namespace UIFramework.Loading
{
    public interface IUILoader
    {
#if UNITASK_SUPPORT
        UniTask<T> LoadAsync<T>(string address, Transform parent, CancellationToken cancellationToken = default) where T : Component;
        UniTask<GameObject> LoadAsync(string address, Transform parent, CancellationToken cancellationToken = default);
        UniTask PreloadAsync(string address, CancellationToken cancellationToken = default);
#else
        T Load<T>(string address, Transform parent) where T : Component;
        GameObject Load(string address, Transform parent);
#endif
        void Release(string address, GameObject instance);
        void ReleaseAll();
    }
}
