// Unit Tests for UI Framework
// Requires: Unity Test Framework package
// Place in Assets/Tests/EditMode/

/*
using NUnit.Framework;
using Luzart.UIFramework;
using Luzart.UIFramework.Examples;

namespace Luzart.UIFramework.Tests
{
    [TestFixture]
    public class UIViewModelTests
    {
        [Test]
        public void ViewModel_WhenPropertyChanged_ShouldNotify()
        {
            // Arrange
            var viewModel = new SettingsViewModel();
            bool notified = false;
            viewModel.OnDataChanged += () => notified = true;

            // Act
            viewModel.MusicVolume = 0.5f;

            // Assert
            Assert.IsTrue(notified, "ViewModel should notify on property change");
            Assert.AreEqual(0.5f, viewModel.MusicVolume, 0.01f);
        }

        [Test]
        public void ViewModel_WhenPropertySetToSameValue_ShouldNotNotify()
        {
            // Arrange
            var viewModel = new SettingsViewModel();
            viewModel.MusicVolume = 0.5f;
            
            int notifyCount = 0;
            viewModel.OnDataChanged += () => notifyCount++;

            // Act
            viewModel.MusicVolume = 0.5f;

            // Assert
            Assert.AreEqual(0, notifyCount, "ViewModel should not notify if value unchanged");
        }

        [Test]
        public void ViewModel_WhenReset_ShouldClearEventHandlers()
        {
            // Arrange
            var viewModel = new SettingsViewModel();
            bool notified = false;
            viewModel.OnDataChanged += () => notified = true;

            // Act
            viewModel.Reset();
            viewModel.MusicVolume = 0.8f;

            // Assert
            Assert.IsFalse(notified, "Event handlers should be cleared after Reset");
        }

        [Test]
        public void ViewModel_WhenVolumeSetOutOfRange_ShouldClamp()
        {
            // Arrange
            var viewModel = new SettingsViewModel();

            // Act & Assert - Upper bound
            viewModel.MusicVolume = 1.5f;
            Assert.AreEqual(1f, viewModel.MusicVolume, 0.01f);

            // Act & Assert - Lower bound
            viewModel.SfxVolume = -0.5f;
            Assert.AreEqual(0f, viewModel.SfxVolume, 0.01f);
        }

        [Test]
        public void MainMenuViewModel_WhenPlayerLevelChanged_ShouldNotify()
        {
            // Arrange
            var viewModel = new MainMenuViewModel();
            int notifyCount = 0;
            viewModel.OnDataChanged += () => notifyCount++;

            // Act
            viewModel.PlayerLevel = 10;
            viewModel.PlayerLevel = 20;

            // Assert
            Assert.AreEqual(2, notifyCount);
            Assert.AreEqual(20, viewModel.PlayerLevel);
        }
    }

    [TestFixture]
    public class UIEventBusTests
    {
        [Test]
        public void EventBus_WhenEventPublished_ShouldInvokeHandler()
        {
            // Arrange
            var eventBus = new UIEventBus();
            bool invoked = false;
            int receivedValue = 0;

            eventBus.Subscribe<TestEvent>(evt => 
            {
                invoked = true;
                receivedValue = evt.Value;
            });

            // Act
            eventBus.Publish(new TestEvent(42));

            // Assert
            Assert.IsTrue(invoked);
            Assert.AreEqual(42, receivedValue);
        }

        [Test]
        public void EventBus_WhenSubscriptionDisposed_ShouldNotInvoke()
        {
            // Arrange
            var eventBus = new UIEventBus();
            bool invoked = false;

            var subscription = eventBus.Subscribe<TestEvent>(evt => invoked = true);

            // Act
            subscription.Dispose();
            eventBus.Publish(new TestEvent(42));

            // Assert
            Assert.IsFalse(invoked, "Handler should not be invoked after disposal");
        }

        [Test]
        public void EventBus_WhenMultipleSubscribers_ShouldInvokeAll()
        {
            // Arrange
            var eventBus = new UIEventBus();
            int invokeCount = 0;

            eventBus.Subscribe<TestEvent>(evt => invokeCount++);
            eventBus.Subscribe<TestEvent>(evt => invokeCount++);
            eventBus.Subscribe<TestEvent>(evt => invokeCount++);

            // Act
            eventBus.Publish(new TestEvent(42));

            // Assert
            Assert.AreEqual(3, invokeCount);
        }

        [Test]
        public void EventBus_WhenHandlerThrowsException_ShouldContinueToOthers()
        {
            // Arrange
            var eventBus = new UIEventBus();
            bool secondHandlerInvoked = false;

            eventBus.Subscribe<TestEvent>(evt => throw new System.Exception("Test exception"));
            eventBus.Subscribe<TestEvent>(evt => secondHandlerInvoked = true);

            // Act
            eventBus.Publish(new TestEvent(42));

            // Assert
            Assert.IsTrue(secondHandlerInvoked, "Second handler should be invoked even if first throws");
        }

        [Test]
        public void EventBus_WhenClear_ShouldRemoveAllSubscriptions()
        {
            // Arrange
            var eventBus = new UIEventBus();
            bool invoked = false;

            eventBus.Subscribe<TestEvent>(evt => invoked = true);

            // Act
            eventBus.Clear();
            eventBus.Publish(new TestEvent(42));

            // Assert
            Assert.IsFalse(invoked, "No handlers should be invoked after Clear");
        }
    }

    [TestFixture]
    public class UIStackManagerTests
    {
        [Test]
        public void StackManager_WhenPushScreen_ShouldIncreaseCount()
        {
            // Arrange
            var stackManager = new UIStackManager();
            var mockScreen = new GameObject().AddComponent<UIScreen>();

            // Act
            stackManager.PushScreen(mockScreen);

            // Assert
            Assert.AreEqual(1, stackManager.ScreenStackCount);

            // Cleanup
            Object.DestroyImmediate(mockScreen.gameObject);
        }

        [Test]
        public void StackManager_WhenPopScreen_ShouldReturnLastPushed()
        {
            // Arrange
            var stackManager = new UIStackManager();
            var screen1 = new GameObject().AddComponent<UIScreen>();
            var screen2 = new GameObject().AddComponent<UIScreen>();

            stackManager.PushScreen(screen1);
            stackManager.PushScreen(screen2);

            // Act
            var popped = stackManager.PopScreen();

            // Assert
            Assert.AreEqual(screen2, popped);
            Assert.AreEqual(1, stackManager.ScreenStackCount);

            // Cleanup
            Object.DestroyImmediate(screen1.gameObject);
            Object.DestroyImmediate(screen2.gameObject);
        }

        [Test]
        public void StackManager_WhenClearAll_ShouldEmpty()
        {
            // Arrange
            var stackManager = new UIStackManager();
            var screen = new GameObject().AddComponent<UIScreen>();
            var popup = new GameObject().AddComponent<UIPopup>();

            stackManager.PushScreen(screen);
            stackManager.PushPopup(popup);

            // Act
            stackManager.ClearAll();

            // Assert
            Assert.AreEqual(0, stackManager.ScreenStackCount);
            Assert.AreEqual(0, stackManager.PopupStackCount);

            // Cleanup
            Object.DestroyImmediate(screen.gameObject);
            Object.DestroyImmediate(popup.gameObject);
        }
    }

    [TestFixture]
    public class UIContextTests
    {
        [Test]
        public void Context_WhenServiceRegistered_ShouldResolve()
        {
            // Arrange
            var context = new UIContext();
            var service = new TestService();

            // Act
            context.RegisterService<ITestService>(service);
            var resolved = context.GetService<ITestService>();

            // Assert
            Assert.AreEqual(service, resolved);
        }

        [Test]
        public void Context_WhenServiceNotRegistered_ShouldReturnDefault()
        {
            // Arrange
            var context = new UIContext();

            // Act
            var resolved = context.GetService<ITestService>();

            // Assert
            Assert.IsNull(resolved);
        }

        [Test]
        public void Context_WhenClear_ShouldDisposeControllers()
        {
            // Arrange
            var context = new UIContext();
            var mockController = new MockController();
            context.RegisterController<UIScreen>(mockController);

            // Act
            context.Clear();

            // Assert
            Assert.IsTrue(mockController.IsDisposed);
        }
    }

    // Test Helpers
    public class TestEvent : UIEvent
    {
        public int Value { get; }
        public TestEvent(int value) { Value = value; }
    }

    public interface ITestService { }
    public class TestService : ITestService { }

    public class MockController : IUIController
    {
        public bool IsDisposed { get; private set; }

        public void Initialize() { }
        public void OnViewShown() { }
        public void OnViewHidden() { }
        
        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
*/
