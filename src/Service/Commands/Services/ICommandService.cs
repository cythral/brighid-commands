using System.Threading;
using System.Threading.Tasks;

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
        Task LoadEmbedded(Command command, CancellationToken cancellationToken = default);
    }
}
