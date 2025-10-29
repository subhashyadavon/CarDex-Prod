using CarDexBackend.Services;
using CarDexBackend.Shared.Validator;
using CarDexDatabase;
using Microsoft.EntityFrameworkCore;
using CarDexBackend.Domain.Enums;
using Npgsql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Database Context with PostgreSQL and register enum types
var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("CarDexDatabase"));
// Map enums to their database types (specify the PostgreSQL enum type name)
dataSourceBuilder.MapEnum<GradeEnum>("grade_enum");
dataSourceBuilder.MapEnum<TradeEnum>("trade_enum");
dataSourceBuilder.MapEnum<RewardEnum>("reward_enum");
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<CarDexDbContext>(options =>
    options.UseNpgsql(dataSource)
);

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
// Try to get secret key from environment variable first, fallback to configuration
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
    ?? jwtSettings["SecretKey"];

// Validate that we have a secret key
if (string.IsNullOrEmpty(secretKey) || secretKey == "UseEnvironmentVariable")
{
    throw new InvalidOperationException(
        "JWT_SECRET_KEY environment variable is not set. " +
        "Please set it using: export JWT_SECRET_KEY='YourSecretKeyHere' " +
        "or configure it in appsettings.Development.json for local development.");
}

builder.Services.AddAuthentication(options =>
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// add services to the container
builder.Services.AddControllers();

// register Swagger to test endpoints
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CarDex API",
        Version = "v1",
        Description = "CarDex Trading Card Game API"
    });

    // Add JWT Bearer authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// add CORS for our frontend(s)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});


// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<ICollectionService, CollectionService>();
builder.Services.AddScoped<IPackService, PackService>();
builder.Services.AddScoped<ITradeService, TradeService>();
builder.Services.AddScoped<IUserService, UserService>();

// Add Authorization
builder.Services.AddAuthorization();

// Register Token Validator
builder.Services.AddScoped<TokenValidator>();

var app = builder.Build();


// Enable Swagger UI in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors();

// Add token validator to validate all API calls
app.UseTokenValidator();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Create database tables if they don't exist (for development)
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

app.Run();