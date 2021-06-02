using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Amazon.S3;

using Brighid.Commands.Core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Brighid.Commands.Commands
{
    /// <inheritdoc />
    public class DefaultCommandService : ICommandService
    {
        private readonly IAmazonS3 s3Client;
        private readonly IUtilsFactory utilsFactory;
        private readonly ICommandCache commandCache;
        private readonly ICommandPackageDownloader downloader;
        private readonly ILogger<DefaultCommandService> logger;
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommandService"/> class.
        /// </summary>
        /// <param name="s3Client">Client to use for S3 API interactions.</param>
        /// <param name="utilsFactory">Factory to create utils with.</param>
        /// <param name="commandCache">Cache for name to command lookups.</param>
        /// <param name="downloader">Service for downloading command packages with.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        /// <param name="loggerFactory">Logger factory to inject into the command's service collection.</param>
        public DefaultCommandService(
            IAmazonS3 s3Client,
            IUtilsFactory utilsFactory,
            ICommandCache commandCache,
            ICommandPackageDownloader downloader,
            ILogger<DefaultCommandService> logger,
            ILoggerFactory loggerFactory
        )
        {
            this.s3Client = s3Client;
            this.utilsFactory = utilsFactory;
            this.commandCache = commandCache;
            this.downloader = downloader;
            this.logger = logger;
            this.loggerFactory = loggerFactory;
        }

        /// <inheritdoc />
        public async Task LoadEmbedded(Command command, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (commandCache.ContainsKey(command.Name!))
            {
                return;
            }

            var assembly = await downloader.DownloadCommandPackageFromS3(command.DownloadURL!, command.AssemblyName!, cancellationToken);
            var type = GetTypeFromAssembly(assembly, command.TypeName!, command.Name!);
            var services = utilsFactory.CreateServiceCollection();
            services.AddSingleton(typeof(ICommandStartup), type.StartupType);
            services.AddSingleton(loggerFactory);
            services.AddLogging();

            var intermediateProvider = services.BuildServiceProvider();
            var startup = intermediateProvider.GetRequiredService<ICommandStartup>();
            startup.ConfigureServices(services);
            services.AddSingleton(typeof(ICommand), type.CommandType);

            var provider = services.BuildServiceProvider();
            var loadedCommand = provider.GetRequiredService<ICommand>();
            commandCache.Add(command.Name!, loadedCommand);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ICommandClrType GetTypeFromAssembly(Assembly assembly, string typeName, string commandName)
        {
            var type = assembly.GetType(typeName, true) ?? throw new CommandNotFoundException(commandName);
            return utilsFactory.CreateCommandClrType(type, commandName);
        }
    }
}
