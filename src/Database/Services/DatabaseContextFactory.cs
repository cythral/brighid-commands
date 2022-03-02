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
        /// <param name="configuration">Configuration to use for the database.</param>
        /// <param name="detectVersion">Function used to detect the MySQL version to use.</param>
        /// <param name="dbOptionsBuilder">Options builder to configure options on.</param>
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
        /// Configures the dbOptionsBuilder.
        /// </summary>
        public void Configure()
        {
            var conn = options.ToString();
            var version = detectVersion(conn);
            dbOptionsBuilder.UseMySql(conn, version);
        }

        /// <summary>
        /// Configures the dbOptionsBuilder and creates a new database context.
        /// </summary>
        /// <param name="args">This parameter is unused.</param>
        /// <returns>The resulting database context.</returns>
        public DatabaseContext CreateDbContext(string[] args)
        {
            Configure();
            return new DatabaseContext(dbOptionsBuilder.Options);
        }
    }
}
