using System;

namespace UIFramework.Utils
{
    /// <summary>
    /// Service locator pattern for dependency injection
    /// Alternative to singleton for better testability
    /// </summary>
    public class ServiceLocator
    {
        private static ServiceLocator instance;
        public static ServiceLocator Instance => instance ?? (instance = new ServiceLocator());
        
        private readonly System.Collections.Generic.Dictionary<Type, object> services = 
            new System.Collections.Generic.Dictionary<Type, object>();
        
        /// <summary>
        /// Register a service
        /// </summary>
        public void Register<T>(T service)
        {
            var type = typeof(T);
            
            if (services.ContainsKey(type))
            {
                UnityEngine.Debug.LogWarning($"[ServiceLocator] Service {type.Name} already registered, replacing");
            }
            
            services[type] = service;
        }
        
        /// <summary>
        /// Get a service
        /// </summary>
        public T Get<T>()
        {
            var type = typeof(T);
            
            if (services.TryGetValue(type, out var service))
            {
                return (T)service;
            }
            
            UnityEngine.Debug.LogError($"[ServiceLocator] Service {type.Name} not found");
            return default;
        }
        
        /// <summary>
        /// Check if service is registered
        /// </summary>
        public bool Has<T>()
        {
            return services.ContainsKey(typeof(T));
        }
        
        /// <summary>
        /// Unregister a service
        /// </summary>
        public void Unregister<T>()
        {
            var type = typeof(T);
            services.Remove(type);
        }
        
        /// <summary>
        /// Clear all services
        /// </summary>
        public void Clear()
        {
            services.Clear();
        }
        
        /// <summary>
        /// Reset instance (for testing)
        /// </summary>
        public static void Reset()
        {
            instance?.Clear();
            instance = null;
        }
    }
}
