using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Auth;
using Brighid.Commands.Sdk;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Brighid.Commands.Service
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
        public async Task<IEnumerable<Command>> ListByType(CommandType type, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var scope = serviceScopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ICommandRepository>();
            return await repository.ListByType(type, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Command> GetByName(string name, ClaimsPrincipal principal, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var scope = serviceScopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ICommandRepository>();
            var command = await repository.FindCommandByName(name, cancellationToken);
            EnsureCommandIsAccessibleToPrincipal(command, principal);
            return command;
        }

        /// <inheritdoc />
        public async Task<Command> Create(CommandRequest command, ClaimsPrincipal principal, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var scope = serviceScopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ICommandRepository>();

            ValidateParameters(command);

            var mappedCommand = (Command)command;
            mappedCommand.OwnerId = Guid.Parse(principal.Identity!.Name!);

            repository.Add(mappedCommand);
            await repository.Save(cancellationToken);
            return mappedCommand;
        }

        /// <inheritdoc />
        public async Task<Command> UpdateByName(string name, CommandRequest request, ClaimsPrincipal principal, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var scope = serviceScopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ICommandRepository>();

            ValidateParameters(request);

            var command = await repository.FindCommandByName(name, cancellationToken);
            EnsurePrincipalOwnsCommandOrIsAnAdministrator(command, principal);

            command.Type = request.Type;
            command.Name = request.Name;
            command.RequiredRole = request.RequiredRole;
            command.Description = request.Description;
            command.IsEnabled = request.IsEnabled;
            command.ArgCount = request.ArgCount;
            command.ValidOptions = request.ValidOptions;

            if (request.EmbeddedLocation != null)
            {
                command.EmbeddedLocation ??= new EmbeddedCommandLocation();
                command.EmbeddedLocation.Checksum = request.EmbeddedLocation.Checksum;
                command.EmbeddedLocation.DownloadURL = request.EmbeddedLocation.DownloadURL;
                command.EmbeddedLocation.AssemblyName = request.EmbeddedLocation.AssemblyName;
                command.EmbeddedLocation.TypeName = request.EmbeddedLocation.TypeName;
            }

            await repository.Save(cancellationToken);
            return command;
        }

        /// <inheritdoc />
        public async Task<Command> DeleteByName(string name, ClaimsPrincipal principal, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var scope = serviceScopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ICommandRepository>();

            var command = await repository.FindCommandByName(name, cancellationToken);
            EnsurePrincipalOwnsCommandOrIsAnAdministrator(command, principal);

            repository.Delete(command);
            await repository.Save(cancellationToken);
            return command;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            var assembly = await downloader.DownloadCommandPackageFromS3(command.EmbeddedLocation!.DownloadURL!, command.EmbeddedLocation.AssemblyName!, cancellationToken);
            var registratorType = assembly.GetType(command.EmbeddedLocation.TypeName, false) ?? throw new CommandNotFoundException(command.Name);
            var registrator = (ICommandRegistrator)(Activator.CreateInstance(registratorType, Array.Empty<object>()) ?? throw new CommandNotFoundException(command.Name));

            var services = utilsFactory.CreateServiceCollection();
            services.AddSingleton(loggerFactory);
            services.AddLogging();

            var runner = registrator.Register(services);
            commandCache.Add(command.Name, runner);
            return runner;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsurePrincipalOwnsCommandOrIsAnAdministrator(Command command, ClaimsPrincipal principal)
        {
            if (!Guid.TryParse(principal.Identity?.Name, out var principalId)
                || (principalId != command.OwnerId && !principal.IsInRole("Administrator")))
            {
                throw new AccessDeniedException("You do not own this command.");
            }
        }

        /// <summary>
        /// Validates a command's parameters against the following rules:
        /// - should not contain multiple parameters with the same name.
        /// - should not contain multiple parameters with the same argument index.
        /// </summary>
        /// <param name="command">The command to validate parameters for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ValidateParameters(Command command)
        {
            var existingNames = new HashSet<string>();
            var existingArgIndexes = new HashSet<byte>();

            foreach (var parameter in command.Parameters)
            {
                if (!existingNames.Add(parameter.Name))
                {
                    throw new DuplicateCommandParameterException(command.Name, parameter.Name);
                }

                var argumentIndex = parameter.ArgumentIndex;
                if (argumentIndex.HasValue && !existingArgIndexes.Add(argumentIndex.Value))
                {
                    throw new DuplicateArgumentIndexException(command.Name, argumentIndex.Value);
                }
            }
        }
    }
}
