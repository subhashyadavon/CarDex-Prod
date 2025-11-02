using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace CarDexBackend.Shared.Validator
{
    /// <summary>
    /// Rate limiting middleware that restricts each user to 50 API requests per minute.
    /// </summary>
    public class RateLimiter : IMiddleware
    {
        private readonly ILogger<RateLimiter> _logger;
        private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> _userRequests = new();
        private const int MaxRequestsPerMinute = 50;
        private const int TimeWindowSeconds = 60;

        public RateLimiter(ILogger<RateLimiter> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Get the endpoint metadata
            var endpoint = context.GetEndpoint();

            // Allow anonymous endpoints (auth endpoints) to bypass rate limiting
            if (endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>() != null)
            {
                await next(context);
                return;
            }

            // Allow auth endpoints and Swagger to bypass rate limiting
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

            // Extract user ID from JWT token
            var userId = ExtractUserIdFromToken(context);

            // If no user ID found (shouldn't happen for authenticated endpoints), allow through
            // TokenValidator will handle authentication
            if (string.IsNullOrEmpty(userId))
            {
                await next(context);
                return;
            }

            // Check rate limit
            if (!IsWithinRateLimit(userId))
            {
                _logger.LogWarning("Rate limit exceeded for user: {UserId}", userId);
                context.Response.StatusCode = 429; // Too Many Requests
                context.Response.Headers["Retry-After"] = "60"; // Retry after 60 seconds
                
                var remainingRequests = GetRemainingRequests(userId);
                var resetTime = GetResetTime(userId);
                
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Rate limit exceeded. Maximum 50 requests per minute allowed.",
                    statusCode = 429,
                    retryAfter = 60,
                    resetAt = resetTime
                });
                return;
            }

            // Record this request
            RecordRequest(userId);

            // Continue to next middleware
            await next(context);
        }

        /// <summary>
        /// Extracts the user ID from the JWT token in the Authorization header.
        /// </summary>
        private string? ExtractUserIdFromToken(HttpContext context)
        {
            try
            {
                // Try to get user from authenticated claims (after UseAuthentication)
                var userIdClaim = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                    ?? context.User?.FindFirst("sub");

                if (userIdClaim != null && !string.IsNullOrEmpty(userIdClaim.Value))
                {
                    return userIdClaim.Value;
                }

                // Fallback: Extract from token directly if authentication hasn't run yet
                var authHeader = context.Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                if (!tokenHandler.CanReadToken(token))
                {
                    return null;
                }

                var jsonToken = tokenHandler.ReadJwtToken(token);
                var subClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "sub");

                return subClaim?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to extract user ID from token");
                return null;
            }
        }

        /// <summary>
        /// Checks if the user is within the rate limit (50 requests per minute).
        /// </summary>
        private bool IsWithinRateLimit(string userId)
        {
            var now = DateTime.UtcNow;
            var windowStart = now.AddSeconds(-TimeWindowSeconds);

            if (!_userRequests.TryGetValue(userId, out var requests))
            {
                return true; // No previous requests
            }

            // Remove old requests outside the time window
            while (requests.TryPeek(out var oldest) && oldest < windowStart)
            {
                requests.TryDequeue(out _);
            }

            // Check if within limit
            return requests.Count < MaxRequestsPerMinute;
        }

        /// <summary>
        /// Records a request for the specified user.
        /// </summary>
        private void RecordRequest(string userId)
        {
            var now = DateTime.UtcNow;
            var windowStart = now.AddSeconds(-TimeWindowSeconds);

            var queue = _userRequests.GetOrAdd(userId, _ => new ConcurrentQueue<DateTime>());
            
            // Remove old requests outside the time window
            while (queue.TryPeek(out var oldest) && oldest < windowStart)
            {
                queue.TryDequeue(out _);
            }
            
            // Add the new request
            queue.Enqueue(now);
        }

        /// <summary>
        /// Gets the number of remaining requests for the user in the current time window.
        /// </summary>
        private int GetRemainingRequests(string userId)
        {
            var now = DateTime.UtcNow;
            var windowStart = now.AddSeconds(-TimeWindowSeconds);

            if (!_userRequests.TryGetValue(userId, out var requests))
            {
                return MaxRequestsPerMinute;
            }

            // Remove old requests outside the time window
            while (requests.TryPeek(out var oldest) && oldest < windowStart)
            {
                requests.TryDequeue(out _);
            }

            return Math.Max(0, MaxRequestsPerMinute - requests.Count);
        }

        /// <summary>
        /// Gets the time when the rate limit will reset for the user.
        /// </summary>
        private DateTime GetResetTime(string userId)
        {
            if (!_userRequests.TryGetValue(userId, out var requests) || requests.IsEmpty)
            {
                return DateTime.UtcNow;
            }

            // Find the oldest request in the queue by peeking
            if (requests.TryPeek(out var oldestRequest))
            {
                return oldestRequest.AddSeconds(TimeWindowSeconds);
            }

            return DateTime.UtcNow;
        }
    }
}

