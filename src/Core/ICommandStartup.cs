using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Commands.Core
{
    /// <summary>
    /// Represents a configurator that is run when a command is loaded into the command service's load context.
    /// </summary>
    public interface ICommandStartup
    {
        /// <summary>
        /// Configures the services to use for the command.
        /// </summary>
        /// <param name="services">The collection of services to configure.</param>
        void ConfigureServices(IServiceCollection services);
    }
}
