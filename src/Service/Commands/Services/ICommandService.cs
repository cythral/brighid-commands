using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Core;

namespace Brighid.Commands.Commands
{
    /// <summary>
    /// Service to manage commands with.
    /// </summary>
    public interface ICommandService
    {
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
