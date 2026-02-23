using System;
using System.Collections.Generic;
using UnityEngine;
using UIFramework.Core;

namespace UIFramework.Managers
{
    /// <summary>
    /// UI Navigation Manager - handles screen transitions and history
    /// </summary>
    public class UINavigationManager
    {
        private readonly Stack<NavigationState> navigationStack = new Stack<NavigationState>();
        private readonly UIManager uiManager;
        
        public int StackDepth => navigationStack.Count;
        public NavigationState CurrentState => navigationStack.Count > 0 ? navigationStack.Peek() : null;
        
        public UINavigationManager(UIManager uiManager)
        {
            this.uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
        }
        
        /// <summary>
        /// Navigate to a new screen
        /// </summary>
        public void NavigateTo(string screenId, IUIData data = null, bool hideOthers = true)
        {
            var state = new NavigationState
            {
                ScreenId = screenId,
                Data = data,
                Timestamp = DateTime.UtcNow
            };
            
            if (hideOthers && navigationStack.Count > 0)
            {
                var previous = navigationStack.Peek();
                uiManager.Hide(previous.ScreenId);
            }
            
            navigationStack.Push(state);
            uiManager.Show(screenId, data);
        }
        
        /// <summary>
        /// Navigate back to previous screen
        /// </summary>
        public bool NavigateBack()
        {
            if (navigationStack.Count <= 1)
            {
                Debug.LogWarning("[UINavigationManager] Cannot go back, at root screen");
                return false;
            }
            
            var current = navigationStack.Pop();
            uiManager.Hide(current.ScreenId);
            
            var previous = navigationStack.Peek();
            uiManager.Show(previous.ScreenId, previous.Data);
            
            return true;
        }
        
        /// <summary>
        /// Navigate to root screen
        /// </summary>
        public void NavigateToRoot()
        {
            while (navigationStack.Count > 1)
            {
                var current = navigationStack.Pop();
                uiManager.Hide(current.ScreenId);
            }
            
            if (navigationStack.Count > 0)
            {
                var root = navigationStack.Peek();
                uiManager.Show(root.ScreenId, root.Data);
            }
        }
        
        /// <summary>
        /// Clear navigation history
        /// </summary>
        public void Clear()
        {
            while (navigationStack.Count > 0)
            {
                var state = navigationStack.Pop();
                uiManager.Hide(state.ScreenId);
            }
        }
        
        /// <summary>
        /// Get navigation history
        /// </summary>
        public List<string> GetHistory()
        {
            var history = new List<string>();
            foreach (var state in navigationStack)
            {
                history.Add(state.ScreenId);
            }
            history.Reverse();
            return history;
        }
    }
    
    /// <summary>
    /// Represents a navigation state in history
    /// </summary>
    [Serializable]
    public class NavigationState
    {
        public string ScreenId;
        public IUIData Data;
        public DateTime Timestamp;
    }
}
