using Amazon.S3;

using Brighid.Commands.Commands;

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
            services.AddSingleton<IAmazonS3, AmazonS3Client>();
            services.AddSingleton<ICommandCache, DefaultCommandCache>();
            services.AddSingleton<IUtilsFactory, DefaultUtilsFactory>();
            services.AddSingleton<ICommandService, DefaultCommandService>();
            services.AddSingleton<ICommandPackageDownloader, DefaultCommandPackageDownloader>();
            services.AddSingleton<ICommandLoader, DefaultCommandLoader>();
            services.AddScoped<ICommandRepository, DefaultCommandRepository>();
        }
    }
}
