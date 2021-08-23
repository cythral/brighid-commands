using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Commands.Core;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Commands.Service
{
    public class CommandTests
    {
        [TestFixture]
        public class RunTests
        {
            [Test, Auto]
            public async Task ShouldInvokeTheCommandRunnerWhenSet(
                CommandContext context,
                [Frozen, Substitute] ICommandRunner runner,
                [Target] Command command,
                CancellationToken cancellationToken
            )
            {
                command.Runner = runner;
                await command.Run(context, cancellationToken);

                await runner.Received().Run(Is(context), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldThrowCommandNotRunnableExceptionWhenNotSet(
                CommandContext context,
                [Frozen, Substitute] ICommandRunner runner,
                [Target] Command command,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => command.Run(context, cancellationToken);

                (await func.Should().ThrowAsync<CommandNotRunnableException>())
                .And.Command.Should().Be(command);
            }
        }
    }
}
