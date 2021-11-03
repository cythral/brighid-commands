using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Commands.Sdk;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Commands.Service
{
    public class DefaultCommandLoaderTests
    {
        [TestFixture]
        public class LoadCommandByNameTests
        {
            [Test, Auto]
            public async Task ShouldLoadEmbeddedCommands(
                string name,
                Command command,
                [Frozen] ICommandRunner runner,
                [Frozen, Substitute] ICommandService service,
                [Target] DefaultCommandLoader loader,
                CancellationToken cancellationToken
            )
            {
                command.Type = CommandType.Embedded;
                await loader.LoadCommand(command, cancellationToken);

                command.Runner.Should().Be(runner);
                await service.Received().LoadEmbedded(Is(command), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldThrowForUnsupportedCommands(
                Command command,
                [Frozen, Substitute] ICommandService service,
                [Target] DefaultCommandLoader loader,
                CancellationToken cancellationToken
            )
            {
                command.Type = (CommandType)(-1);
                Func<Task> func = () => loader.LoadCommand(command, cancellationToken);

                await func.Should().ThrowAsync<CommandTypeNotSupportedException>();
            }
        }
    }
}
