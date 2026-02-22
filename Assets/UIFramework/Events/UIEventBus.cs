using System;
using System.Collections.Generic;
using UnityEngine;

namespace Luzart.UIFramework
{
    public class UIEventBus
    {
        private readonly Dictionary<Type, List<WeakReference>> subscriptions = new Dictionary<Type, List<WeakReference>>();
        private readonly List<WeakReference> tempList = new List<WeakReference>(16);

        public UIEventSubscription Subscribe<TEvent>(Action<TEvent> handler) where TEvent : UIEvent
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var eventType = typeof(TEvent);
            
            if (!subscriptions.TryGetValue(eventType, out var handlers))
            {
                handlers = new List<WeakReference>();
                subscriptions[eventType] = handlers;
            }

            var weakHandler = new WeakReference(handler);
            handlers.Add(weakHandler);

            return new UIEventSubscription(this, eventType, weakHandler);
        }

        public void Unsubscribe(Type eventType, WeakReference weakHandler)
        {
            if (subscriptions.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(weakHandler);
            }
        }

        public void Publish<TEvent>(TEvent eventData) where TEvent : UIEvent
        {
            var eventType = typeof(TEvent);
            
            if (!subscriptions.TryGetValue(eventType, out var handlers))
                return;

            tempList.Clear();
            tempList.AddRange(handlers);

            for (int i = tempList.Count - 1; i >= 0; i--)
            {
                var weakRef = tempList[i];
                
                if (!weakRef.IsAlive)
                {
                    handlers.Remove(weakRef);
                    continue;
                }

                if (weakRef.Target is Action<TEvent> handler)
                {
                    try
                    {
                        handler.Invoke(eventData);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"Error invoking event handler for {eventType.Name}: {ex}");
                    }
                }
            }

            tempList.Clear();
        }

        public void Clear()
        {
            subscriptions.Clear();
            tempList.Clear();
        }

        public void CleanupDeadReferences()
        {
            foreach (var kvp in subscriptions)
            {
                kvp.Value.RemoveAll(weak => !weak.IsAlive);
            }
        }
    }

    public class UIEventSubscription : IDisposable
    {
        private UIEventBus eventBus;
        private Type eventType;
        private WeakReference weakHandler;

        public UIEventSubscription(UIEventBus eventBus, Type eventType, WeakReference weakHandler)
        {
            this.eventBus = eventBus;
            this.eventType = eventType;
            this.weakHandler = weakHandler;
        }

        public void Dispose()
        {
            if (eventBus != null)
            {
                eventBus.Unsubscribe(eventType, weakHandler);
                eventBus = null;
                eventType = null;
                weakHandler = null;
            }
        }
    }
}
