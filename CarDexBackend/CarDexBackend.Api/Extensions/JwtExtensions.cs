using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CarDexBackend.Api.Extensions
{
    /// <summary>
    /// Extension methods for configuring JWT authentication and authorization.
    /// </summary>
    public static class JwtExtensions
    {
        /// <summary>
        /// Configures JWT authentication with token validation parameters.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="config">The application configuration.</param>
        /// <returns>The service collection for chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown when JWT secret key is not configured.</exception>
        public static IServiceCollection ConfigureJwtAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var jwtSettings = config.GetSection("Jwt");
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? jwtSettings["SecretKey"];

            if (string.IsNullOrEmpty(secretKey) || secretKey == "UseEnvironmentVariable")
            {
                throw new InvalidOperationException(
                    "JWT_SECRET_KEY environment variable is not set. " +
                    "Please set it using: export JWT_SECRET_KEY='YourSecretKeyHere' " +
                    "or configure it in appsettings.Development.json for local development.");
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
                };
            });

            return services;
        }

        /// <summary>
        /// Configures authorization services.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection ConfigureAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization();
            return services;
        }
    }
}

