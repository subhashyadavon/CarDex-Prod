using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using CarDexBackend.Shared.Validator;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CarDexBackend.UnitTests.Api.Controllers
{
    /// <summary>
    /// Unit tests for the RateLimiter middleware.
    /// Tests rate limiting functionality without requiring full API integration.
    /// </summary>
    public class RateLimiterTests
    {
        private readonly Mock<ILogger<RateLimiter>> _mockLogger;
        private readonly RateLimiter _rateLimiter;

        public RateLimiterTests()
        {
            _mockLogger = new Mock<ILogger<RateLimiter>>();
            _rateLimiter = new RateLimiter(_mockLogger.Object);
        }

        [Fact]
        public async Task RateLimiter_Allows50RequestsWithinTimeWindow()
        {
            // Arrange
            var context = CreateContextWithUserId("test-user-1");
            var nextCalled = 0;
            RequestDelegate next = async (ctx) =>
            {
                nextCalled++;
                await Task.CompletedTask;
            };

            // Act: Make 50 requests
            for (int i = 0; i < 50; i++)
            {
                var newContext = CreateContextWithUserId("test-user-1");
                await _rateLimiter.InvokeAsync(newContext, next);
            }

            // Assert: All 50 requests should pass through
            Assert.Equal(50, nextCalled);
        }

        [Fact]
        public async Task RateLimiter_Blocks51stRequest()
        {
            // Arrange
            var userId = "test-user-2";
            var nextCalled = 0;
            RequestDelegate next = async (ctx) =>
            {
                nextCalled++;
                await Task.CompletedTask;
            };

            // Act: Make 50 requests first
            for (int i = 0; i < 50; i++)
            {
                var context = CreateContextWithUserId(userId);
                await _rateLimiter.InvokeAsync(context, next);
            }

            // Make 51st request
            var blockedContext = CreateContextWithUserId(userId);
            await _rateLimiter.InvokeAsync(blockedContext, next);

            // Assert: 50 requests passed, 51st should be blocked
            Assert.Equal(50, nextCalled);
            Assert.Equal(429, blockedContext.Response.StatusCode);
        }

        [Fact]
        public async Task RateLimiter_DifferentUsersHaveSeparateLimits()
        {
            // Arrange
            var nextCalled = 0;
            RequestDelegate next = async (ctx) =>
            {
                nextCalled++;
                await Task.CompletedTask;
            };

            // Act: User 1 exhausts their limit
            for (int i = 0; i < 50; i++)
            {
                var context = CreateContextWithUserId("user-1");
                await _rateLimiter.InvokeAsync(context, next);
            }

            // User 2 should still be able to make requests
            var user2Context = CreateContextWithUserId("user-2");
            await _rateLimiter.InvokeAsync(user2Context, next);

            // Assert: User 2's request should pass (not rate limited)
            Assert.Equal(51, nextCalled);
            Assert.NotEqual(429, user2Context.Response.StatusCode);
        }

        [Fact]
        public async Task RateLimiter_BypassesAuthEndpoints()
        {
            // Arrange
            var nextCalled = 0;
            RequestDelegate next = async (ctx) =>
            {
                nextCalled++;
                await Task.CompletedTask;
            };

            // Act: Make many requests to auth endpoints
            for (int i = 0; i < 60; i++)
            {
                var context = CreateContextWithPath("/auth/login");
                await _rateLimiter.InvokeAsync(context, next);
            }

            // Assert: All requests should pass (not rate limited)
            Assert.Equal(60, nextCalled);
        }

        [Fact]
        public async Task RateLimiter_BypassesSwaggerEndpoints()
        {
            // Arrange
            var nextCalled = 0;
            RequestDelegate next = async (ctx) =>
            {
                nextCalled++;
                await Task.CompletedTask;
            };

            // Act: Make many requests to Swagger endpoints
            for (int i = 0; i < 100; i++)
            {
                var context = CreateContextWithPath("/swagger/index.html");
                await _rateLimiter.InvokeAsync(context, next);
            }

            // Assert: All requests should pass (not rate limited)
            Assert.Equal(100, nextCalled);
        }

        [Fact]
        public async Task RateLimiter_ReturnsCorrect429Response()
        {
            // Arrange
            var userId = "test-user-3";
            var nextCalled = 0;
            RequestDelegate next = async (ctx) =>
            {
                nextCalled++;
                await Task.CompletedTask;
            };

            // Act: Exhaust rate limit
            for (int i = 0; i < 50; i++)
            {
                var context = CreateContextWithUserId(userId);
                await _rateLimiter.InvokeAsync(context, next);
            }

            var blockedContext = CreateContextWithUserId(userId);
            await _rateLimiter.InvokeAsync(blockedContext, next);

            // Assert: Check response details
            Assert.Equal(429, blockedContext.Response.StatusCode);
            Assert.True(blockedContext.Response.Headers.ContainsKey("Retry-After"));
        }

        [Fact]
        public async Task RateLimiter_AllowsAnonymousEndpoints()
        {
            // Arrange
            var nextCalled = 0;
            RequestDelegate next = async (ctx) =>
            {
                nextCalled++;
                await Task.CompletedTask;
            };

            // Act: Create context with AllowAnonymous attribute
            var context = CreateContextWithAllowAnonymous();
            await _rateLimiter.InvokeAsync(context, next);

            // Assert: Request should pass
            Assert.Equal(1, nextCalled);
            Assert.NotEqual(429, context.Response.StatusCode);
        }

        // Helper methods
        private HttpContext CreateContextWithUserId(string userId)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/users/me";
            context.Request.Method = "GET";

            // Set user claims
            var claims = new[]
            {
                new Claim("sub", userId),
                new Claim("unique_name", "testuser")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            context.User = new ClaimsPrincipal(identity);

            // Create a JWT token with the user ID
            var token = CreateTestToken(userId);
            context.Request.Headers["Authorization"] = $"Bearer {token}";

            return context;
        }

        private HttpContext CreateContextWithPath(string path)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = path;
            context.Request.Method = "GET";
            return context;
        }

        private HttpContext CreateContextWithAllowAnonymous()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/public";
            context.Request.Method = "GET";

            // Mock endpoint with AllowAnonymous attribute
            var metadata = new Microsoft.AspNetCore.Http.EndpointMetadataCollection(
                new Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute()
            );
            var endpoint = new Microsoft.AspNetCore.Http.Endpoint(
                async (ctx) => await Task.CompletedTask,
                metadata,
                "test"
            );
            context.SetEndpoint(endpoint);

            return context;
        }

        private string CreateTestToken(string userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("TestSecretKeyThatIsAtLeast32CharactersLong!123456");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("sub", userId),
                    new Claim("unique_name", "testuser")
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

