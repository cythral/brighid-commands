using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Auth;

using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Commands.Service
{
    /// <inheritdoc />
    public class DefaultCommandService : ICommandService
    {
        private readonly ICommandLoader loader;
        private readonly IServiceScopeFactory serviceScopeFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommandService"/> class.
        /// </summary>
        /// <param name="loader">Service for loading commands.</param>
        /// <param name="serviceScopeFactory">Factory to create service scopes with.</param>
        public DefaultCommandService(
            ICommandLoader loader,
            IServiceScopeFactory serviceScopeFactory
        )
        {
            this.loader = loader;
            this.serviceScopeFactory = serviceScopeFactory;
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
            PreloadIfEmbedded(mappedCommand, cancellationToken);
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

            command.Version++;
            command.Type = request.Type;
            command.Name = request.Name;
            command.RequiredRole = request.RequiredRole;
            command.Description = request.Description;
            command.IsEnabled = request.IsEnabled;
            command.ArgCount = request.ArgCount;
            command.ValidOptions = request.ValidOptions;
            command.Scopes = request.Scopes;
            command.EmbeddedLocation = request.EmbeddedLocation ?? command.EmbeddedLocation;

            await repository.Save(cancellationToken);
            PreloadIfEmbedded(command, cancellationToken);
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
        public async Task LoadAllEmbeddedCommands(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var commands = await ListByType(CommandType.Embedded, cancellationToken);
            var tasks = from command in commands select loader.LoadEmbedded(command, cancellationToken);
            await Task.WhenAll(tasks);
        }

        /// <inheritdoc />
        public async Task LoadCommand(Command command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            command.Runner = command.Type switch
            {
                CommandType.Embedded => await loader.LoadEmbedded(command, cancellationToken),
                _ => throw new CommandTypeNotSupportedException(command.Type),
            };
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PreloadIfEmbedded(Command command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (command.Type == CommandType.Embedded)
            {
                _ = loader.LoadEmbedded(command, CancellationToken.None);
            }
        }
    }
}
