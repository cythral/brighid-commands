using System;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// Abstraction for a Command's CLR Type.
    /// </summary>
    public interface ICommandClrType
    {
        /// <summary>
        /// Gets the command's startup type.
        /// </summary>
        Type StartupType { get; }

        /// <summary>
        /// Gets the command's underlying type.
        /// </summary>
        Type CommandType { get; }
    }
}
