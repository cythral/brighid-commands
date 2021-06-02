using System;
using System.Reflection;

using Brighid.Commands.Core;

namespace Brighid.Commands.Commands
{
    /// <inheritdoc />
    public class DefaultCommandClrType : ICommandClrType
    {
        private readonly string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommandClrType" /> class.
        /// </summary>
        /// <param name="innerType">The inner CLR type to proxy.</param>
        /// <param name="name">The name of the command.</param>
        public DefaultCommandClrType(
            Type innerType,
            string name
        )
        {
            CommandType = innerType;
            this.name = name;
        }

        /// <inheritdoc />
        public Type StartupType => CommandType.GetCustomAttribute<CommandStartupAttribute>()?.StartupType ?? throw new CommandMissingStartupException(name);

        /// <inheritdoc />
        public Type CommandType { get; }
    }
}
