using System;

namespace Brighid.Commands.Commands
{
    /// <summary>
    /// Exception that is thrown when a command requires a role that the requesting user does not have.
    /// </summary>
    public class CommandRequiresRoleException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRequiresRoleException"/> class.
        /// </summary>
        /// <param name="command">Command that requires a role the user is missing.</param>
        public CommandRequiresRoleException(Command command)
            : base($"Command {command.Name} requires role: {command.RequiredRole}")
        {
            RequiredRole = command.RequiredRole ?? string.Empty;
        }

        /// <summary>
        /// Gets the role that was required for the given command but the user is missing.
        /// </summary>
        public string RequiredRole { get; }
    }
}
