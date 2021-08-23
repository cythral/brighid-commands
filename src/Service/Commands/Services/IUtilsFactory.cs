using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// Factory for creating various utilities.
    /// </summary>
    public interface IUtilsFactory
    {
        /// <summary>
        /// Creates a new file stream that can be written to.
        /// </summary>
        /// <param name="sourceStream">The source stream to copy to the new file.</param>
        /// <param name="filename">The name of the file to open a stream for.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting file stream.</returns>
        Task CreateFileFromStream(Stream sourceStream, string filename, CancellationToken cancellationToken = default);

        /// <summary>
        /// Constructs a <see cref="S3UriInfo" /> class from the given <paramref name="s3Uri" />.
        /// </summary>
        /// <param name="s3Uri">The S3 URI to parse and get info for.</param>
        /// <returns>The resulting S3 URI Info.</returns>
        S3UriInfo CreateS3UriInfo(string s3Uri);

        /// <summary>
        /// Loads an assembly from the given <paramref name="location" />.
        /// </summary>
        /// <param name="name">The name of the assembly.</param>
        /// <param name="location">The path to the assembly.</param>
        /// <returns>The resulting assembly.</returns>
        Assembly LoadAssemblyFromFile(string name, string location);

        /// <summary>
        /// Creates a new Service Collection.
        /// </summary>
        /// <returns>The resulting service collection.</returns>
        IServiceCollection CreateServiceCollection();

        /// <summary>
        /// Creates a new <see cref="ICommandClrType" /> abstraction over the given <paramref name="commandType" />.
        /// </summary>
        /// <param name="commandType">The command's real type to abstract over.</param>
        /// <param name="name">The name of the command.</param>
        /// <returns>The abstracted command clr type.</returns>
        ICommandClrType CreateCommandClrType(Type commandType, string name);

        /// <summary>
        /// Extract a zip file to the given <paramref name="directory" />.
        /// </summary>
        /// <param name="zipFile">Zip file to extract.</param>
        /// <param name="directory">Directory to extract the zip file to.</param>
        void ExtractZipFile(string zipFile, string directory);

        /// <summary>
        /// Generate a new GUID.
        /// </summary>
        /// <returns>The resulting GUID.</returns>
        Guid CreateGuid();
    }
}
