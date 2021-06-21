using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Commands.Commands
{
    /// <summary>
    /// Repository to lookup commands from.
    /// </summary>
    public interface ICommandRepository
    {
        /// <summary>
        /// Lists available commands.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A list of commands.</returns>
        Task<IEnumerable<Command>> List(CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a command by its name.
        /// </summary>
        /// <param name="name">The name of the command to find.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting command.</returns>
        Task<Command> FindCommandByName(string name, CancellationToken cancellationToken = default);
    }
}
