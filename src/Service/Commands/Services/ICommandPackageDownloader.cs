using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// Service for downloading command packages.
    /// </summary>
    public interface ICommandPackageDownloader
    {
        /// <summary>
        /// Downloads and loads the assembly from S3.
        /// </summary>
        /// <param name="s3Uri">URI of the package in S3.</param>
        /// <param name="assemblyName">Name of the assembly in the command package.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting assembly.</returns>
        Task<Assembly> DownloadCommandPackageFromS3(string s3Uri, string assemblyName, CancellationToken cancellationToken);
    }
}
