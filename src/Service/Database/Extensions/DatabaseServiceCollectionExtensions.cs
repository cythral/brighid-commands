using Brighid.Commands;
using Brighid.Commands.Database;

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
            var databaseOptions = configuration.GetSection("Database").Get<DatabaseOptions>() ?? new();
            var connectionString = databaseOptions.ToString();
            var version = Program.IsRunningViaEfTool
                ? new MySqlServerVersion(new System.Version(7, 0))
                : ServerVersion.AutoDetect(connectionString);

            services.AddDbContextPool<DatabaseContext>(options =>
            {
                options
                .UseMySql(connectionString, version);
            });
        }
    }
}
