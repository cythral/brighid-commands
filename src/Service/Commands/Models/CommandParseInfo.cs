using System;

namespace Brighid.Commands.Commands
{
    /// <summary>
    /// Information useful for parsing a command with.
    /// </summary>
    public class CommandParseInfo
    {
        /// <summary>
        /// Gets or sets the number of arguments the command has.
        /// </summary>
        public uint ArgCount { get; set; }

        /// <summary>
        /// Gets or sets the options that are valid for this command.
        /// </summary>
        public string[] ValidOptions { get; set; } = Array.Empty<string>();
    }
}
