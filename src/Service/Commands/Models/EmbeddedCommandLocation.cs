namespace Brighid.Commands.Commands
{
    /// <summary>
    /// Represents the location of an embedded command.
    /// </summary>
    public class EmbeddedCommandLocation
    {
        /// <summary>
        /// Gets or sets the URL where the command's package can be downloaded from.
        /// </summary>
        public string? DownloadURL { get; set; }

        /// <summary>
        /// Gets or sets the name of the assembly within the package that the command lives in.
        /// </summary>
        public string? AssemblyName { get; set; }

        /// <summary>
        /// Gets or sets the fully-qualified name of the command type within the assembly.
        /// </summary>
        public string? TypeName { get; set; }

        /// <summary>
        /// Gets or sets the checksum of the command's contents.
        /// </summary>
        public string? Checksum { get; set; }
    }
}
