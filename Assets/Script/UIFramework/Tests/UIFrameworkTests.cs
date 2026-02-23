#if UNITY_EDITOR && NUNIT_SUPPORT
// Unit tests require NUnit Test Framework package
// Install from: Window ? Package Manager ? Unity Registry ? Test Framework
using NUnit.Framework;
using UIFramework.Core;
using UIFramework.Communication;

namespace UIFramework.Tests
{
    [TestFixture]
    public class UIFrameworkTests
    {
        [Test]
        public void EventBus_SubscribeAndPublish_HandlerReceivesEvent()
        {
            var eventBus = new EventBus();
            var handler = new TestEventHandler();
            eventBus.Subscribe<TestEvent>(handler);
            var testEvent = new TestEvent { Message = "Test" };
            
            eventBus.Publish(testEvent);
            
            Assert.IsTrue(handler.WasCalled);
            Assert.AreEqual("Test", handler.ReceivedMessage);
        }
        
        [Test]
        public void EventBus_Unsubscribe_HandlerDoesNotReceiveEvent()
        {
            var eventBus = new EventBus();
            var handler = new TestEventHandler();
            eventBus.Subscribe<TestEvent>(handler);
            eventBus.Unsubscribe<TestEvent>(handler);
            var testEvent = new TestEvent { Message = "Test" };
            
            eventBus.Publish(testEvent);
            
            Assert.IsFalse(handler.WasCalled);
        }
        
        [Test]
        public void UIController_Initialize_SetsViewAndData()
        {
            var controller = new TestController();
            var view = new TestUIView();
            var data = new TestUIData();
            
            controller.Initialize(view, data);
            
            Assert.IsTrue(controller.IsInitialized);
        }
        
        [Test]
        public void UIData_Immutability_CreateNewInstance()
        {
            var data = new TestUIData { Value = 10 };
            var newData = data.WithValue(20);
            
            Assert.AreEqual(10, data.Value);
            Assert.AreEqual(20, newData.Value);
        }
    }
}
#endif

// Test helper classes are always available for use in other contexts
namespace UIFramework.Tests
{
    using System.Threading;
    using UIFramework.Core;
    using UIFramework.Communication;
    
    public class TestEvent : IUIEvent
    {
        public string Message { get; set; }
    }
    
    public class TestEventHandler : IEventHandler<TestEvent>
    {
        public bool WasCalled { get; private set; }
        public string ReceivedMessage { get; private set; }
        
        public void Handle(TestEvent eventData)
        {
            WasCalled = true;
            ReceivedMessage = eventData.Message;
        }
    }
    
    public class TestUIView : IUIView
    {
        public string ViewId => "TestView";
        public UIState State => UIState.None;
        public UILayer Layer => UILayer.Screen;
        public bool IsVisible => false;
        
        public void Initialize(IUIData data) { }
        public void Show() { }
        public void Hide() { }
        public void Refresh() { }
        public void Dispose() { }
        
        #if UNITASK_SUPPORT
        public async Cysharp.Threading.Tasks.UniTask ShowAsync(CancellationToken cancellationToken = default)
        {
            await Cysharp.Threading.Tasks.UniTask.Yield();
        }
        
        public async Cysharp.Threading.Tasks.UniTask HideAsync(CancellationToken cancellationToken = default)
        {
            await Cysharp.Threading.Tasks.UniTask.Yield();
        }
        #endif
    }
    
    public class TestController : UIControllerBase
    {
        public bool IsInitialized { get; private set; }
        
        protected override void OnInitialize()
        {
            IsInitialized = true;
        }
    }
    
    public class TestUIData : UIDataBase
    {
        public int Value { get; set; }
        
        public TestUIData WithValue(int newValue)
        {
            return new TestUIData { Value = newValue };
        }
    }
}
