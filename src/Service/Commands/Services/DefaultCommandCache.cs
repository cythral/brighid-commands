using System.Collections.Concurrent;

namespace Brighid.Commands.Service
{
    /// <inheritdoc />
    public class DefaultCommandCache : ConcurrentDictionary<string, CommandVersion>, ICommandCache
    {
    }
}
