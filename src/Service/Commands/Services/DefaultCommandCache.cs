using System.Collections.Concurrent;

using Brighid.Commands.Core;

namespace Brighid.Commands.Service
{
    /// <inheritdoc />
    public class DefaultCommandCache : ConcurrentDictionary<string, ICommandRunner>, ICommandCache
    {
    }
}
