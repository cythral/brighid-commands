using System;

namespace Brighid.Commands.Commands
{
    /// <summary>
    /// Request to create or update a command.
    /// </summary>
    public class CommandRequest : Command
    {
        /// <summary>
        /// Gets or sets the command's ID.
        /// </summary>
        protected new Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the owner's ID.
        /// </summary>
        protected new Guid OwnerId { get; set; } = Guid.NewGuid();
    }
}
