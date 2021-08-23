using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Amazon.S3;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Commands.Auth;
using Brighid.Commands.Core;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
                command.EmbeddedLocation!.Checksum.Should().Be(request.EmbeddedLocation!.Checksum);
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
                command.EmbeddedLocation!.DownloadURL.Should().Be(request.EmbeddedLocation!.DownloadURL);
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
                command.EmbeddedLocation!.AssemblyName.Should().Be(request.EmbeddedLocation!.AssemblyName);
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
                command.EmbeddedLocation!.TypeName.Should().Be(request.EmbeddedLocation!.TypeName);
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
        public class LoadEmbedded
        {
            [Test, Auto]
            public async Task ShouldDownloadFromS3(
                string contents,
                Command command,
                MemoryStream fileStream,
                [Frozen] S3UriInfo s3UriInfo,
                [Frozen, Substitute] Assembly assembly,
                [Frozen, Substitute] ICommandClrType commandClrType,
                [Frozen, Substitute] ICommandPackageDownloader downloader,
                [Frozen, Substitute] IUtilsFactory utilsFactory,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                using var responseStream = new MemoryStream(Encoding.UTF8.GetBytes(contents));
                assembly.GetType(Any<string>(), Any<bool>()).Returns(typeof(TestCommand));
                commandClrType.StartupType.Returns(typeof(TestStartup));
                commandClrType.CommandType.Returns(typeof(TestCommand));

                await service.LoadEmbedded(command, cancellationToken);

                await downloader.Received().DownloadCommandPackageFromS3(Is(command.EmbeddedLocation!.DownloadURL!), Is(command.EmbeddedLocation!.AssemblyName!), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldThrowIfTheTypeCouldNotBeLoadedFromTheAssembly(
                string contents,
                Command command,
                MemoryStream fileStream,
                Type commandType,
                [Frozen, Substitute] Assembly assembly,
                [Frozen] S3UriInfo s3UriInfo,
                [Frozen, Substitute] IUtilsFactory utilsFactory,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                assembly.GetType(Any<string>(), Any<bool>()).Returns((Type?)null);

                Func<Task> func = () => service.LoadEmbedded(command, cancellationToken);

                await func.Should().ThrowAsync<CommandNotFoundException>();
            }

            [Test, Auto]
            public async Task ShouldRunTheStartupClass(
                string contents,
                Command command,
                MemoryStream fileStream,
                CommandStartupAttribute startupAttribute,
                [Frozen] IServiceCollection services,
                [Frozen] ICommandClrType commandType,
                [Frozen, Substitute] Assembly assembly,
                [Frozen] S3UriInfo s3UriInfo,
                [Frozen, Substitute] IAmazonS3 s3Client,
                [Frozen, Substitute] IUtilsFactory utilsFactory,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                commandType.StartupType.Returns(typeof(TestStartup));
                commandType.CommandType.Returns(typeof(TestCommand));

                await service.LoadEmbedded(command, cancellationToken);

                var query = from svc in services
                            where svc.ImplementationType == typeof(TestDummyClass)
                            select svc;

                query.Should().HaveCount(1);
            }

            [Test, Auto]
            public async Task ShouldAddTheCommandToTheCache(
                string contents,
                Command command,
                MemoryStream fileStream,
                CommandStartupAttribute startupAttribute,
                [Frozen] ICommandCache cache,
                [Frozen] IServiceCollection services,
                [Frozen] ICommandClrType commandType,
                [Frozen, Substitute] Assembly assembly,
                [Frozen] S3UriInfo s3UriInfo,
                [Frozen, Substitute] IAmazonS3 s3Client,
                [Frozen, Substitute] IUtilsFactory utilsFactory,
                [Target] DefaultCommandService service,
                CancellationToken cancellationToken
            )
            {
                assembly.GetType(Any<string>(), Any<bool>()).Returns(typeof(TestCommand));
                commandType.StartupType.Returns(typeof(TestStartup));
                commandType.CommandType.Returns(typeof(TestCommand));

                await service.LoadEmbedded(command, cancellationToken);

                cache.Should().ContainKey(command.Name!).WhichValue.Should().BeOfType<TestCommand>();
            }

            private class TestStartup : ICommandStartup
            {
                private readonly ILogger<TestStartup> logger;

                public TestStartup(
                    ILogger<TestStartup> logger
                )
                {
                    this.logger = logger;
                }

                public void ConfigureServices(IServiceCollection services)
                {
                    services.AddSingleton<TestDummyClass>();
                }
            }

            private class TestCommand : ICommandRunner
            {
                public Task<string> Run(CommandContext context, CancellationToken cancellationToken = default)
                {
                    throw new NotImplementedException();
                }
            }

            private class TestDummyClass
            {
            }
        }
    }
}
