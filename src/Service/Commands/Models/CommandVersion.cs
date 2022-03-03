using System.Numerics;

using Brighid.Commands.Sdk;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// Represents a version of a command.
    /// </summary>
    public class CommandVersion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandVersion"/> class.
        /// </summary>
        /// <param name="version">Version of the command.</param>
        /// <param name="runner">The command runner.</param>
        public CommandVersion(
            BigInteger version,
            ICommandRunner runner
        )
        {
            Version = version;
            Runner = runner;
        }

        /// <summary>
        /// Gets or sets the version number of the command.
        /// </summary>
        public BigInteger Version { get; set; }

        /// <summary>
        /// Gets or sets the runner for the command.
        /// </summary>
        public ICommandRunner Runner { get; set; }
    }
}
