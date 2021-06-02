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
    }
}
