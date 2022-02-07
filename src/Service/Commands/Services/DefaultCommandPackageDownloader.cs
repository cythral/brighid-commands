using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Amazon.S3;

using Microsoft.Extensions.Options;

namespace Brighid.Commands.Service
{
    /// <inheritdoc />
    public class DefaultCommandPackageDownloader : ICommandPackageDownloader
    {
        private readonly IAmazonS3 s3Client;
        private readonly IUtilsFactory utilsFactory;
        private readonly ServiceOptions serviceOptions;
        private readonly ConcurrentDictionary<string, Task<Assembly>> downloadTasks = new();
        private readonly ConcurrentDictionary<string, string> downloadedPackages = new();
        private readonly ConcurrentDictionary<string, Assembly> loadedAssemblies = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommandPackageDownloader"/> class.
        /// </summary>
        /// <param name="s3Client">Client for interacting with S3.</param>
        /// <param name="utilsFactory">Factory for various utilities.</param>
        /// <param name="serviceOptions">Options to use for the commands service.</param>
        public DefaultCommandPackageDownloader(
            IAmazonS3 s3Client,
            IUtilsFactory utilsFactory,
            IOptions<ServiceOptions> serviceOptions
        )
        {
            this.s3Client = s3Client;
            this.utilsFactory = utilsFactory;
            this.serviceOptions = serviceOptions.Value;
        }

        /// <inheritdoc />
        public async Task<Assembly> DownloadCommandPackageFromS3(string s3Uri, string assemblyName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (loadedAssemblies.TryGetValue(s3Uri + assemblyName, out var assembly))
            {
                return assembly;
            }

            var request = new CommandDownloadRequest { AssemblyName = assemblyName, CancellationToken = cancellationToken };
            var downloadTask = downloadTasks.GetOrAdd(s3Uri, Download, request);
            return await downloadTask;
        }

        private async Task<Assembly> Download(string s3Uri, CommandDownloadRequest request)
        {
            request.CancellationToken.ThrowIfCancellationRequested();

            if (!downloadedPackages.TryGetValue(s3Uri, out var destination))
            {
                var uriInfo = new Uri(s3Uri);
                var key = uriInfo.AbsolutePath.TrimStart('/');
                var response = await s3Client.GetObjectAsync(uriInfo.Host, key, request.CancellationToken);
                var identifier = utilsFactory.CreateGuid();
                var zipFilePath = $"{serviceOptions.EmbeddedCommandsDirectory}/{identifier}.zip";
                destination = $"{serviceOptions.EmbeddedCommandsDirectory}/{identifier}";

                await utilsFactory.CreateFileFromStream(response.ResponseStream, zipFilePath, request.CancellationToken);
                utilsFactory.ExtractZipFile(zipFilePath, destination);
                downloadedPackages.TryAdd(s3Uri, destination);
            }

            var assembly = utilsFactory.LoadAssemblyFromFile(request.AssemblyName, $"{destination}/{request.AssemblyName}.dll");
            loadedAssemblies.TryAdd(s3Uri + request.AssemblyName, assembly);
            downloadTasks.TryRemove(s3Uri, out _);
            return assembly;
        }
    }
}
