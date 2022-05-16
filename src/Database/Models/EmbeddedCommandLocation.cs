using System.Text.Json;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// Represents the location of an embedded command.
    /// </summary>
    public readonly struct EmbeddedCommandLocation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedCommandLocation" /> struct.
        /// </summary>
        /// <param name="downloadUrl">The URL where the command's package can be downloaded from.</param>
        /// <param name="assemblyName">The name of the assembly within the package that the command lives in.</param>
        /// <param name="typeName">The fully-qualified name of the command type within the assembly.</param>
        /// <param name="checksum">The checksum of the command's contents.</param>
        public EmbeddedCommandLocation(
            string downloadUrl,
            string assemblyName,
            string typeName,
            string checksum
        )
        {
            DownloadURL = downloadUrl;
            AssemblyName = assemblyName;
            TypeName = typeName;
            Checksum = checksum;
        }

        /// <summary>
        /// Gets the URL where the command's package can be downloaded from.
        /// </summary>
        public readonly string DownloadURL { get; init; }

        /// <summary>
        /// Gets the name of the assembly within the package that the command lives in.
        /// </summary>
        public readonly string AssemblyName { get; init; }

        /// <summary>
        /// Gets the fully-qualified name of the command type within the assembly.
        /// </summary>
        public readonly string TypeName { get; init; }

        /// <summary>
        /// Gets the checksum of the command's contents.
        /// </summary>
        public readonly string Checksum { get; init; }

        /// <summary>
        /// Converts an EmbeddedCommandLocation to a string and vice versa.
        /// </summary>
        public class Converter : ValueConverter<EmbeddedCommandLocation?, string>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Converter" /> class.
            /// </summary>
            public Converter()
                : base(
                    location => JsonSerializer.Serialize(location, (JsonSerializerOptions?)null),
                    @string => JsonSerializer.Deserialize<EmbeddedCommandLocation>(@string, (JsonSerializerOptions?)null)
                )
            {
            }
        }
    }
}
