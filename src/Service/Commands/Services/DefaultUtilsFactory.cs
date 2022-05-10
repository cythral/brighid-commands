using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Sdk;

using McMaster.NETCore.Plugins;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Brighid.Commands.Service
{
    /// <inheritdoc />
    public class DefaultUtilsFactory : IUtilsFactory
    {
        /// <inheritdoc />
        public async Task CreateFileFromStream(Stream sourceStream, string filename, CancellationToken cancellationToken)
        {
            using var fileStream = File.OpenWrite(filename);
            await sourceStream.CopyToAsync(fileStream, cancellationToken);
            await sourceStream.FlushAsync(cancellationToken);
        }

        /// <inheritdoc />
        public S3UriInfo CreateS3UriInfo(string s3Uri)
        {
            var parts = s3Uri.Replace("s3://", string.Empty).Split('/');
            return new S3UriInfo
            {
                Bucket = parts.First(),
                Key = string.Join('/', parts.Skip(1)),
            };
        }

        /// <inheritdoc />
        public IServiceCollection CreateServiceCollection()
        {
            return new ServiceCollection();
        }

        /// <inheritdoc />
        public Assembly LoadAssemblyFromFile(string name, string location)
        {
            var absolutePath = Path.GetFullPath(location);
            var result = PluginLoader.CreateFromAssemblyFile(
                assemblyFile: absolutePath,
                sharedTypes: new[]
                {
                    typeof(IServiceCollection),
                    typeof(ICommandRegistrator),
                    typeof(ILogger),
                    typeof(IConfiguration),
                    typeof(ConfigurationBinder),
                    typeof(ResourceWriter),
                },
                isUnloadable: true
            );

            return result.LoadDefaultAssembly();
        }

        /// <inheritdoc />
        public void ExtractZipFile(string zipFile, string directory)
        {
            Directory.CreateDirectory(directory);
            ZipFile.ExtractToDirectory(zipFile, directory);
        }

        /// <inheritdoc />
        public Guid CreateGuid()
        {
            return Guid.NewGuid();
        }
    }
}
