using System;
using System.Windows.Input;

namespace ChannelsMixer.Utils.Commands
{
    /// <summary>
    /// Relay command
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> executed;
        private readonly Func<object, bool> canExecute;

        /// <summary>
        /// Relay command constructor
        /// </summary>
        /// <param name="execute">Action with object parameter</param>
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {

        }


        /// <summary>
        /// Relay command constructor
        /// </summary>
        /// <param name="execute">Action with object parameter</param>
        /// <param name="canExecute">Can execute or not</param>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute), "Execute cannot be null.");
            }

            this.executed = execute;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Can execute changed event handler
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        void ICommand.Execute(object parameter)
        {
            this.executed(parameter);
        }

        bool ICommand.CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }
    }
}
