using System.Collections.Concurrent;

using Brighid.Commands.Sdk;

namespace Brighid.Commands.Service
{
    /// <inheritdoc />
    public class DefaultCommandCache : ConcurrentDictionary<string, ICommandRunner>, ICommandCache
    {
    }
}
