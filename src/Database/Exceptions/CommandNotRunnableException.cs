using System;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// Exception thrown when a non-runnable command tries to run.
    /// </summary>
    public class CommandNotRunnableException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandNotRunnableException"/> class.
        /// </summary>
        /// <param name="command">The command that is not runnable.</param>
        public CommandNotRunnableException(Command command)
            : base($"The command {command.Name} is not runnable.")
        {
            Command = command;
        }

        /// <summary>
        /// Gets or sets the non-runnable command.
        /// </summary>
        public Command Command { get; set; }
    }
}
