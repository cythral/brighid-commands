using System.ComponentModel.DataAnnotations;

using Lambdajection.Attributes;

namespace Brighid.Commands.MigrationsRunner
{
    /// <summary>
    /// Represents a request to run database migrations.
    /// </summary>
    public class MigrationsRequest
    {
        /// <summary>
        /// Gets or sets the serial - change this to trigger migrations to run again.
        /// </summary>
        [Required]
        [UpdateRequiresReplacement]
        public string Serial { get; set; } = string.Empty;
    }
}
