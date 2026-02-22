using System;
using System.Collections.Generic;

namespace UIFramework.Events
{
    public class EventBus
    {
        private static EventBus _instance;
        public static EventBus Instance => _instance ??= new EventBus();

        private readonly Dictionary<Type, List<WeakReference>> _subscribers = new Dictionary<Type, List<WeakReference>>();
        private readonly object _lock = new object();

        private EventBus() { }

        public void Subscribe<T>(Action<T> handler) where T : IUIEvent
        {
            if (handler == null)
                return;

            lock (_lock)
            {
                var eventType = typeof(T);
                if (!_subscribers.ContainsKey(eventType))
                {
                    _subscribers[eventType] = new List<WeakReference>();
                }

                _subscribers[eventType].Add(new WeakReference(handler));
            }
        }

        public void Unsubscribe<T>(Action<T> handler) where T : IUIEvent
        {
            if (handler == null)
                return;

            lock (_lock)
            {
                var eventType = typeof(T);
                if (!_subscribers.ContainsKey(eventType))
                    return;

                var list = _subscribers[eventType];
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (!list[i].IsAlive || list[i].Target.Equals(handler))
                    {
                        list.RemoveAt(i);
                    }
                }

                if (list.Count == 0)
                {
                    _subscribers.Remove(eventType);
                }
            }
        }

        public void Publish<T>(T eventData) where T : IUIEvent
        {
            lock (_lock)
            {
                var eventType = typeof(T);
                if (!_subscribers.ContainsKey(eventType))
                    return;

                var list = _subscribers[eventType];
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var weakRef = list[i];
                    if (!weakRef.IsAlive)
                    {
                        list.RemoveAt(i);
                        continue;
                    }

                    var handler = weakRef.Target as Action<T>;
                    try
                    {
                        handler?.Invoke(eventData);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"EventBus error: {ex.Message}");
                    }
                }

                if (list.Count == 0)
                {
                    _subscribers.Remove(eventType);
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _subscribers.Clear();
            }
        }

        public void ClearDeadReferences()
        {
            lock (_lock)
            {
                var keysToRemove = new List<Type>();
                foreach (var kvp in _subscribers)
                {
                    var list = kvp.Value;
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        if (!list[i].IsAlive)
                        {
                            list.RemoveAt(i);
                        }
                    }

                    if (list.Count == 0)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }

                foreach (var key in keysToRemove)
                {
                    _subscribers.Remove(key);
                }
            }
        }
    }

    public interface IUIEvent { }

    public class UIOpenedEvent : IUIEvent
    {
        public Type UIType { get; set; }
        public object Data { get; set; }
    }

    public class UIClosedEvent : IUIEvent
    {
        public Type UIType { get; set; }
    }

    public class UIScreenChangedEvent : IUIEvent
    {
        public Type FromScreen { get; set; }
        public Type ToScreen { get; set; }
    }

    public class UIPopupStackChangedEvent : IUIEvent
    {
        public int StackCount { get; set; }
    }
}
