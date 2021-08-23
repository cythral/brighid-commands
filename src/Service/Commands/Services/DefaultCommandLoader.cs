using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Commands.Service
{
    /// <inheritdoc />
    public class DefaultCommandLoader : ICommandLoader
    {
        private readonly ICommandService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommandLoader"/> class.
        /// </summary>
        /// <param name="service">Service for managing commands.</param>
        public DefaultCommandLoader(
            ICommandService service
        )
        {
            this.service = service;
        }

        /// <inheritdoc />
        public async Task LoadCommand(Command command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            command.Runner = command.Type switch
            {
                CommandType.Embedded => await service.LoadEmbedded(command, cancellationToken),
                _ => throw new CommandTypeNotSupportedException(command.Type),
            };
        }
    }
}
