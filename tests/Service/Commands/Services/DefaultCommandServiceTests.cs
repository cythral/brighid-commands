using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Commands.Auth;
using Brighid.Commands.Sdk;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Commands.Service
{
    public class DefaultCommandServiceTests
    {
        [TestFixture]
        public class ListTests
        {
            [Test, Auto]
            public async Task ShouldReturnAListOfCommandsFromTheRepository(
                [Frozen] Command[] commands,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                repository.List(Any<CancellationToken>()).Returns(commands);
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                var result = await service.List(cancellationToken);

                result.Should().BeEquivalentTo(commands);
                await repository.Received().List(Is(cancellationToken));
            }
        }

        [TestFixture]
        public class ListByTypeTests
        {
            [Test, Auto]
            public async Task ShouldReturnAListOfCommandsFromTheRepositoryOfACertainType(
                CommandType commandType,
                [Frozen] Command[] commands,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                repository.ListByType(Any<CommandType>(), Any<CancellationToken>()).Returns(commands);
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                var result = await service.ListByType(commandType, cancellationToken);

                result.Should().BeEquivalentTo(commands);
                await repository.Received().ListByType(Is(commandType), Is(cancellationToken));
            }
        }

        [TestFixture]
        public class GetByNameTests
        {
            [Test, Auto]
            public async Task ShouldFetchTheCommandFromTheRepository(
                string name,
                string requiredRole,
                Guid ownerId,
                ClaimsIdentity identity,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                command.RequiredRole = requiredRole;
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                var principal = new ClaimsPrincipal(identity);
                identity.AddClaim(new Claim(ClaimTypes.Name, ownerId.ToString()));
                identity.AddClaim(new Claim(ClaimTypes.Role, requiredRole));

                var result = await service.GetByName(name, principal, cancellationToken);

                result.Should().Be(command);
                await repository.Received().FindCommandByName(Is(name), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldThrowCommandRequiresRoleIfCommandNotAccessibleToUser(
                string name,
                string requiredRole,
                Guid ownerId,
                ClaimsIdentity identity,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                command.RequiredRole = requiredRole;
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                var principal = new ClaimsPrincipal(identity);
                Func<Task> func = () => service.GetByName(name, principal, cancellationToken);

                await func.Should().ThrowAsync<CommandRequiresRoleException>();
            }
        }

        [TestFixture]
        public class CreateTests
        {
            [Test, Auto]
            public async Task ShouldCreateANewCommandWithTheOwnerIdSetToPrincipalsId(
                Guid ownerId,
                CommandRequest request,
                ClaimsIdentity identity,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                var principal = new ClaimsPrincipal(identity);
                identity.AddClaim(new Claim(ClaimTypes.Name, ownerId.ToString()));

                var result = await service.Create(request, principal, cancellationToken);

                result.Should().BeSameAs(request);
                result.OwnerId.Should().Be(ownerId);
                repository.Received().Add(Is(result));
                await repository.Received().Save(Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldLoadCommandIfItsEmbedded(
                Guid ownerId,
                CommandRequest request,
                ClaimsIdentity identity,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandLoader loader,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                var principal = new ClaimsPrincipal(identity);
                identity.AddClaim(new Claim(ClaimTypes.Name, ownerId.ToString()));

                request.Type = CommandType.Embedded;

                await service.Create(request, principal, cancellationToken);
                await loader.Received().LoadEmbedded(Is(request), Any<CancellationToken>());
            }

            [Test, Auto]
            public async Task ShouldThrowIfDuplicateParametersWereGiven(
                string commandName,
                string parameterName,
                Guid ownerId,
                CommandRequest request,
                ClaimsIdentity identity,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                request.Name = commandName;
                request.Parameters = new[]
                {
                    new CommandParameter(parameterName),
                    new CommandParameter("a"),
                    new CommandParameter(parameterName),
                    new CommandParameter("b"),
                };

                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                var principal = new ClaimsPrincipal(identity);
                identity.AddClaim(new Claim(ClaimTypes.Name, ownerId.ToString()));

                Func<Task> func = () => service.Create(request, principal, cancellationToken);

                var exception = (await func.Should().ThrowAsync<DuplicateCommandParameterException>()).Which;
                exception.CommandName.Should().Be(commandName);
                exception.ParameterName.Should().Be(parameterName);

                repository.DidNotReceive().Add(Any<Command>());
                await repository.DidNotReceive().Save(Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldThrowIfDuplicateArgumentIndexesWereGiven(
                string commandName,
                byte argumentIndex,
                Guid ownerId,
                CommandRequest request,
                ClaimsIdentity identity,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                request.Name = commandName;
                request.Parameters = new[]
                {
                    new CommandParameter("a", argumentIndex: argumentIndex),
                    new CommandParameter("b"),
                    new CommandParameter("c", argumentIndex: argumentIndex),
                    new CommandParameter("b"),
                };

                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                var principal = new ClaimsPrincipal(identity);
                identity.AddClaim(new Claim(ClaimTypes.Name, ownerId.ToString()));

                Func<Task> func = () => service.Create(request, principal, cancellationToken);

                var exception = (await func.Should().ThrowAsync<DuplicateArgumentIndexException>()).Which;
                exception.CommandName.Should().Be(commandName);
                exception.ArgumentIndex.Should().Be(argumentIndex);

                repository.DidNotReceive().Add(Any<Command>());
                await repository.DidNotReceive().Save(Is(cancellationToken));
            }
        }

        [TestFixture]
        public class UpdateByNameTests
        {
            [Test, Auto]
            public async Task ShouldFindCommandInDatabase(
                string name,
                CommandRequest request,
                ClaimsIdentity identity,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                identity.AddClaim(new Claim(ClaimTypes.Name, command.OwnerId.ToString()));
                var principal = new ClaimsPrincipal(identity);

                var result = await service.UpdateByName(name, request, principal, cancellationToken);

                result.Should().Be(command);
                await repository.Received().FindCommandByName(name, cancellationToken);
            }

            [Test, Auto]
            public async Task ShouldThrowIfDuplicateParametersWereGiven(
                string name,
                string parameterName,
                CommandRequest request,
                ClaimsIdentity identity,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                request.Name = name;
                request.Parameters = new[]
                {
                    new CommandParameter(parameterName),
                    new CommandParameter("a"),
                    new CommandParameter(parameterName),
                    new CommandParameter("b"),
                };

                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                identity.AddClaim(new Claim(ClaimTypes.Name, command.OwnerId.ToString()));
                var principal = new ClaimsPrincipal(identity);

                Func<Task> func = () => service.UpdateByName(name, request, principal, cancellationToken);

                var exception = (await func.Should().ThrowAsync<DuplicateCommandParameterException>()).Which;
                exception.CommandName.Should().Be(name);
                exception.ParameterName.Should().Be(parameterName);

                await repository.DidNotReceive().FindCommandByName(name, cancellationToken);
            }

            [Test, Auto]
            public async Task ShouldThrowIfDuplicateArgumentIndexesWereGiven(
                string name,
                byte argumentIndex,
                CommandRequest request,
                ClaimsIdentity identity,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                request.Name = name;
                request.Parameters = new[]
                {
                    new CommandParameter("a", argumentIndex: argumentIndex),
                    new CommandParameter("b"),
                    new CommandParameter("c", argumentIndex: argumentIndex),
                    new CommandParameter("b"),
                };

                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                identity.AddClaim(new Claim(ClaimTypes.Name, command.OwnerId.ToString()));
                var principal = new ClaimsPrincipal(identity);

                Func<Task> func = () => service.UpdateByName(name, request, principal, cancellationToken);

                var exception = (await func.Should().ThrowAsync<DuplicateArgumentIndexException>()).Which;
                exception.CommandName.Should().Be(name);
                exception.ArgumentIndex.Should().Be(argumentIndex);

                await repository.DidNotReceive().FindCommandByName(name, cancellationToken);
            }

            [Test, Auto]
            public async Task ShouldUpdateCommandType(
                string name,
                ClaimsIdentity identity,
                CommandRequest request,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                request.Type = (CommandType)(-1);
                identity.AddClaim(new Claim(ClaimTypes.Name, command.OwnerId.ToString()));
                var principal = new ClaimsPrincipal(identity);

                var result = await service.UpdateByName(name, request, principal, cancellationToken);

                result.Should().BeSameAs(command);
                command.Type.Should().Be(request.Type);
            }

            [Test, Auto]
            public async Task ShouldUpdateCommandName(
                string name,
                ClaimsIdentity identity,
                CommandRequest request,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                identity.AddClaim(new Claim(ClaimTypes.Name, command.OwnerId.ToString()));
                var principal = new ClaimsPrincipal(identity);

                var result = await service.UpdateByName(name, request, principal, cancellationToken);

                result.Should().BeSameAs(command);
                command.Name.Should().Be(request.Name);
            }

            [Test, Auto]
            public async Task ShouldIncrementCommandVersion(
                string name,
                ClaimsIdentity identity,
                CommandRequest request,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                command.Version = 2;

                identity.AddClaim(new Claim(ClaimTypes.Name, command.OwnerId.ToString()));
                var principal = new ClaimsPrincipal(identity);

                var result = await service.UpdateByName(name, request, principal, cancellationToken);

                result.Should().BeSameAs(command);
                command.Version.Should().Be(3);
            }

            [Test, Auto]
            public async Task ShouldUpdateCommandRequiredRole(
                string name,
                ClaimsIdentity identity,
                CommandRequest request,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                identity.AddClaim(new Claim(ClaimTypes.Name, command.OwnerId.ToString()));
                var principal = new ClaimsPrincipal(identity);

                var result = await service.UpdateByName(name, request, principal, cancellationToken);

                result.Should().BeSameAs(command);
                command.RequiredRole.Should().Be(request.RequiredRole);
            }

            [Test, Auto]
            public async Task ShouldUpdateCommandChecksum(
                string name,
                ClaimsIdentity identity,
                CommandRequest request,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                identity.AddClaim(new Claim(ClaimTypes.Name, command.OwnerId.ToString()));
                var principal = new ClaimsPrincipal(identity);

                var result = await service.UpdateByName(name, request, principal, cancellationToken);

                result.Should().BeSameAs(command);

                var location = command.EmbeddedLocation!.Value;
                location.Checksum.Should().Be(request.EmbeddedLocation!.Value.Checksum);
            }

            [Test, Auto]
            public async Task ShouldUpdateCommandDescription(
                string name,
                ClaimsIdentity identity,
                CommandRequest request,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                identity.AddClaim(new Claim(ClaimTypes.Name, command.OwnerId.ToString()));
                var principal = new ClaimsPrincipal(identity);

                var result = await service.UpdateByName(name, request, principal, cancellationToken);

                result.Should().BeSameAs(command);
                command.Description.Should().Be(request.Description);
            }

            [Test, Auto]
            public async Task ShouldUpdateCommandDownloadUrl(
                string name,
                ClaimsIdentity identity,
                CommandRequest request,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                identity.AddClaim(new Claim(ClaimTypes.Name, command.OwnerId.ToString()));
                var principal = new ClaimsPrincipal(identity);

                var result = await service.UpdateByName(name, request, principal, cancellationToken);

                result.Should().BeSameAs(command);
                var location = command.EmbeddedLocation!.Value;
                location.DownloadURL.Should().Be(request.EmbeddedLocation!.Value.DownloadURL);
            }

            [Test, Auto]
            public async Task ShouldUpdateCommandAssemblyName(
                string name,
                ClaimsIdentity identity,
                CommandRequest request,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                identity.AddClaim(new Claim(ClaimTypes.Name, command.OwnerId.ToString()));
                var principal = new ClaimsPrincipal(identity);

                var result = await service.UpdateByName(name, request, principal, cancellationToken);

                result.Should().BeSameAs(command);
                var location = command.EmbeddedLocation!.Value;
                location.AssemblyName.Should().Be(request.EmbeddedLocation!.Value.AssemblyName);
            }

            [Test, Auto]
            public async Task ShouldUpdateCommandTypeName(
                string name,
                ClaimsIdentity identity,
                CommandRequest request,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                identity.AddClaim(new Claim(ClaimTypes.Name, command.OwnerId.ToString()));
                var principal = new ClaimsPrincipal(identity);

                var result = await service.UpdateByName(name, request, principal, cancellationToken);

                result.Should().BeSameAs(command);
                var location = command.EmbeddedLocation!.Value;
                location.TypeName.Should().Be(request.EmbeddedLocation!.Value.TypeName);
            }

            [Test, Auto]
            public async Task ShouldUpdateCommandEnabledState(
                string name,
                ClaimsIdentity identity,
                CommandRequest request,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                identity.AddClaim(new Claim(ClaimTypes.Name, command.OwnerId.ToString()));
                var principal = new ClaimsPrincipal(identity);

                var result = await service.UpdateByName(name, request, principal, cancellationToken);

                result.Should().BeSameAs(command);
                command.IsEnabled.Should().Be(request.IsEnabled);
            }

            [Test, Auto]
            public async Task ShouldUpdateCommandArgCount(
                string name,
                ClaimsIdentity identity,
                CommandRequest request,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                identity.AddClaim(new Claim(ClaimTypes.Name, command.OwnerId.ToString()));
                var principal = new ClaimsPrincipal(identity);

                var result = await service.UpdateByName(name, request, principal, cancellationToken);

                result.Should().BeSameAs(command);
                command.ArgCount.Should().Be(request.ArgCount);
            }

            [Test, Auto]
            public async Task ShouldUpdateCommandValidOptions(
                string name,
                ClaimsIdentity identity,
                CommandRequest request,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                identity.AddClaim(new Claim(ClaimTypes.Name, command.OwnerId.ToString()));
                var principal = new ClaimsPrincipal(identity);

                var result = await service.UpdateByName(name, request, principal, cancellationToken);

                result.Should().BeSameAs(command);
                command.ValidOptions.Should().BeEquivalentTo(request.ValidOptions);
            }

            [Test, Auto]
            public async Task ShouldUpdateCommandScopes(
                string name,
                ClaimsIdentity identity,
                CommandRequest request,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                identity.AddClaim(new Claim(ClaimTypes.Name, command.OwnerId.ToString()));
                var principal = new ClaimsPrincipal(identity);

                var result = await service.UpdateByName(name, request, principal, cancellationToken);

                result.Should().BeSameAs(command);
                command.Scopes.Should().BeEquivalentTo(request.Scopes);
            }

            [Test, Auto]
            public async Task ShouldSaveCommandInTheDatabase(
                string name,
                ClaimsIdentity identity,
                CommandRequest request,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                identity.AddClaim(new Claim(ClaimTypes.Name, command.OwnerId.ToString()));
                var principal = new ClaimsPrincipal(identity);

                await service.UpdateByName(name, request, principal, cancellationToken);

                await repository.Received().Save(Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldNotUpdateCommandIfPrincipalDoesntOwnCommandAndIsNotAnAdministrator(
                string name,
                ClaimsPrincipal principal,
                CommandRequest request,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                Func<Task> func = () => service.UpdateByName(name, request, principal, cancellationToken);

                await func.Should().ThrowAsync<AccessDeniedException>();
                await repository.DidNotReceive().Save(Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldUpdateCommandIfPrincipalDoesntOwnCommandButIsAnAdministrator(
                string name,
                ClaimsIdentity identity,
                CommandRequest request,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                identity.AddClaim(new Claim(ClaimTypes.Name, Guid.NewGuid().ToString()));
                identity.AddClaim(new Claim(ClaimTypes.Role, "Administrator"));
                var principal = new ClaimsPrincipal(identity);

                await service.UpdateByName(name, request, principal, cancellationToken);

                await repository.Received().Save(Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldLoadCommandIfItsEmbedded(
                string name,
                ClaimsIdentity identity,
                CommandRequest request,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Frozen] ICommandLoader loader,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                request.Type = CommandType.Embedded;
                identity.AddClaim(new Claim(ClaimTypes.Name, command.OwnerId.ToString()));
                var principal = new ClaimsPrincipal(identity);

                await service.UpdateByName(name, request, principal, cancellationToken);

                await loader.Received().LoadEmbedded(Is(command), Any<CancellationToken>());
            }
        }

        [TestFixture]
        public class DeleteByNameTests
        {
            [Test, Auto]
            public async Task ShouldFindCommandInDatabase(
                string name,
                ClaimsIdentity identity,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                identity.AddClaim(new Claim(ClaimTypes.Name, command.OwnerId.ToString()));
                var principal = new ClaimsPrincipal(identity);

                var result = await service.DeleteByName(name, principal, cancellationToken);

                result.Should().Be(command);
                await repository.Received().FindCommandByName(name, cancellationToken);
            }

            [Test, Auto]
            public async Task ShouldDeleteCommandFromTheDatabase(
                string name,
                ClaimsIdentity identity,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                identity.AddClaim(new Claim(ClaimTypes.Name, command.OwnerId.ToString()));
                var principal = new ClaimsPrincipal(identity);

                await service.DeleteByName(name, principal, cancellationToken);

                repository.Received().Delete(command);
                await repository.Received().Save(Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldNotDeleteCommandIfPrincipalDoesntOwnCommandAndIsNotAnAdministrator(
                string name,
                ClaimsPrincipal principal,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                Func<Task> func = () => service.DeleteByName(name, principal, cancellationToken);

                await func.Should().ThrowAsync<AccessDeniedException>();
                repository.DidNotReceive().Delete(command);
                await repository.DidNotReceive().Save(Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldDeleteCommandIfPrincipalDoesntOwnCommandButIsAnAdministrator(
                string name,
                ClaimsIdentity identity,
                [Frozen] Command command,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                identity.AddClaim(new Claim(ClaimTypes.Name, Guid.NewGuid().ToString()));
                identity.AddClaim(new Claim(ClaimTypes.Role, "Administrator"));
                var principal = new ClaimsPrincipal(identity);

                await service.DeleteByName(name, principal, cancellationToken);

                repository.Received().Delete(command);
                await repository.Received().Save(Is(cancellationToken));
            }
        }

        [TestFixture]
        public class EnsureCommandIsAccessibleToPrincipalTests
        {
            [Test, Auto]
            public void ShouldThrowIfUserIsNotInRole(
                string role,
                Command command,
                ClaimsPrincipal principal,
                [Target] DefaultCommandService service
            )
            {
                command.RequiredRole = role;

                Action func = () => service.EnsureCommandIsAccessibleToPrincipal(command, principal);

                func.Should().Throw<CommandRequiresRoleException>().And.RequiredRole.Should().Be(role);
            }

            [Test, Auto]
            public void ShouldNotThrowIfUserIsInRole(
                string role,
                Command command,
                ClaimsPrincipal principal,
                [Target] DefaultCommandService service
            )
            {
                command.RequiredRole = role;

                var identity = new ClaimsIdentity();
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
                principal.AddIdentity(identity);

                Action func = () => service.EnsureCommandIsAccessibleToPrincipal(command, principal);

                func.Should().NotThrow<CommandRequiresRoleException>();
            }

            [Test, Auto]
            public void ShouldNotThrowIfRequiredRoleIsNull(
                string randomRole,
                Command command,
                ClaimsPrincipal principal,
                [Target] DefaultCommandService service
            )
            {
                command.RequiredRole = null;

                var identity = new ClaimsIdentity();
                identity.AddClaim(new Claim(ClaimTypes.Role, randomRole));
                principal.AddIdentity(identity);

                Action func = () => service.EnsureCommandIsAccessibleToPrincipal(command, principal);

                func.Should().NotThrow<CommandRequiresRoleException>();
            }
        }

        [TestFixture]
        public class LoadAllEmbeddedCommandsTests
        {
            [Test, Auto]
            public async Task ShouldLoadEveryEmbeddedCommand(
                Command command1,
                Command command2,
                [Frozen] IServiceScope scope,
                [Frozen] ICommandRepository repository,
                [Frozen] ICommandLoader loader,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                scope.ServiceProvider.Returns(new ServiceCollection()
                    .AddSingleton(repository)
                    .BuildServiceProvider()
                );

                repository.ListByType(Any<CommandType>(), Any<CancellationToken>()).Returns(new[] { command1, command2 });

                await service.LoadAllEmbeddedCommands(cancellationToken);

                await loader.Received().LoadEmbedded(Is(command1), Is(cancellationToken));
                await loader.Received().LoadEmbedded(Is(command2), Is(cancellationToken));
            }
        }

        [TestFixture]
        public class LoadCommandByNameTests
        {
            [Test, Auto]
            public async Task ShouldLoadEmbeddedCommands(
                string name,
                Command command,
                [Frozen] ICommandRunner runner,
                [Frozen, Substitute] ICommandLoader loader,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                command.Type = CommandType.Embedded;
                await service.LoadCommand(command, cancellationToken);

                command.Runner.Should().Be(runner);
                await loader.Received().LoadEmbedded(Is(command), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldThrowForUnsupportedCommands(
                Command command,
                [Frozen, Substitute] ICommandLoader loader,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                command.Type = (CommandType)(-1);
                Func<Task> func = () => service.LoadCommand(command, cancellationToken);

                await func.Should().ThrowAsync<CommandTypeNotSupportedException>();
            }
        }
    }
}
