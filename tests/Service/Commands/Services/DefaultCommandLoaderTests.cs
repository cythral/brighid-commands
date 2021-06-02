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

namespace Brighid.Commands.Commands
{
    public class DefaultCommandLoaderTests
    {
        [TestFixture]
        public class LoadCommandByNameTests
        {
            [Test, Auto]
            public async Task ShouldFetchTheCommandFromTheRepository(
                string name,
                [Frozen, Substitute] ICommandRepository repository,
                [Target] DefaultCommandLoader loader,
                CancellationToken cancellationToken
            )
            {
                await loader.LoadCommandByName(name, cancellationToken);

                await repository.Received().FindCommandByName(Is(name), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldLoadEmbeddedCommands(
                string name,
                [Frozen] Command command,
                [Frozen] ICommandRunner runner,
                [Frozen, Substitute] ICommandService service,
                [Target] DefaultCommandLoader loader,
                CancellationToken cancellationToken
            )
            {
                command.Type = CommandType.Embedded;
                var result = await loader.LoadCommandByName(name, cancellationToken);

                result.Should().Be(command);
                result.Runner.Should().Be(runner);
                await service.Received().LoadEmbedded(Is(command), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldThrowForUnsupportedCommands(
                string name,
                [Frozen] Command command,
                [Frozen, Substitute] ICommandService service,
                [Target] DefaultCommandLoader loader,
                CancellationToken cancellationToken
            )
            {
                command.Type = (CommandType)(-1);
                Func<Task> func = () => loader.LoadCommandByName(name, cancellationToken);

                await func.Should().ThrowAsync<CommandTypeNotSupportedException>();
            }
        }
    }
}
