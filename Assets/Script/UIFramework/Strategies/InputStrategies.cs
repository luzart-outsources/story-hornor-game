using UnityEngine;
using UIFramework.Core;

namespace UIFramework.Strategies
{
    /// <summary>
    /// Default input strategy - does nothing
    /// Override for custom input handling
    /// </summary>
    public class DefaultInputStrategy : IInputStrategy
    {
        public void Enable()
        {
            // Override in derived classes
        }
        
        public void Disable()
        {
            // Override in derived classes
        }
        
        public void ProcessInput()
        {
            // Override in derived classes
        }
    }
    
    /// <summary>
    /// Example: ESC key to close popup
    /// </summary>
    public class EscapeKeyInputStrategy : IInputStrategy
    {
        private readonly System.Action onEscapePressed;
        private bool isEnabled;
        
        public EscapeKeyInputStrategy(System.Action onEscapePressed)
        {
            this.onEscapePressed = onEscapePressed;
        }
        
        public void Enable()
        {
            isEnabled = true;
        }
        
        public void Disable()
        {
            isEnabled = false;
        }
        
        public void ProcessInput()
        {
            if (!isEnabled)
                return;
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                onEscapePressed?.Invoke();
            }
        }
    }
}
