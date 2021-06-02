using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Core;

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

        /// <summary>
        /// Gets or sets the command runner.
        /// </summary>
        [NotMapped]
        public ICommandRunner? Runner { get; set; }

        /// <summary>
        /// Invokes the command using it's set runner.
        /// </summary>
        /// <param name="context">The command context to use.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The command's output.</returns>
        public virtual Task<string> Run(CommandContext context, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Runner != null
                ? Runner.Run(context, cancellationToken)
                : throw new CommandNotRunnableException(this);
        }

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
