using CarDexBackend.Api.Extensions;
using CarDexBackend.Services;
using CarDexBackend.Shared.Validator;

var builder = WebApplication.CreateBuilder(args);

// Configure database
builder.Services.ConfigureDatabase(builder.Configuration);

// Configure JWT authentication and authorization
builder.Services.ConfigureJwtAuthentication(builder.Configuration);
builder.Services.ConfigureAuthorization();

// Configure Swagger/OpenAPI
builder.Services.ConfigureSwagger();

// Add controllers
builder.Services.AddControllers();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Register business services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<ICollectionService, CollectionService>();
builder.Services.AddScoped<IPackService, PackService>();
builder.Services.AddScoped<ITradeService, TradeService>();
builder.Services.AddScoped<IUserService, UserService>();

// Register custom middleware
builder.Services.AddScoped<TokenValidator>();
builder.Services.AddSingleton<RateLimiter>();

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors();
app.UseTokenValidator();
app.UseAuthentication();
app.UseAuthorization();
app.UseUserRateLimiter();
app.MapControllers();

// Ensure database is created
app.EnsureDatabaseCreated();

app.Run();
