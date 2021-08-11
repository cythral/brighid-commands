using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Core;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
        /// Gets or sets the ID of the command owner.
        /// </summary>
        public Guid OwnerId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the type of command this is.
        /// </summary>
        public CommandType Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the command.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the role users need to execute this command.
        /// </summary>
        public string? RequiredRole { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the command.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the location of an embedded command.
        /// </summary>
        public EmbeddedCommandLocation? EmbeddedLocation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this command is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of arguments the command has.
        /// </summary>
        public uint ArgCount { get; set; }

        /// <summary>
        /// Gets or sets a list of valid options.
        /// </summary>
        public List<string> ValidOptions { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the command runner.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
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
                builder
                .Property(command => command.ValidOptions)
                .HasConversion(new ValueConverter<List<string>, string>(
                    list => string.Join(';', list),
                    @string => @string.Split(';', StringSplitOptions.TrimEntries).ToList()
                ));

                builder
                .Property(command => command.EmbeddedLocation)
                .HasConversion(new ValueConverter<EmbeddedCommandLocation?, string>(
                    location => JsonSerializer.Serialize(location, null),
                    @string => JsonSerializer.Deserialize<EmbeddedCommandLocation>(@string, null)
                ));
            }
        }
    }
}
