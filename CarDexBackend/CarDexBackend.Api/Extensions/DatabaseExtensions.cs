using CarDexDatabase;
using CarDexBackend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace CarDexBackend.Api.Extensions
{
    /// <summary>
    /// Extension methods for configuring database services.
    /// </summary>
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Configures the database context with PostgreSQL and enum mappings.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="config">The application configuration.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection ConfigureDatabase(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("CarDexDatabase");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("CarDexDatabase connection string is not configured.");
            }

            var builder = new NpgsqlDataSourceBuilder(connectionString);
            builder.MapEnum<GradeEnum>("grade_enum");
            builder.MapEnum<TradeEnum>("trade_enum");
            builder.MapEnum<RewardEnum>("reward_enum");

            var dataSource = builder.Build();
            services.AddDbContext<CarDexDbContext>(opt => opt.UseNpgsql(dataSource));

            return services;
        }

        /// <summary>
        /// Ensures the database is created and validates the connection.
        /// </summary>
        /// <param name="app">The web application.</param>
        /// <returns>The web application for chaining.</returns>
        public static WebApplication EnsureDatabaseCreated(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CarDexDbContext>();
                try
                {
                    context.Database.EnsureCreated();
                    Console.WriteLine("✓ Database connection established successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Database connection failed: {ex.Message}");
                }
            }

            return app;
        }
    }
}

