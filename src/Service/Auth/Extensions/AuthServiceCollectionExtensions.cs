using System;

using Brighid.Commands.Auth;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Service Collection Extensions for Authentication / Authorization.
    /// </summary>
    public static class AuthServiceCollectionExtensions
    {
        /// <summary>
        /// Adds auth services to the service collection.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configure">Configuration delegate to configure auth with.</param>
        public static void ConfigureAuthServices(this IServiceCollection services, Action<AuthOptions> configure)
        {
            var authOptions = new AuthOptions();
            configure(authOptions);

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RefreshOnIssuerKeyNotFound = true;
                options.RequireHttpsMetadata = authOptions.MetadataAddress.Scheme == "https";
                options.MetadataAddress = authOptions.MetadataAddress.ToString();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    RequireExpirationTime = true,
                    ValidateAudience = false,
                    ValidateIssuer = true,
                    ValidIssuer = authOptions.ValidIssuer,
                    ClockSkew = TimeSpan.FromMinutes(authOptions.ClockSkew),
                };
            });
        }
    }
}
