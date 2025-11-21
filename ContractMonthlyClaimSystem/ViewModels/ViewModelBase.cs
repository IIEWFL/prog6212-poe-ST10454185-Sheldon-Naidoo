using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace ContractMonthlyClaimSystem.ViewModels 
{
    // A helper method to raise the PropertyChanged and RequestClose event.
    public class ViewModelBase : INotifyPropertyChanged
    {
        // Code Attribution
        // This method was adapted from Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/how-to-implement-property-change-notification
        // Microsoft Learn

        // Event for INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // Part 2
        // Event for Navigation/Window Management
        public event EventHandler RequestClose;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Helper method to set a property value and notify if the value has changed.
        // This method minimizes boilerplate code (e.g., if (field != value) { field = value; OnPropertyChanged(); })
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            // Check if the new value is the same as the current value
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            // Update the field
            field = value;

            // Notify the UI
            OnPropertyChanged(propertyName);
            return true;
        }

        // Part 2
        // Method to signal the View to close the current window
        protected void CloseView()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
