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
    public class EchoCommand : ICommandRunner
    {
        private readonly ILogger<EchoCommand> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EchoCommand"/> class.
        /// </summary>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public EchoCommand(
            ILogger<EchoCommand> logger
        )
        {
            this.logger = logger;
        }

        /// <inheritdoc />
        public Task<string> Run(CommandContext context, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Received command request: {@context}", context);
            return Task.FromResult(context.Arguments[0]);
        }
    }
}
