using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Amazon.S3;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Commands.Core;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Commands.Commands
{
    public class DefaultCommandServiceTests
    {
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

                await downloader.Received().DownloadCommandPackageFromS3(Is(command.DownloadURL!), Is(command.AssemblyName!), Is(cancellationToken));
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
