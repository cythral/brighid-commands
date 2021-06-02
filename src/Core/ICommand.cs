using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Commands.Core
{
    /// <summary>
    /// Represents a command.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Runs the command.
        /// </summary>
        /// <param name="context">The context to use when running the command.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>Reply to send back to the user.</returns>
        Task<string> Run(CommandContext context, CancellationToken cancellationToken = default);
    }
}
