using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Core;

using Microsoft.Extensions.Logging;

namespace Brighid.Commands.TestCommands
{
    /// <summary>
    /// Ping command.
    /// </summary>
    [CommandStartup(typeof(PingCommandStartup))]
    public class PingCommand : ICommand
    {
        private readonly ILogger<PingCommand> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PingCommand"/> class.
        /// </summary>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public PingCommand(
            ILogger<PingCommand> logger
        )
        {
            this.logger = logger;
        }

        /// <inheritdoc />
        public Task<string> Run(CommandContext context, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Received command request...");
            return Task.FromResult("Pong");
        }
    }
}
