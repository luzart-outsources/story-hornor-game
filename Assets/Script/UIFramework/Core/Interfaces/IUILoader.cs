using System;
using System.Threading;
using UnityEngine;

namespace UIFramework.Core
{
    /// <summary>
    /// Interface for loading UI prefabs
    /// </summary>
    public interface IUILoader
    {
        GameObject Load(string address, Transform parent);
        void Unload(GameObject instance);
        
        #if UNITASK_SUPPORT
        Cysharp.Threading.Tasks.UniTask<GameObject> LoadAsync(string address, Transform parent, CancellationToken cancellationToken = default);
        #endif
    }
}
