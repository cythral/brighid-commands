using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// Facade for loading commands.
    /// </summary>
    public interface ICommandLoader
    {
        /// <summary>
        /// Loads all embedded commands.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task LoadAllEmbeddedCommands(CancellationToken cancellationToken);

        /// <summary>
        /// Loads a command by name.
        /// </summary>
        /// <param name="command">Command to load.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting command.</returns>
        Task LoadCommand(Command command, CancellationToken cancellationToken);
    }
}
