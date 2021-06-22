using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Brighid.Commands.Commands
{
    /// <inheritdoc />
    public class DefaultCommandService : ICommandService
    {
        private readonly IUtilsFactory utilsFactory;
        private readonly ICommandCache commandCache;
        private readonly ICommandPackageDownloader downloader;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<DefaultCommandService> logger;
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommandService"/> class.
        /// </summary>
        /// <param name="utilsFactory">Factory to create utils with.</param>
        /// <param name="commandCache">Cache for name to command lookups.</param>
        /// <param name="downloader">Service for downloading command packages with.</param>
        /// <param name="serviceScopeFactory">Factory to create service scopes with.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        /// <param name="loggerFactory">Logger factory to inject into the command's service collection.</param>
        public DefaultCommandService(
            IUtilsFactory utilsFactory,
            ICommandCache commandCache,
            ICommandPackageDownloader downloader,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<DefaultCommandService> logger,
            ILoggerFactory loggerFactory
        )
        {
            this.utilsFactory = utilsFactory;
            this.commandCache = commandCache;
            this.downloader = downloader;
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
            this.loggerFactory = loggerFactory;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Command>> List(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var scope = serviceScopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ICommandRepository>();
            return await repository.List(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Command> Create(CommandRequest command, ClaimsPrincipal principal, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var scope = serviceScopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ICommandRepository>();

            var mappedCommand = (Command)command;
            mappedCommand.OwnerId = Guid.Parse(principal.Identity!.Name!);
            Console.WriteLine(principal.Identity!.Name!);

            await repository.Add(mappedCommand, cancellationToken);
            await repository.Save(cancellationToken);
            return mappedCommand;
        }

        /// <inheritdoc />
        public void EnsureCommandIsAccessibleToPrincipal(Command command, ClaimsPrincipal principal)
        {
            if (command.RequiredRole != null && !principal.IsInRole(command.RequiredRole))
            {
                throw new CommandRequiresRoleException(command);
            }
        }

        /// <inheritdoc />
        public async Task<ICommandRunner> LoadEmbedded(Command command, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (commandCache.TryGetValue(command.Name!, out var cachedCommand))
            {
                return cachedCommand;
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
            services.AddSingleton(typeof(ICommandRunner), type.CommandType);

            var provider = services.BuildServiceProvider();
            var loadedCommand = provider.GetRequiredService<ICommandRunner>();
            commandCache.Add(command.Name!, loadedCommand);
            return loadedCommand;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ICommandClrType GetTypeFromAssembly(Assembly assembly, string typeName, string commandName)
        {
            var type = assembly.GetType(typeName, true) ?? throw new CommandNotFoundException(commandName);
            return utilsFactory.CreateCommandClrType(type, commandName);
        }
    }
}
