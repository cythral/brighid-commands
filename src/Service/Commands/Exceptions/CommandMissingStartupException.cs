using System;

namespace Brighid.Commands.Commands
{
    /// <summary>
    /// Exception thrown when a command class is missing it a <see cref="Core.CommandStartupAttribute" />.
    /// </summary>
    public class CommandMissingStartupException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandMissingStartupException" /> class.
        /// </summary>
        /// <param name="commandName">The name of the command missing a startup class attribute.</param>
        public CommandMissingStartupException(string commandName)
            : base($"Attempted to load command {commandName}, but it did not have a startup type registered.")
        {
        }
    }
}
