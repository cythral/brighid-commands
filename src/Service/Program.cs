using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using Environments = Brighid.Commands.Constants.Environments;

#pragma warning disable SA1600, CS1591

namespace Brighid.Commands
{
    public class Program
    {
        public static bool IsRunningViaEfTool { get; private set; } = true;

        public static async Task Main(string[] args)
        {
            IsRunningViaEfTool = false;
            using var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
            .UseSerilog(dispose: true)
            .ConfigureHostConfiguration(ConfigureHostConfiguration)
            .UseDefaultServiceProvider(ConfigureServiceProvider)
            .ConfigureWebHostDefaults(ConfigureWebHostDefaults);
        }

        public static void ConfigureHostConfiguration(IConfigurationBuilder builder)
        {
            builder.AddEnvironmentVariables();
        }

        public static void ConfigureServiceProvider(HostBuilderContext context, ServiceProviderOptions options)
        {
            if (context.HostingEnvironment.IsEnvironment(Environments.Local) || context.HostingEnvironment.IsEnvironment(Environments.Dev))
            {
                options.ValidateScopes = true;
                options.ValidateOnBuild = true;
            }
        }

        public static void ConfigureWebHostDefaults(IWebHostBuilder builder)
        {
            builder.UseStartup<Startup>();
            builder.ConfigureKestrel((context, options) =>
            {
                var section = context.Configuration.GetSection("Commands");
                var serviceOptions = section.Get<ServiceOptions>() ?? new ServiceOptions();
                options.ListenAnyIP(serviceOptions.Port, listenOptions =>
                {
                    listenOptions.Protocols = serviceOptions.Protocols;
                });
            });
        }
    }
}
