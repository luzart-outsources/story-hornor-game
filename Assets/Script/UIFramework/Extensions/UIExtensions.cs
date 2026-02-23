using System;

namespace UIFramework.Extensions
{
    /// <summary>
    /// Extension methods for IUIData
    /// </summary>
    public static class UIDataExtensions
    {
        /// <summary>
        /// Creates a deep copy of UI data
        /// </summary>
        public static T Clone<T>(this T data) where T : Core.IUIData, ICloneable
        {
            return (T)data.Clone();
        }
    }
    
    /// <summary>
    /// Extension methods for EventBus
    /// </summary>
    public static class EventBusExtensions
    {
        /// <summary>
        /// Subscribe with automatic unsubscribe on dispose
        /// </summary>
        public static IDisposable SubscribeWithDispose<T>(this Communication.EventBus eventBus, Communication.IEventHandler<T> handler) 
            where T : Communication.IUIEvent
        {
            eventBus.Subscribe(handler);
            return new EventSubscription<T>(eventBus, handler);
        }
        
        private class EventSubscription<T> : IDisposable where T : Communication.IUIEvent
        {
            private Communication.EventBus eventBus;
            private Communication.IEventHandler<T> handler;
            
            public EventSubscription(Communication.EventBus eventBus, Communication.IEventHandler<T> handler)
            {
                this.eventBus = eventBus;
                this.handler = handler;
            }
            
            public void Dispose()
            {
                eventBus?.Unsubscribe(handler);
                eventBus = null;
                handler = null;
            }
        }
    }
}
