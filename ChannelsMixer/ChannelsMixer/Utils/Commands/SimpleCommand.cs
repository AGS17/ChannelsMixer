using System;

namespace ChannelsMixer.Utils.Commands
{
    /// <summary>
    /// Simple command
    /// </summary>
    public class SimpleCommand : BaseSimpleCommand
    {
        private readonly Action executed;

        /// <summary>
        /// Simple command constructor
        /// </summary>
        /// <param name="executed">Action</param>
        /// <param name="canExecute">Can execute or not</param>
        public SimpleCommand(Action executed, Func<bool> canExecute) : base(canExecute)
        {
            if (executed == null)
            {
                throw new ArgumentException("executed must be set");
            }

            this.executed = executed;
        }

        /// <summary>
        /// Simple command constructor
        /// </summary>
        /// <param name="executed">Action</param>
        public SimpleCommand(Action executed)
            : this(executed, () => true)
        {
        }

        /// <summary>
        /// Execute command
        /// </summary>
        /// <param name="parameter">Parameter object</param>
        public override void Execute(object parameter)
        {
            this.executed();
        }
    }
}
