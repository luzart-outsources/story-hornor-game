using System;

namespace UIFramework.Core
{
    /// <summary>
    /// Base interface for UI controllers (logic layer)
    /// </summary>
    public interface IUIController : IDisposable
    {
        void Initialize(IUIView view, IUIData data);
        void OnShow();
        void OnHide();
        void OnRefresh();
    }
}
