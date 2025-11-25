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
        public static IServiceCollection ConfigureDatabase(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("CarDexDatabase");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("CarDexDatabase connection string is not configured.");
            }

            var builder = new NpgsqlDataSourceBuilder(connectionString);
            // Map enums with explicit PostgreSQL type names
            // Use case-insensitive mapping to handle uppercase values in database
            builder.MapEnum<GradeEnum>("grade_enum", nameTranslator: new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
            builder.MapEnum<TradeEnum>("trade_enum", nameTranslator: new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
            builder.MapEnum<RewardEnum>("reward_enum", nameTranslator: new Npgsql.NameTranslation.NpgsqlNullNameTranslator());

            var dataSource = builder.Build();
            services.AddDbContext<CarDexDbContext>(opt => opt.UseNpgsql(dataSource));

            return services;
        }


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

