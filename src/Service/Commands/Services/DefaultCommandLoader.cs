using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Commands.Commands
{
    /// <inheritdoc />
    public class DefaultCommandLoader : ICommandLoader
    {
        private readonly ICommandRepository repository;
        private readonly ICommandService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommandLoader"/> class.
        /// </summary>
        /// <param name="repository">Repository layer for commands.</param>
        /// <param name="service">Service for managing commands.</param>
        public DefaultCommandLoader(
            ICommandRepository repository,
            ICommandService service
        )
        {
            this.repository = repository;
            this.service = service;
        }

        /// <inheritdoc />
        public async Task<Command> LoadCommandByName(string name, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var command = await repository.FindCommandByName(name, cancellationToken);
            command.Runner = command.Type switch
            {
                CommandType.Embedded => await service.LoadEmbedded(command, cancellationToken),
                _ => throw new CommandTypeNotSupportedException(command.Type),
            };

            return command;
        }
    }
}
