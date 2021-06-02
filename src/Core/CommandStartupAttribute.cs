using System;

namespace Brighid.Commands.Core
{
    /// <summary>
    /// Attribute to indicate what type of startup class to use for a Command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandStartupAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandStartupAttribute" /> class.
        /// </summary>
        /// <param name="startupType">The type of startup class to use.</param>
        public CommandStartupAttribute(Type startupType)
        {
            StartupType = startupType;
        }

        /// <summary>
        /// Gets or sets the startup type of the command.
        /// </summary>
        public Type StartupType { get; set; }
    }
}
