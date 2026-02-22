// PlayMode Integration Tests
// Requires: Unity Test Framework package
// Place in Assets/Tests/PlayMode/

/*
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Luzart.UIFramework;
using Luzart.UIFramework.Examples;

namespace Luzart.UIFramework.Tests.PlayMode
{
    public class UIManagerIntegrationTests
    {
        private GameObject uiRootGO;
        private UIManager uiManager;
        private UIRegistry registry;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Create UI root
            uiRootGO = new GameObject("TestUIRoot");
            var canvas = uiRootGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Create registry
            registry = ScriptableObject.CreateInstance<UIRegistry>();
            
            // Create UI Manager
            uiManager = uiRootGO.AddComponent<UIManager>();
            
            // Assign registry via reflection
            var registryField = typeof(UIManager).GetField("registry", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            registryField?.SetValue(uiManager, registry);

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (uiManager != null)
            {
                uiManager.HideAll();
            }

            if (uiRootGO != null)
            {
                Object.Destroy(uiRootGO);
            }

            if (registry != null)
            {
                Object.Destroy(registry);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator UIManager_ShowAsync_ShouldLoadAndShowUI()
        {
            // Arrange
            var testScreenPrefab = CreateTestScreenPrefab();
            AddToRegistry("TestScreen", testScreenPrefab, UILayer.Screen);

            // Act
            var showTask = uiManager.ShowAsync<TestScreen>();
            yield return new WaitUntil(() => showTask.IsCompleted);

            // Assert
            Assert.IsTrue(uiManager.IsOpened<TestScreen>());
            Assert.IsNotNull(showTask.Result);
            Assert.AreEqual(UIState.Visible, showTask.Result.State);

            // Cleanup
            Object.Destroy(testScreenPrefab);
        }

        [UnityTest]
        public IEnumerator UIManager_Hide_ShouldCloseUI()
        {
            // Arrange
            var testScreenPrefab = CreateTestScreenPrefab();
            AddToRegistry("TestScreen", testScreenPrefab, UILayer.Screen);
            
            var showTask = uiManager.ShowAsync<TestScreen>();
            yield return new WaitUntil(() => showTask.IsCompleted);

            // Act
            uiManager.Hide<TestScreen>();
            yield return new WaitForSeconds(0.5f);

            // Assert
            Assert.IsFalse(uiManager.IsOpened<TestScreen>());

            // Cleanup
            Object.Destroy(testScreenPrefab);
        }

        [UnityTest]
        public IEnumerator UIManager_ShowSameUITwice_ShouldReturnExisting()
        {
            // Arrange
            var testScreenPrefab = CreateTestScreenPrefab();
            AddToRegistry("TestScreen", testScreenPrefab, UILayer.Screen);

            // Act
            var task1 = uiManager.ShowAsync<TestScreen>();
            yield return new WaitUntil(() => task1.IsCompleted);

            var task2 = uiManager.ShowAsync<TestScreen>();
            yield return new WaitUntil(() => task2.IsCompleted);

            // Assert
            Assert.AreEqual(task1.Result, task2.Result, "Should return same instance");

            // Cleanup
            Object.Destroy(testScreenPrefab);
        }

        [UnityTest]
        public IEnumerator UIManager_HideAll_ShouldCloseAllUIs()
        {
            // Arrange
            var screen1Prefab = CreateTestScreenPrefab();
            var popup1Prefab = CreateTestPopupPrefab();
            
            AddToRegistry("TestScreen", screen1Prefab, UILayer.Screen);
            AddToRegistry("TestPopup", popup1Prefab, UILayer.Popup);

            var task1 = uiManager.ShowAsync<TestScreen>();
            var task2 = uiManager.ShowAsync<TestPopup>();
            
            yield return new WaitUntil(() => task1.IsCompleted && task2.IsCompleted);

            // Act
            uiManager.HideAll();
            yield return new WaitForSeconds(0.5f);

            // Assert
            Assert.IsFalse(uiManager.IsOpened<TestScreen>());
            Assert.IsFalse(uiManager.IsOpened<TestPopup>());

            // Cleanup
            Object.Destroy(screen1Prefab);
            Object.Destroy(popup1Prefab);
        }

        [UnityTest]
        public IEnumerator UIManager_GoBack_ShouldNavigateToPreviousScreen()
        {
            // Arrange
            var screen1Prefab = CreateTestScreenPrefab();
            var screen2Prefab = CreateTestScreen2Prefab();
            
            AddToRegistry("TestScreen", screen1Prefab, UILayer.Screen);
            AddToRegistry("TestScreen2", screen2Prefab, UILayer.Screen);

            var task1 = uiManager.ShowAsync<TestScreen>();
            yield return new WaitUntil(() => task1.IsCompleted);

            var task2 = uiManager.ShowAsync<TestScreen2>();
            yield return new WaitUntil(() => task2.IsCompleted);

            // Act
            uiManager.GoBack();
            yield return new WaitForSeconds(0.5f);

            // Assert
            Assert.IsFalse(uiManager.IsOpened<TestScreen2>());
            Assert.IsTrue(uiManager.IsOpened<TestScreen>());

            // Cleanup
            Object.Destroy(screen1Prefab);
            Object.Destroy(screen2Prefab);
        }

        // Helper methods
        private GameObject CreateTestScreenPrefab()
        {
            var go = new GameObject("TestScreen");
            go.AddComponent<CanvasGroup>();
            var screen = go.AddComponent<TestScreen>();
            return go;
        }

        private GameObject CreateTestScreen2Prefab()
        {
            var go = new GameObject("TestScreen2");
            go.AddComponent<CanvasGroup>();
            var screen = go.AddComponent<TestScreen2>();
            return go;
        }

        private GameObject CreateTestPopupPrefab()
        {
            var go = new GameObject("TestPopup");
            go.AddComponent<CanvasGroup>();
            var popup = go.AddComponent<TestPopup>();
            return go;
        }

        private void AddToRegistry(string viewId, GameObject prefab, UILayer layer)
        {
            var entry = new UIRegistryEntry
            {
                viewId = viewId,
                layer = layer,
                loadMode = UILoadMode.Direct,
                prefab = prefab,
                transitionType = UITransitionType.None,
                enablePooling = false
            };

            registry.AddEntry(entry);
        }
    }

    // Test UI classes
    public class TestScreen : UIScreen { }
    public class TestScreen2 : UIScreen { }
    public class TestPopup : UIPopup { }
}
*/
