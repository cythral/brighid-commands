using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Commands.Commands
{
    /// <summary>
    /// Facade for loading commands.
    /// </summary>
    public interface ICommandLoader
    {
        /// <summary>
        /// Loads a command by name.
        /// </summary>
        /// <param name="name">Name of the command to load.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting command.</returns>
        Task<Command> LoadCommandByName(string name, CancellationToken cancellationToken);
    }
}
