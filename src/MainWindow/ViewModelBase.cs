using System;
using System.ComponentModel;

namespace WinCompose.Gui
{
    /// <summary>
    /// A minimal implementation of the <see cref="INotifyPropertyChanged"/> interface
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void SetValue<T>(ref T field, T value, string propertyName, Action<T> callback = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                if (callback != null)
                    callback(value);
                OnPropertyChanged(propertyName);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
