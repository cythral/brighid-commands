using Brighid.Commands.Database;

using Destructurama;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;

namespace Brighid.Commands
{
    /// <summary>
    /// Configures the application when it starts up.
    /// </summary>
    public class Startup
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup" /> class.
        /// </summary>
        /// <param name="configuration">Configuration to use for the application.</param>
        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Destructure.UsingAttributes()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:s}] {Message:lj} {Properties:j} {Exception}{NewLine}")
                .CreateLogger();
        }

        /// <summary>
        /// Configures services for the application.
        /// </summary>
        /// <param name="services">Collection of services to configure.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.Configure<ServiceOptions>(configuration.GetSection("Commands"));
            services.AddHealthChecks();
            services.AddControllers();
            services.AddSwaggerGen();
            services.ConfigureDatabaseServices(configuration);
            services.ConfigureCommandServices();
        }

        /// <summary>
        /// Configures the environment.
        /// </summary>
        /// <param name="app">The application being configured.</param>
        /// <param name="environment">Environment used for the adapter.</param>
        /// <param name="databaseContext">Context used to interact with the database.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public void Configure(
            IApplicationBuilder app,
            IHostEnvironment environment,
            DatabaseContext databaseContext,
            ILogger<Startup> logger
        )
        {
            logger.LogInformation("Starting. Environment: {@environment}", environment.EnvironmentName);

            if (environment.IsEnvironment("local"))
            {
                databaseContext.Database.MigrateAsync().GetAwaiter().GetResult();
            }

            app.UseSwagger();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Brighid Commands Swagger"));

            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/healthcheck");
                endpoints.MapControllers();
            });
        }
    }
}
