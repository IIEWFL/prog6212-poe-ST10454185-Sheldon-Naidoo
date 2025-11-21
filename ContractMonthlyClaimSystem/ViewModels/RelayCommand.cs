using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ContractMonthlyClaimSystem.ViewModels
{
    public class RelayCommand : ICommand
    {
        // Code Attribution 
        // This method was adapted from CodeProject
        // https://www.codeproject.com/articles/ICommand-Interface-in-WPF
        // Snesh Prajapati
        // https://www.codeproject.com/search?editorId=9373265

        // Action<object> delegate points to the method in the ViewModel.
        private readonly Action<object> _execute;

        // Func<object, bool> delegate returns true if the command is enabled, otherwise false.
        private readonly Func<object, bool> _canExecute;

        // Part 2: NEW Code
        private readonly Func<object, Task> _executeAsync;

        public event EventHandler CanExecuteChanged
        {
            // The add accessor subscribes to CommandManager.RequerySuggested
            // This event is raised automatically by WPF in response to various user actions
            add { CommandManager.RequerySuggested += value; }
            // The remove accessor unsubscribes the event handler.
            remove { CommandManager.RequerySuggested -= value; }
        }

        // The constructor initializes the command with the execute action and an optional canExecute function.
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            // Code Attribution
            // This method was adapted from Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/relaycommand
            // Microsoft Learn

            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _executeAsync = null;
            _canExecute = canExecute;
        }

        // Part 3: NEW Code - Constructor for asynchronous commands (Task)
        public RelayCommand(Func<object, Task> executeAsync, Func<object, bool> canExecute = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _execute = null;
            _canExecute = canExecute;
        }

        // This method determines whether the command can be executed.
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        // Part 2: UPDATED Code
        // This method is called when the command is invoked
        public async void Execute(object parameter)
        {
            if (_execute != null)
            {
                _execute(parameter);
            }
            else if (_executeAsync != null)
            {
                await _executeAsync(parameter);
            }
        }

        // Part 2: NEW Code
        // Public method to force re-evaluation of CanExecute
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}