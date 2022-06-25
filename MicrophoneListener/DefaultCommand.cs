using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MicrophoneListener
{
    public class DefaultCommand : ICommand
    {
        public DefaultCommand(Action<object> action)
        {
            Action = action;
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(null, null);
        }

        public Action<object> Action { get; set; }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Action.Invoke(parameter);
        }
    }
}