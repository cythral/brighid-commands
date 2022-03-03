using System;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Sdk;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Brighid.Commands.Service
{
    /// <inheritdoc />
    public class DefaultCommandLoader : ICommandLoader
    {
        private readonly ICommandPackageDownloader downloader;
        private readonly ICommandCache commandCache;
        private readonly ILoggerFactory loggerFactory;
        private readonly IUtilsFactory utilsFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommandLoader"/> class.
        /// </summary>
        /// <param name="downloader">Service used for downloading command packages.</param>
        /// <param name="commandCache">Cache for name to command lookups.</param>
        /// <param name="utilsFactory">Factory to create utils with.</param>
        /// <param name="loggerFactory">Logger factory to inject into the command's service collection.</param>
        public DefaultCommandLoader(
            ICommandPackageDownloader downloader,
            ICommandCache commandCache,
            IUtilsFactory utilsFactory,
            ILoggerFactory loggerFactory
        )
        {
            this.downloader = downloader;
            this.commandCache = commandCache;
            this.utilsFactory = utilsFactory;
            this.loggerFactory = loggerFactory;
        }

        /// <inheritdoc />
        public async Task<ICommandRunner> LoadEmbedded(Command command, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (commandCache.TryGetValue(command.Name!, out var cachedCommand) && cachedCommand.Version >= command.Version)
            {
                return cachedCommand.Runner;
            }

            var assembly = await downloader.DownloadCommandPackageFromS3(command.EmbeddedLocation!.DownloadURL!, command.EmbeddedLocation.AssemblyName!, cancellationToken);
            var registratorType = assembly.GetType(command.EmbeddedLocation.TypeName, false) ?? throw new CommandNotFoundException(command.Name);
            var registrator = (ICommandRegistrator)(Activator.CreateInstance(registratorType, Array.Empty<object>()) ?? throw new CommandNotFoundException(command.Name));

            var services = utilsFactory.CreateServiceCollection();
            services.AddSingleton(loggerFactory);
            services.AddLogging();

            var runner = registrator.Register(services);
            commandCache[command.Name] = new CommandVersion(command.Version, runner);
            return runner;
        }
    }
}
