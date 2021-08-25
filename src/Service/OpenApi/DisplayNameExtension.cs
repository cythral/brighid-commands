using Microsoft.OpenApi;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Writers;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// Writes display name for properties.
    /// </summary>
    public class DisplayNameExtension : IOpenApiExtension
    {
        private readonly string displayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayNameExtension" /> class.
        /// </summary>
        /// <param name="displayName">The display name to write.</param>
        public DisplayNameExtension(
            string displayName
        )
        {
            this.displayName = displayName;
        }

        /// <inheritdoc />
        public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
        {
            writer.WriteRaw($"\"{displayName}\"");
        }
    }
}
