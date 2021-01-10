#nullable enable
using System;
using System.Windows.Input;

namespace NonDomainEntities
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public bool CanExecute(object? parameter)
        {
            return _canExecute();
        }

        public void Execute(object? parameter)
        {
            _execute();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }

        public event EventHandler? CanExecuteChanged;
        
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _canExecute = canExecute ?? (() => true);
            _execute = execute;
        }
    }
}