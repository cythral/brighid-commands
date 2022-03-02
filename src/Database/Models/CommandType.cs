using System.Text.Json.Serialization;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// A type of command.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CommandType
    {
        /// <summary>
        /// Command that can be embedded into the command service (assembly can be downloaded directly into the command service).
        /// </summary>
        Embedded,
    }
}
