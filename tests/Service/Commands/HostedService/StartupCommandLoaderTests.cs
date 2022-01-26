using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Commands.Service
{
    public class StartupCommandLoaderTests
    {
        [TestFixture]
        public class StartAsyncTests
        {
            [Test, Auto]
            public async Task ShouldLoadAllEmbeddedCommands(
                [Frozen, Substitute] ICommandLoader commandLoader,
                [Target] StartupCommandLoader loader,
                CancellationToken cancellationToken
            )
            {
                await loader.StartAsync(cancellationToken);

                await commandLoader.Received().LoadAllEmbeddedCommands(Is(cancellationToken));
            }
        }
    }
}
