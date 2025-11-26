using CarDexDatabase;
using CarDexBackend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.NameTranslation;
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

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            var nameTranslator = new NpgsqlNullNameTranslator();
            dataSourceBuilder.MapEnum<GradeEnum>("grade_enum", nameTranslator);
            dataSourceBuilder.MapEnum<TradeEnum>("trade_enum", nameTranslator);
            dataSourceBuilder.MapEnum<RewardEnum>("reward_enum", nameTranslator);

            var dataSource = dataSourceBuilder.Build();

            services.AddDbContext<CarDexDbContext>(opt => opt.UseNpgsql(dataSource));

            return services;
        }


        public static WebApplication EnsureDatabaseCreated(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CarDexDbContext>();
                int maxRetries = 10;
                int delaySeconds = 2;

                for (int i = 0; i < maxRetries; i++)
                {
                    try
                    {
                        // Manually create enums first to avoid "type does not exist" errors
                        // This handles the case where EnsureCreated tries to use enums before creating them
                        // Using uppercase values to match Supabase schema
                        var sql = @"
                            DO $$ BEGIN
                                CREATE TYPE grade_enum AS ENUM ('FACTORY', 'LIMITED_RUN', 'NISMO');
                            EXCEPTION
                                WHEN duplicate_object THEN null;
                            END $$;

                            DO $$ BEGIN
                                CREATE TYPE trade_enum AS ENUM ('FOR_CARD', 'FOR_PRICE');
                            EXCEPTION
                                WHEN duplicate_object THEN null;
                            END $$;

                            DO $$ BEGIN
                                CREATE TYPE reward_enum AS ENUM ('PACK', 'CURRENCY', 'CARD_FROM_TRADE', 'CURRENCY_FROM_TRADE');
                            EXCEPTION
                                WHEN duplicate_object THEN null;
                            END $$;
                        ";
                        context.Database.ExecuteSqlRaw(sql);

                        context.Database.EnsureCreated();
                        Console.WriteLine("✓ Database connection established successfully!");
                        return app;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"✗ Database connection failed (Attempt {i + 1}/{maxRetries}): {ex.Message}");
                        if (i == maxRetries - 1)
                        {
                            Console.WriteLine("Giving up after multiple attempts.");
                        }
                        else
                        {
                            Console.WriteLine($"Retrying in {delaySeconds} seconds...");
                            System.Threading.Thread.Sleep(delaySeconds * 1000);
                        }
                    }
                }
            }

            return app;
        }
    }
}

