using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Serilog;

#pragma warning disable SA1600, CS1591

namespace Brighid.Commands
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseSerilog(dispose: true)
                .ConfigureHostConfiguration(config =>
                {
                    config.AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(builder =>
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
                });
        }
    }
}
