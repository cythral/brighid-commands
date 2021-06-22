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
        /// Add a command to the change tracker (will not be persisted to the database until save is called.)
        /// </summary>
        /// <param name="command">The command to add to the database.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Add(Command command, CancellationToken cancellationToken = default);

        /// <summary>
        /// Save changes to the database.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Save(CancellationToken cancellationToken = default);

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
