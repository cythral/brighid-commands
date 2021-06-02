using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Commands.Commands
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
        public ICommandClrType CreateCommandClrType(Type commandType, string name)
        {
            return new DefaultCommandClrType(commandType, name);
        }

        /// <inheritdoc />
        public Assembly LoadAssemblyFromFile(string name, string location)
        {
            var loadContext = new AssemblyLoadContext(name);
            var absolutePath = Path.GetFullPath(location);
            var assembly = loadContext.LoadFromAssemblyPath(absolutePath);
            return assembly;
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
