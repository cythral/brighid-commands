using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Sdk;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Brighid.Commands.Service
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
        /// Gets or sets the version of this command.
        /// </summary>
        public BigInteger Version { get; set; } = 1;

        /// <summary>
        /// Gets or sets the name of the command.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the role users need to execute this command.
        /// </summary>
        public string? RequiredRole { get; set; }

        /// <summary>
        /// Gets or sets the description of the command.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the location of an embedded command.
        /// </summary>
        public EmbeddedCommandLocation? EmbeddedLocation { get; set; }

        /// <summary>
        /// Gets or sets the command's parameters.
        /// </summary>
        public ICollection<CommandParameter> Parameters { get; set; } = Array.Empty<CommandParameter>();

        /// <summary>
        /// Gets or sets the command's scopes.
        /// </summary>
        public ICollection<string> Scopes { get; set; } = new HashSet<string>();

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
        public virtual Task<CommandResult> Run(CommandContext context, CancellationToken cancellationToken = default)
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
                .Property(command => command.Version)
                .HasConversion(new ValueConverter<BigInteger, string>(
                    bigInt => bigInt.ToString(),
                    @string => BigInteger.Parse(@string)
                ));

                builder
                .Property(command => command.ValidOptions)
                .HasConversion(new ValueConverter<List<string>, string>(
                    list => string.Join(';', list),
                    @string => @string.Split(';', StringSplitOptions.TrimEntries).ToList()
                ));

                builder
                .Property(command => command.EmbeddedLocation)
                .HasConversion<EmbeddedCommandLocation.Converter>();

                builder
                .Property(command => command.Parameters)
                .HasConversion(new ValueConverter<ICollection<CommandParameter>, string>(
                    parameter => JsonSerializer.Serialize(parameter, (JsonSerializerOptions?)null),
                    @string => JsonSerializer.Deserialize<CommandParameter[]>(@string, (JsonSerializerOptions?)null) ?? Array.Empty<CommandParameter>()
                ));

                builder
                .Property(command => command.Scopes)
                .HasConversion<Scope.Converter, Scope.Comparer>();
            }

            private class Scope
            {
                public class Converter : ValueConverter<ICollection<string>, string>
                {
                    public Converter()
                        : base(
                            scopes => string.Join(',', scopes),
                            @string => @string.Split(',', StringSplitOptions.None).ToHashSet()
                        )
                    {
                    }
                }

                public class Comparer : ValueComparer<ICollection<string>>
                {
                    public Comparer()
                        : base(
                            (a, b) => CollectionEqual(a, b),
                            (scopes) => scopes
                                .OrderBy(scopes => scopes)
                                .Aggregate(0, (hashCode, scope) => HashCode.Combine(hashCode, scope.GetHashCode()))
                        )
                    {
                    }

                    private static bool CollectionEqual(ICollection<string>? left, ICollection<string>? right)
                    {
                        var sortedLeft = left?.OrderBy(left => left) ?? (IEnumerable<string>)Array.Empty<string>();
                        var sortedRight = right?.OrderBy(right => right) ?? (IEnumerable<string>)Array.Empty<string>();

                        return sortedLeft.SequenceEqual(sortedRight);
                    }
                }
            }
        }
    }
}
