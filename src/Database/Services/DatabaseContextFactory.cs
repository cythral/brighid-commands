using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Brighid.Commands
{
    /// <summary>
    /// Factory for creating database context instances, or simply configuring them.
    /// </summary>
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        private readonly DatabaseOptions options;
        private readonly Func<string, ServerVersion> detectVersion;
        private readonly DbContextOptionsBuilder dbOptionsBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseContextFactory" /> class.
        /// </summary>
        public DatabaseContextFactory()
            : this(
                new ConfigurationBuilder().AddEnvironmentVariables().Build(),
                (_) => new MySqlServerVersion(new Version(5, 7, 0)),
                new DbContextOptionsBuilder<DatabaseContext>()
            )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseContextFactory" /> class.
        /// </summary>
        /// <param name="configuration">Configuration values to use for the database connection.</param>
        /// <param name="detectVersion">Delegate used to detect the server version.</param>
        /// <param name="dbOptionsBuilder">Builder service to build the database options with.</param>
        public DatabaseContextFactory(
            IConfiguration configuration,
            Func<string, ServerVersion> detectVersion,
            DbContextOptionsBuilder dbOptionsBuilder
        )
        {
            options = configuration.GetSection("Database").Get<DatabaseOptions>() ?? new DatabaseOptions();
            this.detectVersion = detectVersion;
            this.dbOptionsBuilder = dbOptionsBuilder;
        }

        /// <summary>
        /// Configures the database options.
        /// </summary>
        /// <param name="configure">Delegate to configure custom properties for the database options.</param>
        public void Configure(Action<DbContextOptionsBuilder>? configure = null)
        {
            var conn = $"Server={options.Host};";
            conn += $"Database={options.Name};";
            conn += $"User={options.User};";
            conn += $"Password=\"{options.Password}\";";
            conn += "GuidFormat=Binary16;";
            conn += "UseCompression=true";

            var version = detectVersion(conn);
            dbOptionsBuilder.UseMySql(conn, version);

            configure?.Invoke(dbOptionsBuilder);
        }

        /// <summary>
        /// Creates the database context.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>The database context.</returns>
        public DatabaseContext CreateDbContext(string[] args)
        {
            Configure();
            return new DatabaseContext(dbOptionsBuilder.Options);
        }
    }
}
