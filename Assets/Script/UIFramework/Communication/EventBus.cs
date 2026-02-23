using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework.Communication
{
    /// <summary>
    /// Event Bus for decoupled communication between UI elements
    /// Thread-safe, weak-reference based to prevent memory leaks
    /// </summary>
    public class EventBus
    {
        private static EventBus instance;
        public static EventBus Instance => instance ?? (instance = new EventBus());
        
        private readonly Dictionary<Type, List<WeakReference>> subscribers = new Dictionary<Type, List<WeakReference>>();
        private readonly object lockObject = new object();
        
        /// <summary>
        /// Subscribe to an event
        /// </summary>
        public void Subscribe<T>(IEventHandler<T> handler) where T : IUIEvent
        {
            lock (lockObject)
            {
                var eventType = typeof(T);
                
                if (!subscribers.ContainsKey(eventType))
                {
                    subscribers[eventType] = new List<WeakReference>();
                }
                
                // Check if already subscribed
                CleanupDeadReferences(eventType);
                
                foreach (var weakRef in subscribers[eventType])
                {
                    if (weakRef.IsAlive && weakRef.Target == handler)
                    {
                        return; // Already subscribed
                    }
                }
                
                subscribers[eventType].Add(new WeakReference(handler));
            }
        }
        
        /// <summary>
        /// Unsubscribe from an event
        /// </summary>
        public void Unsubscribe<T>(IEventHandler<T> handler) where T : IUIEvent
        {
            lock (lockObject)
            {
                var eventType = typeof(T);
                
                if (!subscribers.ContainsKey(eventType))
                    return;
                
                subscribers[eventType].RemoveAll(weakRef => 
                    !weakRef.IsAlive || weakRef.Target == handler);
                    
                if (subscribers[eventType].Count == 0)
                {
                    subscribers.Remove(eventType);
                }
            }
        }
        
        /// <summary>
        /// Publish an event to all subscribers
        /// </summary>
        public void Publish<T>(T eventData) where T : IUIEvent
        {
            List<IEventHandler<T>> handlersToNotify = new List<IEventHandler<T>>();
            
            lock (lockObject)
            {
                var eventType = typeof(T);
                
                if (!subscribers.ContainsKey(eventType))
                    return;
                
                CleanupDeadReferences(eventType);
                
                foreach (var weakRef in subscribers[eventType])
                {
                    if (weakRef.IsAlive && weakRef.Target is IEventHandler<T> handler)
                    {
                        handlersToNotify.Add(handler);
                    }
                }
            }
            
            // Notify outside the lock to prevent deadlocks
            foreach (var handler in handlersToNotify)
            {
                try
                {
                    handler.Handle(eventData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[EventBus] Error handling event {typeof(T).Name}: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// Clear all subscribers
        /// </summary>
        public void Clear()
        {
            lock (lockObject)
            {
                subscribers.Clear();
            }
        }
        
        private void CleanupDeadReferences(Type eventType)
        {
            if (!subscribers.ContainsKey(eventType))
                return;
                
            subscribers[eventType].RemoveAll(weakRef => !weakRef.IsAlive);
            
            if (subscribers[eventType].Count == 0)
            {
                subscribers.Remove(eventType);
            }
        }
        
        /// <summary>
        /// Periodic cleanup of all dead references
        /// Call this occasionally to keep memory clean
        /// </summary>
        public void CleanupAll()
        {
            lock (lockObject)
            {
                var typesToRemove = new List<Type>();
                
                foreach (var kvp in subscribers)
                {
                    kvp.Value.RemoveAll(weakRef => !weakRef.IsAlive);
                    
                    if (kvp.Value.Count == 0)
                    {
                        typesToRemove.Add(kvp.Key);
                    }
                }
                
                foreach (var type in typesToRemove)
                {
                    subscribers.Remove(type);
                }
            }
        }
    }
    
    /// <summary>
    /// Base interface for UI events
    /// </summary>
    public interface IUIEvent { }
    
    /// <summary>
    /// Interface for event handlers
    /// </summary>
    public interface IEventHandler<T> where T : IUIEvent
    {
        void Handle(T eventData);
    }
}
