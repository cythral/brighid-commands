using System;

namespace Brighid.Commands.Commands
{
    /// <summary>
    /// Exception that is thrown when a command type is not supported.
    /// </summary>
    public class CommandTypeNotSupportedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandTypeNotSupportedException"/> class.
        /// </summary>
        /// <param name="commandType">The type of command not supported.</param>
        public CommandTypeNotSupportedException(CommandType commandType)
            : base($"Command Type {commandType} is not supported.")
        {
        }
    }
}
