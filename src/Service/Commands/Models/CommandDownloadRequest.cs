using System.Threading;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// Represents a request to download a command.
    /// </summary>
    public readonly struct CommandDownloadRequest
    {
        /// <summary>
        /// Gets the assembly name.
        /// </summary>
        public string AssemblyName { get; init; }

        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        public CancellationToken CancellationToken { get; init; }
    }
}
