namespace Luzart
{
    using System;
    using UnityEngine;

    [CreateAssetMenu(fileName = "NewGameEvent", menuName = "Luzart/Events/Game Event")]
    public class GameEventChannel : ScriptableObject
    {
        private Action listeners;

        public void Register(Action callback)
        {
            listeners += callback;
        }

        public void Unregister(Action callback)
        {
            listeners -= callback;
        }

        public void Raise()
        {
            listeners?.Invoke();
        }

        private void OnDisable()
        {
            listeners = null;
        }
    }

    public class GameEventChannel<T> : ScriptableObject
    {
        private Action<T> listeners;

        public void Register(Action<T> callback)
        {
            listeners += callback;
        }

        public void Unregister(Action<T> callback)
        {
            listeners -= callback;
        }

        public void Raise(T data)
        {
            listeners?.Invoke(data);
        }

        private void OnDisable()
        {
            listeners = null;
        }
    }

    [CreateAssetMenu(fileName = "NewStringEvent", menuName = "Luzart/Events/String Event")]
    public class StringEventChannel : GameEventChannel<string> { }
}
