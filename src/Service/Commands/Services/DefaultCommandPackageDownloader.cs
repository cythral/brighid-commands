using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Amazon.S3;

using Microsoft.Extensions.Options;

namespace Brighid.Commands.Commands
{
    /// <inheritdoc />
    public class DefaultCommandPackageDownloader : ICommandPackageDownloader
    {
        private readonly IAmazonS3 s3Client;
        private readonly IUtilsFactory utilsFactory;
        private readonly ServiceOptions serviceOptions;
        private readonly ConcurrentDictionary<string, string> downloadedUrls = new();
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
        /// <todo>Handle concurrency between multiple threads wanting to download the same package.</todo>
        public async Task<Assembly> DownloadCommandPackageFromS3(string s3Uri, string assemblyName, CancellationToken cancellationToken)
        {
            if (loadedAssemblies.TryGetValue(s3Uri + assemblyName, out var assembly))
            {
                return assembly;
            }

            if (!downloadedUrls.TryGetValue(s3Uri, out var destination))
            {
                var uriInfo = ParseS3Uri(s3Uri);
                var response = await s3Client.GetObjectAsync(uriInfo.Bucket, uriInfo.Key, cancellationToken);
                var identifier = utilsFactory.CreateGuid();
                var zipFilePath = $"{serviceOptions.EmbeddedCommandsDirectory}/{identifier}.zip";
                destination = $"{serviceOptions.EmbeddedCommandsDirectory}/{identifier}";

                await utilsFactory.CreateFileFromStream(response.ResponseStream, zipFilePath, cancellationToken);
                utilsFactory.ExtractZipFile(zipFilePath, destination);
                downloadedUrls.TryAdd(s3Uri, destination);
            }

            assembly = utilsFactory.LoadAssemblyFromFile(assemblyName, $"{destination}/{assemblyName}.dll");
            loadedAssemblies.TryAdd(s3Uri + assemblyName, assembly);
            return assembly;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static S3UriInfo ParseS3Uri(string s3Uri)
        {
            var parts = s3Uri.Replace("s3://", string.Empty).Split('/');
            return new S3UriInfo
            {
                Bucket = parts.First(),
                Key = string.Join('/', parts.Skip(1)),
            };
        }
    }
}
