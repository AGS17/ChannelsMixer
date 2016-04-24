using System;
using System.Windows.Input;

namespace ChannelsMixer.Utils.Commands
{
    /// <summary>
    /// Base simple command abstract
    /// </summary>
    public abstract class BaseSimpleCommand : ICommand
    {
        private readonly Func<bool> canExecute;

        /// <summary>
        /// Base simple command constructor
        /// </summary>
        protected BaseSimpleCommand(Func<bool> canExecute)
        {
            if (canExecute == null)
            {
                throw new ArgumentException("canExecute must be set");
            }

            this.canExecute = canExecute;
        }

        /// <summary>
        /// Can execute the simple command
        /// </summary>
        /// <param name="parameter">Parameter object</param>
        /// <returns>Can execute or not</returns>
        public bool CanExecute(object parameter)
        {
            return this.canExecute();
        }

        /// <summary>
        /// Execute command
        /// </summary>
        /// <param name="parameter">Parameter object</param>
        public abstract void Execute(object parameter);

        /// <summary>
        /// Can execute changed event handler
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Can execute changed event
        /// </summary>
        public void OnCanExecuteChanged()
        {
            EventHandler handler = this.CanExecuteChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}