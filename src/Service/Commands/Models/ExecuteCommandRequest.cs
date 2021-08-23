using System;
using System.Collections.Generic;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// Represents a request to execute a command.
    /// </summary>
    public class ExecuteCommandRequest
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
