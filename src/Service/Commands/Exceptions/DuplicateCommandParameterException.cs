using System;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// Exception that is thrown when duplicate command parameters were given in a request.
    /// </summary>
    public class DuplicateCommandParameterException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateCommandParameterException" /> class.
        /// </summary>
        /// <param name="commandName">The name of the command that had duplicate parameters.</param>
        /// <param name="parameterName">The name of the parameter that was found multiple times.</param>
        public DuplicateCommandParameterException(string commandName, string parameterName)
            : base($"Command {commandName} had multiple parameters with the same name: {parameterName}")
        {
            CommandName = commandName;
            ParameterName = parameterName;
        }

        /// <summary>
        /// Gets the name of the command that was not found.
        /// </summary>
        public string CommandName { get; }

        /// <summary>
        /// Gets the name of the duplicated parameter.
        /// </summary>
        public string ParameterName { get; }
    }
}
