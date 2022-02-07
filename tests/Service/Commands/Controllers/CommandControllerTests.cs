using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Commands.Auth;
using Brighid.Commands.Sdk;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Commands.Service
{
    public class CommandControllerTests
    {
        [TestFixture]
        public class ListTests
        {
            [Test, Auto]
            public async Task ShouldReturnListOfCommandsFromService(
                HttpContext httpContext,
                [Frozen] Command[] commands,
                [Frozen] ICommandService service,
                [Target] CommandController controller
            )
            {
                service.List(Any<CancellationToken>()).Returns(commands);

                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                var result = (await controller.List()).Result;

                result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(commands);

                await service.Received().List(Is(httpContext.RequestAborted));
            }
        }

        [TestFixture]
        public class CreateCommandTests
        {
            [Test, Auto]
            public async Task ShouldCreateACommand(
                HttpContext httpContext,
                CommandRequest request,
                [Frozen] Command command,
                [Frozen] ICommandService service,
                [Target] CommandController controller,
                CancellationToken cancellationToken
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                var result = (await controller.CreateCommand(request)).Result;

                result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be(command);

                await service.Received().Create(Is(request), Is(httpContext.User), Is(cancellationToken));
            }
        }

        [TestFixture]
        public class UpdateCommandTests
        {
            [Test, Auto]
            public async Task ShouldUpdateACommand(
                string name,
                HttpContext httpContext,
                CommandRequest request,
                [Frozen] Command command,
                [Frozen] ICommandService service,
                [Target] CommandController controller,
                CancellationToken cancellationToken
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                var result = (await controller.UpdateCommand(name, request)).Result;

                result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be(command);

                await service.Received().UpdateByName(Is(name), Is(request), Is(httpContext.User), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldReturnForbiddenIfServiceThrowsAccessDenied(
                string name,
                string message,
                HttpContext httpContext,
                CommandRequest request,
                [Frozen] Command command,
                [Frozen] ICommandService service,
                [Target] CommandController controller,
                CancellationToken cancellationToken
            )
            {
                service.UpdateByName(Any<string>(), Any<CommandRequest>(), Any<ClaimsPrincipal>(), Any<CancellationToken>()).Throws(new AccessDeniedException(message));

                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                var result = (await controller.UpdateCommand(name, request)).Result;

                result.Should().BeOfType<ForbidResult>();

                await service.Received().UpdateByName(Is(name), Is(request), Is(httpContext.User), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldReturnNotFoundIfServiceThrowsCommandNotFound(
                string name,
                string message,
                HttpContext httpContext,
                CommandRequest request,
                [Frozen] Command command,
                [Frozen] ICommandService service,
                [Target] CommandController controller,
                CancellationToken cancellationToken
            )
            {
                service.UpdateByName(Any<string>(), Any<CommandRequest>(), Any<ClaimsPrincipal>(), Any<CancellationToken>()).Throws(new CommandNotFoundException(name));

                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                var result = (await controller.UpdateCommand(name, request)).Result;

                result.Should().BeOfType<NotFoundResult>();

                await service.Received().UpdateByName(Is(name), Is(request), Is(httpContext.User), Is(cancellationToken));
            }
        }

        [TestFixture]
        public class DeleteCommandTests
        {
            [Test, Auto]
            public async Task ShouldDeleteACommand(
                string name,
                HttpContext httpContext,
                [Frozen] Command command,
                [Frozen] ICommandService service,
                [Target] CommandController controller,
                CancellationToken cancellationToken
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                var result = (await controller.DeleteCommand(name)).Result;

                result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be(command);

                await service.Received().DeleteByName(Is(name), Is(httpContext.User), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldReturnForbiddenIfServiceThrowsAccessDenied(
                string name,
                string message,
                HttpContext httpContext,
                [Frozen] Command command,
                [Frozen] ICommandService service,
                [Target] CommandController controller,
                CancellationToken cancellationToken
            )
            {
                service.DeleteByName(Any<string>(), Any<ClaimsPrincipal>(), Any<CancellationToken>()).Throws(new AccessDeniedException(message));

                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                var result = (await controller.DeleteCommand(name)).Result;

                result.Should().BeOfType<ForbidResult>();

                await service.Received().DeleteByName(Is(name), Is(httpContext.User), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldReturnNotFoundIfServiceThrowsCommandNotFound(
                string name,
                string message,
                HttpContext httpContext,
                [Frozen] Command command,
                [Frozen] ICommandService service,
                [Target] CommandController controller,
                CancellationToken cancellationToken
            )
            {
                service.DeleteByName(Any<string>(), Any<ClaimsPrincipal>(), Any<CancellationToken>()).Throws(new CommandNotFoundException(name));

                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                var result = (await controller.DeleteCommand(name)).Result;

                result.Should().BeOfType<NotFoundResult>();

                await service.Received().DeleteByName(Is(name), Is(httpContext.User), Is(cancellationToken));
            }
        }

        [TestFixture]
        public class GetCommandParameters
        {
            [Test, Auto]
            public async Task ShouldReturnParameters(
                string name,
                HttpContext httpContext,
                [Frozen] Command command,
                [Frozen, Substitute] ICommandService service,
                [Target] CommandController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var result = (await controller.GetCommandParameters(name)).Result;

                result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(command.Parameters);

                await service.Received().GetByName(Is(name), Is(httpContext.User), Is(httpContext.RequestAborted));
            }
        }

        [TestFixture]
        public class ExecuteTests
        {
            [Test, Auto]
            public async Task ShouldGetTheCommandByName(
                string commandName,
                HttpContext httpContext,
                ExecuteCommandRequest request,
                [Frozen, Substitute] ICommandService service,
                [Target] CommandController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                await controller.Execute(commandName);

                await service.Received().GetByName(Is(commandName), Is(httpContext.User), Is(httpContext.RequestAborted));
            }

            [Test, Auto]
            public async Task ShouldLoadTheCommandByName(
                string commandName,
                HttpContext httpContext,
                ExecuteCommandRequest request,
                [Frozen] Command command,
                [Frozen, Substitute] ICommandService service,
                [Target] CommandController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                await controller.Execute(commandName);

                await service.Received().LoadCommand(Is(command), Is(httpContext.RequestAborted));
            }

            [Test, Auto]
            public async Task ShouldExecuteTheLoadedCommand(
                string commandName,
                HttpContext httpContext,
                ExecuteCommandRequest request,
                [Frozen] ICommandRunner runner,
                [Frozen, Substitute] ICommandLoader loader,
                [Target] CommandController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                await controller.Execute(commandName);

                await runner.Received().Run(
                    Is<CommandContext>(context => context.InputStream == httpContext.Request.Body),
                    Is(httpContext.RequestAborted)
                );
            }

            [Test, Auto]
            public async Task ShouldReturnAcceptedIfCommandTypeIsNotEmbedded(
                string commandName,
                HttpContext httpContext,
                ExecuteCommandRequest request,
                [Frozen] ICommandRunner runner,
                [Frozen] Command command,
                [Frozen, Substitute] ICommandLoader loader,
                [Target] CommandController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                command.Type = (CommandType)(-1);
                var result = (await controller.Execute(commandName)).Result;

                result.Should().BeOfType<AcceptedResult>();
                result.As<AcceptedResult>()
                .Value.Should().BeOfType<ExecuteCommandResponse>()
                .Which.ReplyImmediately.Should().BeFalse();

                await runner.Received().Run(Any<CommandContext>());
            }

            [Test, Auto]
            public async Task ShouldReturnOkWithResult(
                string commandName,
                CommandResult commandOutput,
                HttpContext httpContext,
                ExecuteCommandRequest request,
                [Frozen] ICommandRunner command,
                [Frozen, Substitute] ICommandLoader loader,
                [Target] CommandController controller
            )
            {
                command.Run(Any<CommandContext>(), Any<CancellationToken>()).Returns(commandOutput);
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                var result = (await controller.Execute(commandName)).Result;

                var response = result.Should().BeOfType<OkObjectResult>().Which
                .Value.Should().BeOfType<ExecuteCommandResponse>()
                .Which;

                response.ReplyImmediately.Should().BeTrue();
                response.Response.Should().Be(commandOutput.Message);
            }

            [Test, Auto]
            public async Task ShouldReturnForbiddenIfUserDoesntHaveAccessToCommand(
                string commandName,
                CommandResult commandOutput,
                HttpContext httpContext,
                ExecuteCommandRequest request,
                [Frozen] Command command,
                [Frozen] ICommandRunner runner,
                [Frozen, Substitute] ICommandService service,
                [Target] CommandController controller
            )
            {
                runner.Run(Any<CommandContext>(), Any<CancellationToken>()).Returns(commandOutput);
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                service.When(svc => svc.GetByName(Any<string>(), Any<ClaimsPrincipal>(), Any<CancellationToken>())).Throw(new CommandRequiresRoleException(command));

                var result = (await controller.Execute(commandName)).Result;

                result.Should().BeOfType<ForbidResult>();
            }

            [Test, Auto]
            public async Task ShouldReturnNotFoundIfTheCommandWasNotFound(
                string commandName,
                HttpContext httpContext,
                ExecuteCommandRequest request,
                [Frozen, Substitute] ICommandService service,
                [Target] CommandController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                service.GetByName(Any<string>(), Any<ClaimsPrincipal>(), Any<CancellationToken>()).Throws(new CommandNotFoundException(commandName));

                var result = (await controller.Execute(commandName)).Result;

                result.Should().BeOfType<NotFoundResult>();
            }
        }
    }
}
