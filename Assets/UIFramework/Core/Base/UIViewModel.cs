using System;

namespace Luzart.UIFramework
{
    public abstract class UIViewModel : IUIViewModel
    {
        public event Action OnDataChanged;

        protected void NotifyDataChanged()
        {
            OnDataChanged?.Invoke();
        }

        public virtual void Reset()
        {
            OnDataChanged = null;
        }
    }
}
