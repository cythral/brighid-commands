using System;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// Restrictions to impart on command parsers for a particular command.
    /// </summary>
    public class CommandParserRestrictions
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
