using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Brighid.Commands
{
    /// <summary>
    /// Miscellaneous service options.
    /// </summary>
    public class ServiceOptions
    {
        /// <summary>
        /// Gets or sets the commands directory, where embedded commands get downloaded to.
        /// </summary>
        public string EmbeddedCommandsDirectory { get; set; } = "/var/brighid/commands";

        /// <summary>
        /// Gets or sets the port to use for the commands service.
        /// </summary>
        public int Port { get; set; } = 80;

        /// <summary>
        /// Gets or sets the protocols to use for the commands service.
        /// </summary>
        public HttpProtocols Protocols { get; set; } = HttpProtocols.Http2;
    }
}
