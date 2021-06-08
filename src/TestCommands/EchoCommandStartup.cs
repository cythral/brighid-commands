using Brighid.Commands.Core;

using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Commands.TestCommands
{
    /// <inheritdoc />
    public class EchoCommandStartup : ICommandStartup
    {
        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services)
        {
        }
    }
}
