using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// Represents a parameter of a command.
    /// </summary>
    public struct CommandParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandParameter" /> struct.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="description">The description of the parameter.</param>
        /// <param name="type">The type of the parameter.</param>
        /// <param name="argumentIndex">The argument index.</param>
        public CommandParameter(
            string name,
            string? description = null,
            CommandParameterType type = default,
            byte? argumentIndex = null
        )
        {
            Name = name;
            Description = description;
            Type = type;
            ArgumentIndex = argumentIndex;
        }

        /// <summary>
        /// Gets the name of the command parameter.
        /// </summary>
        [JsonPropertyName("n")]
        [DisplayName(nameof(Name))]
        public string Name { get; init; }

        /// <summary>
        /// Gets the description of the command parameter.
        /// </summary>
        [JsonPropertyName("d")]
        [DisplayName(nameof(Description))]
        public string? Description { get; init; }

        /// <summary>
        /// Gets the type of the command parameter.
        /// </summary>
        [JsonPropertyName("t")]
        [DisplayName(nameof(Type))]
        public CommandParameterType Type { get; init; }

        /// <summary>
        /// Gets the argument index, if this parameter can be used as an argument.
        /// </summary>
        [JsonPropertyName("i")]
        [DisplayName(nameof(ArgumentIndex))]
        public byte? ArgumentIndex { get; init; }
    }
}
