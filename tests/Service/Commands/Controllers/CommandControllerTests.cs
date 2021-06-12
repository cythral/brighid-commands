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
        public class GetCommandParseInfo
        {
            [Test, Auto]
            public async Task ShouldReturnArgCount(
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

                var result = (await controller.GetCommandParserRestrictions(name)).Result;

                result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeOfType<CommandParserRestrictions>()
                .Which.ArgCount.Should().Be(argCount);

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

                var result = (await controller.GetCommandParserRestrictions(name)).Result;

                var validOptions = result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeOfType<CommandParserRestrictions>()
                .Which.ValidOptions;

                validOptions.Should().Contain(option1);
                validOptions.Should().Contain(option2);
                await repository.Received().FindCommandByName(Is(name), Is(httpContext.RequestAborted));
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

                var result = (await controller.GetCommandParserRestrictions(name)).Result;

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

                var result = (await controller.GetCommandParserRestrictions(name)).Result;

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
                ExecuteCommandRequest request,
                [Frozen, Substitute] ICommandRepository repository,
                [Target] CommandController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                await controller.Execute(commandName, request);

                await repository.Received().FindCommandByName(Is(commandName), Is(httpContext.RequestAborted));
            }

            [Test, Auto]
            public async Task ShouldLoadTheCommandByName(
                string commandName,
                HttpContext httpContext,
                ExecuteCommandRequest request,
                [Frozen] Command command,
                [Frozen, Substitute] ICommandLoader loader,
                [Target] CommandController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                await controller.Execute(commandName, request);

                await loader.Received().LoadCommand(Is(command), Is(httpContext.RequestAborted));
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
                await controller.Execute(commandName, request);

                await runner.Received().Run(
                    Is<CommandContext>(context =>
                        context.Options == request.Options &&
                        context.Arguments == request.Arguments
                    ),
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
                var result = (await controller.Execute(commandName, request)).Result;

                result.Should().BeOfType<AcceptedResult>();
                result.As<AcceptedResult>()
                .Value.Should().BeOfType<ExecuteCommandResponse>()
                .Which.ReplyImmediately.Should().BeFalse();

                await runner.Received().Run(Any<CommandContext>());
            }

            [Test, Auto]
            public async Task ShouldReturnOkWithResult(
                string commandName,
                string commandOutput,
                HttpContext httpContext,
                ExecuteCommandRequest request,
                [Frozen] ICommandRunner command,
                [Frozen, Substitute] ICommandLoader loader,
                [Target] CommandController controller
            )
            {
                command.Run(Any<CommandContext>(), Any<CancellationToken>()).Returns(commandOutput);
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                var result = (await controller.Execute(commandName, request)).Result;

                var response = result.Should().BeOfType<OkObjectResult>().Which
                .Value.Should().BeOfType<ExecuteCommandResponse>()
                .Which;

                response.ReplyImmediately.Should().BeTrue();
                response.Response.Should().Be(commandOutput);
            }

            [Test, Auto]
            public async Task ShouldReturnForbiddenIfUserDoesntHaveAccessToCommand(
                string commandName,
                string commandOutput,
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

                service.When(svc => svc.EnsureCommandIsAccessibleToPrincipal(Any<Command>(), Any<ClaimsPrincipal>())).Throw(new CommandRequiresRoleException(command));

                var result = (await controller.Execute(commandName, request)).Result;

                result.Should().BeOfType<ForbidResult>();
            }
        }
    }
}
