using Amazon.S3;

using Brighid.Commands.Service;

using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Service Collection Extensions for commands.
    /// </summary>
    public static class CommandServiceCollectionExtensions
    {
        /// <summary>
        /// Configures commands services.
        /// </summary>
        /// <param name="services">Services to configure.</param>
        public static void ConfigureCommandServices(this IServiceCollection services)
        {
            services.AddSingleton<IAmazonS3>(new AmazonS3Client(new AmazonS3Config { UseDualstackEndpoint = true }));
            services.AddSingleton<ICommandCache, DefaultCommandCache>();
            services.AddSingleton<IUtilsFactory, DefaultUtilsFactory>();
            services.AddSingleton<ICommandService, DefaultCommandService>();
            services.AddSingleton<ICommandPackageDownloader, DefaultCommandPackageDownloader>();
            services.AddSingleton<ICommandLoader, DefaultCommandLoader>();
            services.AddSingleton<IHostedService, CommandLoaderBackgroundService>();
            services.AddScoped<ICommandRepository, DefaultCommandRepository>();
        }
    }
}
