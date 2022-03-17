using System.Numerics;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// A response to an Execute Command Request.
    /// </summary>
    public class ExecuteCommandResponse
    {
        /// <summary>
        /// Gets or sets the response the command returned (not always set depending on command type.)
        /// </summary>
        public string? Response { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the client should reply immediately with the response.
        /// </summary>
        public bool ReplyImmediately { get; set; }

        /// <summary>
        /// Gets or sets the version number of the command that was executed.
        /// </summary>
        public BigInteger Version { get; set; }
    }
}
