using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Brighid.Commands.Database
{
    /// <inheritdoc />
    public class DesignTimeDatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        /// <inheritdoc />
        public DatabaseContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

            var databaseOptions = configuration.GetSection("Database").Get<DatabaseOptions>();
            optionsBuilder
            .UseMySql(
                databaseOptions.ToString(),
                new MySqlServerVersion(new Version(5, 7, 7))
            );

            return new DatabaseContext(optionsBuilder.Options);
        }
    }
}
