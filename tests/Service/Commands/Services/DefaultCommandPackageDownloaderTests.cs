using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Amazon.S3;
using Amazon.S3.Model;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Commands.Service
{
    public class DefaultCommandPackageDownloaderTests
    {
        [TestFixture]
        public class DownloadCommandPackageFromS3Tests
        {
            [Test, Auto]
            public async Task ShouldDownloadTheZipFromS3(
                string bucket,
                string key,
                string assemblyName,
                [Frozen, Substitute] IAmazonS3 s3Client,
                [Target] DefaultCommandPackageDownloader downloader,
                CancellationToken cancellationToken
            )
            {
                var uri = $"s3://{bucket}/{key}";
                await downloader.DownloadCommandPackageFromS3(uri, assemblyName, cancellationToken);

                await s3Client.Received().GetObjectAsync(Is(bucket), Is(key), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldNotDownloadZipsMultipleTimes(
                string bucket,
                string key,
                string assemblyName,
                [Frozen, Substitute] IAmazonS3 s3Client,
                [Target] DefaultCommandPackageDownloader downloader,
                CancellationToken cancellationToken
            )
            {
                var uri = $"s3://{bucket}/{key}";
                await downloader.DownloadCommandPackageFromS3(uri, assemblyName, cancellationToken);
                await downloader.DownloadCommandPackageFromS3(uri, assemblyName, cancellationToken);

                await s3Client.Received(1).GetObjectAsync(Is(bucket), Is(key), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldSaveTheZipToAFile(
                string bucket,
                string key,
                string assemblyName,
                [Frozen] Guid zipId,
                [Frozen] ServiceOptions serviceOptions,
                [Frozen] GetObjectResponse response,
                [Frozen, Substitute] IAmazonS3 s3Client,
                [Frozen, Substitute] IUtilsFactory utilsFactory,
                [Target] DefaultCommandPackageDownloader downloader,
                CancellationToken cancellationToken
            )
            {
                var uri = $"s3://{bucket}/{key}";
                await downloader.DownloadCommandPackageFromS3(uri, assemblyName, cancellationToken);

                await utilsFactory.Received().CreateFileFromStream(Is(response.ResponseStream), Is($"{serviceOptions.EmbeddedCommandsDirectory}/{zipId}.zip"), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldExtractTheZipFile(
                string bucket,
                string key,
                string assemblyName,
                [Frozen] Guid zipId,
                [Frozen] ServiceOptions serviceOptions,
                [Frozen] GetObjectResponse response,
                [Frozen, Substitute] IAmazonS3 s3Client,
                [Frozen, Substitute] IUtilsFactory utilsFactory,
                [Target] DefaultCommandPackageDownloader downloader,
                CancellationToken cancellationToken
            )
            {
                var uri = $"s3://{bucket}/{key}";
                await downloader.DownloadCommandPackageFromS3(uri, assemblyName, cancellationToken);

                utilsFactory.Received().ExtractZipFile(Is($"{serviceOptions.EmbeddedCommandsDirectory}/{zipId}.zip"), Is($"{serviceOptions.EmbeddedCommandsDirectory}/{zipId}"));
            }

            [Test, Auto]
            public async Task ShouldLoadTheAssemblyFromTheDirectory(
                string bucket,
                string key,
                string assemblyName,
                [Frozen] Guid zipId,
                [Frozen] ServiceOptions serviceOptions,
                [Frozen] GetObjectResponse response,
                [Frozen, Substitute] IAmazonS3 s3Client,
                [Frozen, Substitute] IUtilsFactory utilsFactory,
                [Target] DefaultCommandPackageDownloader downloader,
                CancellationToken cancellationToken
            )
            {
                var uri = $"s3://{bucket}/{key}";
                await downloader.DownloadCommandPackageFromS3(uri, assemblyName, cancellationToken);

                utilsFactory.Received().ExtractZipFile(Is($"{serviceOptions.EmbeddedCommandsDirectory}/{zipId}.zip"), Is($"{serviceOptions.EmbeddedCommandsDirectory}/{zipId}"));
            }

            [Test, Auto]
            public async Task ShouldLoadTheAssemblyFromTheDirectory(
                string bucket,
                string key,
                string assemblyName,
                [Frozen] Guid zipId,
                [Frozen] ServiceOptions serviceOptions,
                [Frozen, Substitute] Assembly assembly,
                [Frozen] GetObjectResponse response,
                [Frozen, Substitute] IAmazonS3 s3Client,
                [Frozen, Substitute] IUtilsFactory utilsFactory,
                [Target] DefaultCommandPackageDownloader downloader,
                CancellationToken cancellationToken
            )
            {
                var uri = $"s3://{bucket}/{key}";
                var result = await downloader.DownloadCommandPackageFromS3(uri, assemblyName, cancellationToken);

                result.Should().BeSameAs(assembly);

                utilsFactory.Received().LoadAssemblyFromFile(Is(assemblyName), Is($"{serviceOptions.EmbeddedCommandsDirectory}/{zipId}/{assemblyName}.dll"));
            }

            [Test, Auto]
            public async Task ShouldNotLoadTheAssemblyFromTheDirectoryMultipleTimes(
                string bucket,
                string key,
                string assemblyName,
                [Frozen, Substitute] Assembly assembly,
                [Frozen] GetObjectResponse response,
                [Frozen, Substitute] IAmazonS3 s3Client,
                [Frozen, Substitute] IUtilsFactory utilsFactory,
                [Target] DefaultCommandPackageDownloader downloader,
                CancellationToken cancellationToken
            )
            {
                var uri = $"s3://{bucket}/{key}";
                var result = await downloader.DownloadCommandPackageFromS3(uri, assemblyName, cancellationToken);
                await downloader.DownloadCommandPackageFromS3(uri, assemblyName, cancellationToken);

                result.Should().BeSameAs(assembly);

                utilsFactory.Received(1).LoadAssemblyFromFile(Is(assemblyName), Any<string>());
            }
        }
    }
}
