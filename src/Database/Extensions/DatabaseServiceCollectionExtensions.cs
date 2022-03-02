using Brighid.Commands;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// IServiceCollection Extensions for Database.
    /// </summary>
    public static class DatabaseServiceCollectionExtensions
    {
        /// <summary>
        /// Configures the service collection to use request services.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configuration">Application configuration object.</param>
        public static void ConfigureDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContextPool<DatabaseContext>(optionsBuilder =>
            {
                var factory = new DatabaseContextFactory(configuration, ServerVersion.AutoDetect, optionsBuilder);
                factory.Configure();
            });
        }
    }
}
