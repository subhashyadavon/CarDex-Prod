using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace CarDexBackend.Shared.Validator
{
    /// <summary>
    /// Validates JWT tokens for all API calls.
    /// </summary>
    public class TokenValidator : IMiddleware
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenValidator> _logger;

        public TokenValidator(IConfiguration configuration, ILogger<TokenValidator> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Get the endpoint metadata
            var endpoint = context.GetEndpoint();

            // Check if endpoint allows anonymous third-person access
            if (endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>() != null)
            {
                await next(context);
                return;
            }

            // Allow auth endpoints (login/register) to pass through without validation
            var path = context.Request.Path.Value?.ToLower();
            if (path != null && (
                path.StartsWith("/auth/register") || 
                path.StartsWith("/auth/login") || 
                path.StartsWith("/auth/debug") ||
                path == "/" ||
                path == "/favicon.ico" ||
                path.StartsWith("/swagger") ||
                path.StartsWith("/swagger-ui")))
            {
                await next(context);
                return;
            }

            // For all other endpoints, require token validation
            // This ensures nothing is public except auth endpoints

            // For protected endpoints, validate the token
            var token = ExtractTokenFromHeader(context.Request.Headers["Authorization"]);

            if (string.IsNullOrEmpty(token))
            {
                // Use debug level instead of warning for expected missing tokens
                _logger.LogDebug("Missing authentication token for protected endpoint: {Method} {Path}", 
                    context.Request.Method, context.Request.Path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Authentication token is required.",
                    statusCode = 401
                });
                return;
            }

            if (!ValidateToken(token))
            {
                // Use debug level instead of warning for expected invalid tokens
                _logger.LogDebug("Invalid authentication token for protected endpoint: {Method} {Path}", 
                    context.Request.Method, context.Request.Path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Invalid or expired authentication token.",
                    statusCode = 401
                });
                return;
            }

            // Token is valid, proceed to next middleware
            await next(context);
        }

        /// <summary>
        /// Extracts the JWT token from the Authorization header.
        /// </summary>
        private string? ExtractTokenFromHeader(string? authHeader)
        {
            if (string.IsNullOrEmpty(authHeader))
                return null;

            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return null;

            return authHeader.Substring("Bearer ".Length).Trim();
        }

        /// <summary>
        /// Validates the JWT token signature, expiration, issuer, and audience.
        /// </summary>
        private bool ValidateToken(string token)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("Jwt");
                var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
                    ?? jwtSettings["SecretKey"];

                if (string.IsNullOrEmpty(secretKey) || secretKey == "UseEnvironmentVariable")
                {
                    _logger.LogError("JWT secret key is not configured properly");
                    return false;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(secretKey);

                var validationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
                };

                // Validate the token
                tokenHandler.ValidateToken(token, validationParameters, out _);
                
                _logger.LogDebug("JWT token validation successful");
                return true;
            }
            catch (Microsoft.IdentityModel.Tokens.SecurityTokenExpiredException ex)
            {
                _logger.LogWarning(ex, "JWT token has expired");
                return false;
            }
            catch (Microsoft.IdentityModel.Tokens.SecurityTokenInvalidSignatureException ex)
            {
                _logger.LogWarning(ex, "JWT token signature is invalid");
                return false;
            }
            catch (Microsoft.IdentityModel.Tokens.SecurityTokenValidationException ex)
            {
                _logger.LogWarning(ex, "JWT token validation failed: {Message}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token validation");
                return false;
            }
        }
    }
}

