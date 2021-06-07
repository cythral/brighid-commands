using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Commands.Core;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Commands.Commands
{
    public class CommandControllerTests
    {
        [TestFixture]
        public class GetCommandInfoHeadersTests
        {
            [Test, Auto]
            public async Task ShouldSetArgCountHeader(
                string name,
                uint argCount,
                HttpContext httpContext,
                [Frozen] Command command,
                [Frozen, Substitute] ICommandRepository repository,
                [Target] CommandController controller
            )
            {
                command.RequiredRole = null;
                command.ArgCount = argCount;
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                await controller.GetCommandInfoHeaders(name);

                httpContext.Response.Headers["x-command-argcount"].Should().Contain(argCount.ToString());
                await repository.Received().FindCommandByName(Is(name), Is(httpContext.RequestAborted));
            }

            [Test, Auto]
            public async Task ShouldSetValidOptionsHeader(
                string name,
                string option1,
                string option2,
                HttpContext httpContext,
                [Frozen] Command command,
                [Frozen, Substitute] ICommandRepository repository,
                [Target] CommandController controller
            )
            {
                command.RequiredRole = null;
                command.ValidOptions = new List<string> { option1, option2 };
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                await controller.GetCommandInfoHeaders(name);

                httpContext.Response.Headers["x-command-options"].Should().Contain(option1);
                httpContext.Response.Headers["x-command-options"].Should().Contain(option2);
                await repository.Received().FindCommandByName(Is(name), Is(httpContext.RequestAborted));
            }

            [Test, Auto]
            public async Task ShouldReturnOk(
                string name,
                uint argCount,
                HttpContext httpContext,
                [Frozen] Command command,
                [Frozen, Substitute] ICommandRepository repository,
                [Target] CommandController controller
            )
            {
                command.RequiredRole = null;
                command.ArgCount = argCount;
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var result = await controller.GetCommandInfoHeaders(name);

                result.Should().BeOfType<OkResult>();
            }

            [Test, Auto]
            public async Task ShouldReturnNotFoundIfCommandWasntFound(
                string name,
                HttpContext httpContext,
                [Frozen] Command command,
                [Frozen, Substitute] ICommandRepository repository,
                [Target] CommandController controller
            )
            {
                command.RequiredRole = null;
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                repository.FindCommandByName(Any<string>(), Any<CancellationToken>()).Throws(new CommandNotFoundException(name));

                var result = await controller.GetCommandInfoHeaders(name);

                result.Should().BeOfType<NotFoundResult>();
            }

            [Test, Auto]
            public async Task ShouldReturnForbiddenIfUserDoesntHavePermissionToUseCommand(
                string name,
                HttpContext httpContext,
                [Frozen] Command command,
                [Frozen, Substitute] ICommandService service,
                [Frozen, Substitute] ICommandRepository repository,
                [Target] CommandController controller
            )
            {
                command.RequiredRole = null;
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                service.When(svc => svc.EnsureCommandIsAccessibleToPrincipal(Any<Command>(), Any<ClaimsPrincipal>())).Throw(new CommandRequiresRoleException(command));

                var result = await controller.GetCommandInfoHeaders(name);

                result.Should().BeOfType<ForbidResult>();
                service.Received().EnsureCommandIsAccessibleToPrincipal(Is(command), Is(httpContext.User));
            }
        }

        [TestFixture]
        public class ExecuteTests
        {
            [Test, Auto]
            public async Task ShouldFindTheCommandInTheRepository(
                string commandName,
                HttpContext httpContext,
                [Frozen, Substitute] ICommandRepository repository,
                [Target] CommandController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                await controller.Execute(commandName);

                await repository.Received().FindCommandByName(Is(commandName), Is(httpContext.RequestAborted));
            }

            [Test, Auto]
            public async Task ShouldLoadTheCommandByName(
                string commandName,
                HttpContext httpContext,
                [Frozen] Command command,
                [Frozen, Substitute] ICommandLoader loader,
                [Target] CommandController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                await controller.Execute(commandName);

                await loader.Received().LoadCommand(Is(command), Is(httpContext.RequestAborted));
            }

            [Test, Auto]
            public async Task ShouldExecuteTheLoadedCommand(
                string commandName,
                HttpContext httpContext,
                [Frozen] ICommandRunner runner,
                [Frozen, Substitute] ICommandLoader loader,
                [Target] CommandController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                await controller.Execute(commandName);

                await runner.Received().Run(Any<CommandContext>(), Is(httpContext.RequestAborted));
            }

            [Test, Auto]
            public async Task ShouldReturnAcceptedIfCommandTypeIsNotEmbedded(
                string commandName,
                HttpContext httpContext,
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
                await runner.Received().Run(Any<CommandContext>());
            }

            [Test, Auto]
            public async Task ShouldReturnOkWithResult(
                string commandName,
                string commandOutput,
                HttpContext httpContext,
                [Frozen] ICommandRunner command,
                [Frozen, Substitute] ICommandLoader loader,
                [Target] CommandController controller
            )
            {
                command.Run(Any<CommandContext>(), Any<CancellationToken>()).Returns(commandOutput);
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                var result = (await controller.Execute(commandName)).Result;

                result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(commandOutput);
            }

            [Test, Auto]
            public async Task ShouldReturnForbiddenIfUserDoesntHaveAccessToCommand(
                string commandName,
                string commandOutput,
                HttpContext httpContext,
                [Frozen] Command command,
                [Frozen] ICommandRunner runner,
                [Frozen, Substitute] ICommandService service,
                [Target] CommandController controller
            )
            {
                runner.Run(Any<CommandContext>(), Any<CancellationToken>()).Returns(commandOutput);
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                service.When(svc => svc.EnsureCommandIsAccessibleToPrincipal(Any<Command>(), Any<ClaimsPrincipal>())).Throw(new CommandRequiresRoleException(command));

                var result = (await controller.Execute(commandName)).Result;

                result.Should().BeOfType<ForbidResult>();
            }
        }
    }
}
