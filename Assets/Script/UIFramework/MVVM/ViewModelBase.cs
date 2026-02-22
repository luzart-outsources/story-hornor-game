using System;

namespace UIFramework.MVVM
{
    public interface IViewModel
    {
        event Action OnDataChanged;
        void NotifyDataChanged();
    }

    public abstract class ViewModelBase : IViewModel
    {
        public event Action OnDataChanged;

        protected void NotifyPropertyChanged()
        {
            OnDataChanged?.Invoke();
        }

        public void NotifyDataChanged()
        {
            OnDataChanged?.Invoke();
        }
    }
}
