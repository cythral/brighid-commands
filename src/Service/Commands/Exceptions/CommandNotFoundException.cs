using System;

namespace Brighid.Commands.Commands
{
    /// <summary>
    /// Exception that is thrown when a command cannot be found in an assembly.
    /// </summary>
    public class CommandNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandNotFoundException" /> class.
        /// </summary>
        /// <param name="commandName">The name of the command that was not found..</param>
        public CommandNotFoundException(string commandName)
            : base($"Command {commandName} was not found.")
        {
            CommandName = commandName;
        }

        /// <summary>
        /// Gets the name of the command that was not found.
        /// </summary>
        public string CommandName { get; }
    }
}
