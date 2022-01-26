using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// Service that loads commands on startup.
    /// </summary>
    public class StartupCommandLoader : IHostedService
    {
        private readonly ICommandLoader loader;
        private readonly ILogger<StartupCommandLoader> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupCommandLoader" /> class.
        /// </summary>
        /// <param name="loader">Service that provides command-loading functionality.</param>
        /// <param name="logger">Service used to log information to some destination(s).</param>
        public StartupCommandLoader(
            ICommandLoader loader,
            ILogger<StartupCommandLoader> logger
        )
        {
            this.loader = loader;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Pre-loading all embedded commands.");
            await loader.LoadAllEmbeddedCommands(cancellationToken);
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
