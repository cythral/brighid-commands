using System;
using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Brighid.Commands.Commands
{
    /// <summary>
    /// Represents a command that can be executed.
    /// </summary>
    public class Command
    {
        /// <summary>
        /// Gets or sets the ID of the command.
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the type of command this is.
        /// </summary>
        public CommandType Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the command.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the checksum of the command's contents.
        /// </summary>
        public string? Checksum { get; set; }

        /// <summary>
        /// Gets or sets the description of the command.
        /// </summary>
        public string? Description { get; set; }

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
        /// Gets or sets a value indicating whether or not this command is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <inheritdoc />
        public class EntityConfig : IEntityTypeConfiguration<Command>
        {
            /// <inheritdoc />
            public void Configure(EntityTypeBuilder<Command> builder)
            {
                builder.HasIndex(bucket => bucket.Name).IsUnique();
            }
        }
    }
}
