using System;
using System.ComponentModel;

namespace GG.ViewModels
{
    /// <summary>
    /// A base view model that implements INotifyPropertyChanged.
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            // Take a copy to prevent thread issues.
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
