using System.Threading;
using UnityEngine;
using UIFramework.Core;
using System.Collections.Generic;

namespace UIFramework.Testing
{
    /// <summary>
    /// Mock UI View for testing controllers without Unity objects
    /// </summary>
    public class MockUIView : IUIView
    {
        public string ViewId { get; set; } = "MockView";
        public UIState State { get; set; } = UIState.None;
        public UILayer Layer { get; set; } = UILayer.Screen;
        public bool IsVisible => State == UIState.Visible;
        
        public bool InitializeCalled { get; private set; }
        public bool ShowCalled { get; private set; }
        public bool HideCalled { get; private set; }
        public bool RefreshCalled { get; private set; }
        public bool DisposeCalled { get; private set; }
        
        public IUIData LastInitializedData { get; private set; }
        
        public void Initialize(IUIData data)
        {
            InitializeCalled = true;
            LastInitializedData = data;
            State = UIState.Hidden;
        }
        
        public void Show()
        {
            ShowCalled = true;
            State = UIState.Visible;
        }
        
        public void Hide()
        {
            HideCalled = true;
            State = UIState.Hidden;
        }
        
        public void Refresh()
        {
            RefreshCalled = true;
        }
        
        public void Dispose()
        {
            DisposeCalled = true;
            State = UIState.Disposed;
        }
        
        #if UNITASK_SUPPORT
        public async Cysharp.Threading.Tasks.UniTask ShowAsync(CancellationToken cancellationToken = default)
        {
            await Cysharp.Threading.Tasks.UniTask.Yield();
            Show();
        }
        
        public async Cysharp.Threading.Tasks.UniTask HideAsync(CancellationToken cancellationToken = default)
        {
            await Cysharp.Threading.Tasks.UniTask.Yield();
            Hide();
        }
        #endif
        
        public void Reset()
        {
            InitializeCalled = false;
            ShowCalled = false;
            HideCalled = false;
            RefreshCalled = false;
            DisposeCalled = false;
            LastInitializedData = null;
            State = UIState.None;
        }
    }
    
    /// <summary>
    /// Mock event handler for testing
    /// </summary>
    public class MockEventHandler<T> : Communication.IEventHandler<T> where T : Communication.IUIEvent
    {
        private readonly System.Action<T> callback;
        
        public bool WasCalled { get; private set; }
        public T LastEvent { get; private set; }
        public int CallCount { get; private set; }
        
        public MockEventHandler(System.Action<T> callback = null)
        {
            this.callback = callback;
        }
        
        public void Handle(T eventData)
        {
            WasCalled = true;
            LastEvent = eventData;
            CallCount++;
            callback?.Invoke(eventData);
        }
        
        public void Reset()
        {
            WasCalled = false;
            LastEvent = default;
            CallCount = 0;
        }
    }
    
    /// <summary>
    /// Mock UI data for testing
    /// </summary>
    public class MockUIData : UIDataBase
    {
        public string TestValue { get; set; }
        public int TestNumber { get; set; }
        
        public MockUIData(string value = "test", int number = 0)
        {
            TestValue = value;
            TestNumber = number;
        }
    }
}
