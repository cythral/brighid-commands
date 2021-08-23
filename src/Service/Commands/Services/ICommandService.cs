using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Core;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// Service to manage commands with.
    /// </summary>
    public interface ICommandService
    {
        /// <summary>
        /// Get a list of all commands.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The list of commands.</returns>
        Task<IEnumerable<Command>> List(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a command by its name for the given <paramref name="principal" />.
        /// </summary>
        /// <param name="name">The name of the command to retrieve.</param>
        /// <param name="principal">The principal that is requesting the command.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting command.</returns>
        Task<Command> GetByName(string name, ClaimsPrincipal principal, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new command for the given <paramref name="principal" />.
        /// </summary>
        /// <param name="command">Request info to use when creating the command.</param>
        /// <param name="principal">The principal to create the command for.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting command.</returns>
        Task<Command> Create(CommandRequest command, ClaimsPrincipal principal, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a command by name for the given <paramref name="principal" />.
        /// </summary>
        /// <param name="name">The name of the command to update.</param>
        /// <param name="command">The updated command info.</param>
        /// <param name="principal">The principal/user to update the command for.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The updated command.</returns>
        Task<Command> UpdateByName(string name, CommandRequest command, ClaimsPrincipal principal, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a command by its name.
        /// </summary>
        /// <param name="name">The name of the command to delete.</param>
        /// <param name="principal">The principal to delete the command for.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The deleted command.</returns>
        Task<Command> DeleteByName(string name, ClaimsPrincipal principal, CancellationToken cancellationToken = default);

        /// <summary>
        /// Loads an embedded command into the assembly load context.
        /// </summary>
        /// <param name="command">The command to load.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task<ICommandRunner> LoadEmbedded(Command command, CancellationToken cancellationToken = default);

        /// <summary>
        /// Ensures that a command is accessible to the given <paramref name="principal" />.  If not,
        /// then a <see cref="CommandRequiresRoleException" /> is thrown.
        /// </summary>
        /// <param name="command">Command to validate against.</param>
        /// <param name="principal">Principal to check roles against the given <paramref name="command" />.</param>
        /// <exception cref="CommandRequiresRoleException">Thrown when the <paramref name="principal" /> does not have the required command role.</exception>
        void EnsureCommandIsAccessibleToPrincipal(Command command, ClaimsPrincipal principal);
    }
}
