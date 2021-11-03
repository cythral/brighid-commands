using System.Collections.Generic;

using Brighid.Commands.Sdk;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// Cache for commands when looking up by name.
    /// </summary>
    public interface ICommandCache : IDictionary<string, ICommandRunner>
    {
    }
}
