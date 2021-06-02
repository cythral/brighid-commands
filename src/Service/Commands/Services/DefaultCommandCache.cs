using System.Collections.Concurrent;

using Brighid.Commands.Core;

namespace Brighid.Commands.Commands
{
    /// <inheritdoc />
    public class DefaultCommandCache : ConcurrentDictionary<string, ICommandRunner>, ICommandCache
    {
    }
}
