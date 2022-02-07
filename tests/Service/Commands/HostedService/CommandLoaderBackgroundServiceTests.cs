using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Commands.Service
{
    public class CommandLoaderBackgroundServiceTests
    {
        [TestFixture]
        public class StartAsyncTests
        {
            [Test, Auto]
            public async Task ShouldLoadAllEmbeddedCommands(
                [Frozen, Substitute] ICommandService service,
                [Target] CommandLoaderBackgroundService loader,
                CancellationToken cancellationToken
            )
            {
                await loader.StartAsync(cancellationToken);

                await service.Received().LoadAllEmbeddedCommands(Is(cancellationToken));
            }
        }
    }
}
