using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// Service that loads commands on startup.
    /// </summary>
    public class CommandLoaderBackgroundService : IHostedService
    {
        private readonly ICommandService commandService;
        private readonly ILogger<CommandLoaderBackgroundService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLoaderBackgroundService" /> class.
        /// </summary>
        /// <param name="commandService">Service that provides command-loading functionality.</param>
        /// <param name="logger">Service used to log information to some destination(s).</param>
        public CommandLoaderBackgroundService(
            ICommandService commandService,
            ILogger<CommandLoaderBackgroundService> logger
        )
        {
            this.commandService = commandService;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Pre-loading all embedded commands.");
            await commandService.LoadAllEmbeddedCommands(cancellationToken);
            logger.LogInformation("All embedded commands have been loaded.");
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }
}
