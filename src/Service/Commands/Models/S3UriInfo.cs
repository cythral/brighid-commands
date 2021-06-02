namespace Brighid.Commands.Commands
{
    /// <summary>
    /// Represents info about an S3 URI.
    /// </summary>
    public class S3UriInfo
    {
        /// <summary>
        /// Gets or sets the bucket in the S3 URI.
        /// </summary>
        public string Bucket { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the key in the S3 URI.
        /// </summary>
        public string Key { get; set; } = string.Empty;
    }
}
