using System;
using System.Collections.Generic;
using UnityEngine;

namespace Luzart.UIFramework
{
    public class UIContext
    {
        private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();
        private readonly Dictionary<Type, IUIController> controllers = new Dictionary<Type, IUIController>();

        public void RegisterService<T>(T service)
        {
            var type = typeof(T);
            services[type] = service;
        }

        public T GetService<T>()
        {
            var type = typeof(T);
            return services.TryGetValue(type, out var service) ? (T)service : default;
        }

        public void RegisterController<T>(IUIController controller) where T : UIBase
        {
            var type = typeof(T);
            controllers[type] = controller;
        }

        public IUIController GetController<T>() where T : UIBase
        {
            var type = typeof(T);
            return controllers.TryGetValue(type, out var controller) ? controller : null;
        }

        public void Clear()
        {
            foreach (var controller in controllers.Values)
            {
                controller?.Dispose();
            }
            controllers.Clear();
            services.Clear();
        }
    }
}
