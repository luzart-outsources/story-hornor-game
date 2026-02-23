using System;

namespace UIFramework.Core
{
    /// <summary>
    /// Base class for immutable UI data (ViewModel)
    /// </summary>
    [Serializable]
    public abstract class UIDataBase : IUIData
    {
        // Implement specific data properties in derived classes
        // Keep data immutable or use controlled mutation
    }
}
