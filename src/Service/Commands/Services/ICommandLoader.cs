using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Sdk;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// Facade for loading commands.
    /// </summary>
    public interface ICommandLoader
    {
        /// <summary>
        /// Loads an embedded command into the assembly load context.
        /// </summary>
        /// <param name="command">The command to load.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task<ICommandRunner> LoadEmbedded(Command command, CancellationToken cancellationToken = default);
    }
}
