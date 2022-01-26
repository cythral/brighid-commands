using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Commands.Service
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
        void Add(Command command);

        /// <summary>
        /// Delete a command.
        /// </summary>
        /// <param name="command">The command to delete.</param>
        void Delete(Command command);

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
        /// Lists available commands of a certain type.
        /// </summary>
        /// <param name="type">The type of commands to list.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A list of commands.</returns>
        Task<IEnumerable<Command>> ListByType(CommandType type, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a command by its name.
        /// </summary>
        /// <param name="name">The name of the command to find.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting command.</returns>
        Task<Command> FindCommandByName(string name, CancellationToken cancellationToken = default);
    }
}
