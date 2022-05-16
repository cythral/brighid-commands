using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Commands.Sdk;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Commands.Service
{
    public class DefaultCommandLoaderTests
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
                [Frozen, Substitute] ICommandPackageDownloader downloader,
                [Frozen, Substitute] IUtilsFactory utilsFactory,
                [Target] DefaultCommandLoader loader,
                CancellationToken cancellationToken
            )
            {
                using var responseStream = new MemoryStream(Encoding.UTF8.GetBytes(contents));
                assembly.GetType(Any<string>(), Any<bool>()).Returns(typeof(TestCommandRegistrator));

                await loader.LoadEmbedded(command, cancellationToken);

                var location = command.EmbeddedLocation!.Value;

                await downloader.Received().DownloadCommandPackageFromS3(Is(location.DownloadURL), Is(location.AssemblyName), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldCacheCommands(
                string contents,
                Command command,
                MemoryStream fileStream,
                [Frozen] S3UriInfo s3UriInfo,
                [Frozen, Substitute] Assembly assembly,
                [Frozen, Substitute] ICommandPackageDownloader downloader,
                [Frozen, Substitute] IUtilsFactory utilsFactory,
                [Target] DefaultCommandLoader loader,
                CancellationToken cancellationToken
            )
            {
                using var responseStream = new MemoryStream(Encoding.UTF8.GetBytes(contents));
                assembly.GetType(Any<string>(), Any<bool>()).Returns(typeof(TestCommandRegistrator));

                await loader.LoadEmbedded(command, cancellationToken);
                await loader.LoadEmbedded(command, cancellationToken);

                var location = command.EmbeddedLocation!.Value;
                await downloader.Received(1).DownloadCommandPackageFromS3(Is(location.DownloadURL), Is(location.AssemblyName), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldInvalidateCacheIfNewerCommandIsAvailable(
                string contents,
                Command command,
                MemoryStream fileStream,
                [Frozen] S3UriInfo s3UriInfo,
                [Frozen, Substitute] Assembly assembly,
                [Frozen, Substitute] ICommandPackageDownloader downloader,
                [Frozen, Substitute] IUtilsFactory utilsFactory,
                [Target] DefaultCommandLoader loader,
                CancellationToken cancellationToken
            )
            {
                using var responseStream = new MemoryStream(Encoding.UTF8.GetBytes(contents));
                assembly.GetType(Any<string>(), Any<bool>()).Returns(typeof(TestCommandRegistrator));

                command.Version = 1;
                await loader.LoadEmbedded(command, cancellationToken);

                command.Version = 2;
                await loader.LoadEmbedded(command, cancellationToken);

                var location = command.EmbeddedLocation!.Value;
                await downloader.Received(2).DownloadCommandPackageFromS3(Is(location.DownloadURL), Is(location.AssemblyName), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldNotInvalidateCacheIfOlderCommandVersionIsRequested(
                string contents,
                Command command,
                MemoryStream fileStream,
                [Frozen] S3UriInfo s3UriInfo,
                [Frozen, Substitute] Assembly assembly,
                [Frozen, Substitute] ICommandPackageDownloader downloader,
                [Frozen, Substitute] IUtilsFactory utilsFactory,
                [Target] DefaultCommandLoader loader,
                CancellationToken cancellationToken
            )
            {
                using var responseStream = new MemoryStream(Encoding.UTF8.GetBytes(contents));
                assembly.GetType(Any<string>(), Any<bool>()).Returns(typeof(TestCommandRegistrator));

                command.Version = 2;
                await loader.LoadEmbedded(command, cancellationToken);

                command.Version = 1;
                await loader.LoadEmbedded(command, cancellationToken);

                var location = command.EmbeddedLocation!.Value;
                await downloader.Received(1).DownloadCommandPackageFromS3(Is(location.DownloadURL), Is(location.AssemblyName), Is(cancellationToken));
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
                [Target] DefaultCommandLoader loader,
                CancellationToken cancellationToken
            )
            {
                assembly.GetType(Any<string>(), Any<bool>()).Returns((Type?)null);

                Func<Task> func = () => loader.LoadEmbedded(command, cancellationToken);

                await func.Should().ThrowAsync<CommandNotFoundException>();
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

            private class TestCommandRegistrator : ICommandRegistrator
            {
                public ICommandRunner Register(IServiceCollection services)
                {
                    return new TestCommandRunner();
                }
            }

            private class TestCommandRunner : ICommandRunner
            {
                public Task<CommandResult> Run(CommandContext context, CancellationToken cancellationToken = default)
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
