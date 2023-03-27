using System.Collections.Generic;
using System.Numerics;

using Brighid.Commands.Service;

using Destructurama;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using Serilog;
using Serilog.Events;

using Swashbuckle.AspNetCore.SwaggerGen;

using Environments = Brighid.Commands.Constants.Environments;

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
                .MinimumLevel.Override("DefaultHealthCheckService", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore.Server.Kestrel", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Warning)
                .Filter.ByExcluding("RequestPath = '/healthcheck' and (StatusCode = 200 or EventId.Name = 'ExecutingEndpoint' or EventId.Name = 'ExecutedEndpoint')")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:s}] {Message:lj} {Properties:j} {Exception}{NewLine}")
                .CreateLogger();
        }

        /// <summary>
        /// Configures services for the application.
        /// </summary>
        /// <param name="services">Collection of services to configure.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new BigIntegerJsonConverter());
            });

            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.Configure<ServiceOptions>(configuration.GetSection("Commands"));
            services.AddHealthChecks();
            services.AddRecaptchaService();
            services.AddSwaggerGen(ConfigureSwaggerGenOptions);
            services.ConfigureDatabaseServices(configuration);
            services.ConfigureCommandServices();
            services.ConfigureAuthServices(configuration.GetSection("Auth").Bind);
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
        }

        /// <summary>
        /// Configures Swagger Generation Options.
        /// </summary>
        /// <param name="options">The options to configure.</param>
        public void ConfigureSwaggerGenOptions(SwaggerGenOptions options)
        {
            options.SwaggerDoc($"v1", new OpenApiInfo { Title = "Brighid Commands", Version = ThisAssembly.AssemblyVersion });
            options.SchemaFilter<DisplayNameFilter>();
            options.OperationFilter<ExecuteCommandFilter>();
            options.MapType<BigInteger>(() => new OpenApiSchema { Type = "string" });
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
            databaseContext.Database.OpenConnection();

            if (environment.IsEnvironment(Environments.Local))
            {
                databaseContext.Database.Migrate();
            }

            app.UseXRay("Commands");
            app.UseForwardedHeaders();
            app.UseSwagger(options => options.PreSerializeFilters.Add(ConfigureSwaggerPreserializeFilter));
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Brighid Commands Swagger"));

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/healthcheck");
                endpoints.MapControllers();
            });
        }

        private void ConfigureSwaggerPreserializeFilter(OpenApiDocument document, HttpRequest request)
        {
            var server = new OpenApiServer { Url = $"https://{request.Host.Value}" };
            document.Servers = new List<OpenApiServer> { server };
        }
    }
}
