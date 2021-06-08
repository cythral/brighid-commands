using System;
using System.Collections.Generic;

namespace Brighid.Commands.Core
{
    /// <summary>
    /// Context holder for executing commands.
    /// </summary>
    public class CommandContext
    {
        /// <summary>
        /// Gets or sets the arguments used to run the command.
        /// </summary>
        public string[] Arguments { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the options used to run the command.
        /// </summary>
        public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object>();
    }
}
