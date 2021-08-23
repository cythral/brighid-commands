using System;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// Exception that is thrown when duplicate command parameters were given in a request.
    /// </summary>
    public class DuplicateArgumentIndexException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateArgumentIndexException" /> class.
        /// </summary>
        /// <param name="commandName">The name of the command that had duplicate parameters.</param>
        /// <param name="argumentIndex">The argument index that was found multiple times.</param>
        public DuplicateArgumentIndexException(string commandName, byte argumentIndex)
            : base($"Command {commandName} had multiple parameters with the same argumentIndex: {argumentIndex}")
        {
            CommandName = commandName;
            ArgumentIndex = argumentIndex;
        }

        /// <summary>
        /// Gets the name of the command that was not found.
        /// </summary>
        public string CommandName { get; }

        /// <summary>
        /// Gets the duplicated argument index.
        /// </summary>
        public byte ArgumentIndex { get; }
    }
}
