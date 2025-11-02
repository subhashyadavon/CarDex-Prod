using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using CarDexBackend.Shared.Validator;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace CarDexBackend.UnitTests.Api
{
    public class TokenValidatorTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<TokenValidator>> _mockLogger;
        private readonly TokenValidator _tokenValidator;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public TokenValidatorTests()
        {
            _secretKey = "TestSecretKeyForJwtTokenGenerationThatIsAtLeast32CharactersLong";
            _issuer = "CarDex";
            _audience = "CarDexUsers";

            _mockConfiguration = new Mock<IConfiguration>();
            var jwtSection = new Mock<IConfigurationSection>();
            jwtSection.Setup(x => x["SecretKey"]).Returns(_secretKey);
            jwtSection.Setup(x => x["Issuer"]).Returns(_issuer);
            jwtSection.Setup(x => x["Audience"]).Returns(_audience);
            _mockConfiguration.Setup(x => x.GetSection("Jwt")).Returns(jwtSection.Object);

            _mockLogger = new Mock<ILogger<TokenValidator>>();
            _tokenValidator = new TokenValidator(_mockConfiguration.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task InvokeAsync_ShouldAllowAnonymousEndpoints()
        {
            // Arrange
            var context = CreateContextWithPath("/api/test");
            var endpoint = CreateEndpointWithAllowAnonymous();
            context.SetEndpoint(endpoint);
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_ShouldAllowAuthRegisterEndpoint()
        {
            // Arrange
            var context = CreateContextWithPath("/auth/register");
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_ShouldAllowAuthLoginEndpoint()
        {
            // Arrange
            var context = CreateContextWithPath("/auth/login");
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_ShouldAllowSwaggerEndpoints()
        {
            // Arrange
            var context = CreateContextWithPath("/swagger/index.html");
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_ShouldAllowRootPath()
        {
            // Arrange
            var context = CreateContextWithPath("/");
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_ShouldRejectProtectedEndpointWithoutToken()
        {
            // Arrange
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers.Remove("Authorization");
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.False(nextCalled);
            Assert.Equal(401, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_ShouldRejectProtectedEndpointWithInvalidToken()
        {
            // Arrange
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = "Bearer invalid.token.here";
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.False(nextCalled);
            Assert.Equal(401, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_ShouldAllowProtectedEndpointWithValidToken()
        {
            // Arrange
            var token = CreateValidToken();
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = $"Bearer {token}";
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_ShouldRejectExpiredToken()
        {
            // Arrange
            var token = CreateExpiredToken();
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = $"Bearer {token}";
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.False(nextCalled);
            Assert.Equal(401, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_ShouldRejectTokenWithWrongIssuer()
        {
            // Arrange
            var token = CreateTokenWithWrongIssuer();
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = $"Bearer {token}";
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.False(nextCalled);
            Assert.Equal(401, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_ShouldRejectTokenWithWrongAudience()
        {
            // Arrange
            var token = CreateTokenWithWrongAudience();
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = $"Bearer {token}";
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.False(nextCalled);
            Assert.Equal(401, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_ShouldRejectTokenWithWrongSignature()
        {
            // Arrange
            var token = CreateTokenWithWrongSignature();
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = $"Bearer {token}";
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.False(nextCalled);
            Assert.Equal(401, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_ShouldRejectMalformedToken()
        {
            // Arrange
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = "Bearer not.a.valid.jwt.token";
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.False(nextCalled);
            Assert.Equal(401, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_ShouldReturn401ResponseWithMessage()
        {
            // Arrange
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers.Remove("Authorization");
            context.Response.Body = new MemoryStream();

            RequestDelegate next = async (HttpContext ctx) => await Task.CompletedTask;

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.Equal(401, context.Response.StatusCode);
            context.Response.Body.Position = 0;
            var reader = new StreamReader(context.Response.Body);
            var body = await reader.ReadToEndAsync();
            Assert.Contains("Authentication token is required", body);
        }

        [Fact]
        public async Task InvokeAsync_ShouldAllowAuthDebugEndpoint()
        {
            // Arrange
            var context = CreateContextWithPath("/auth/debug");
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_ShouldAllowFaviconEndpoint()
        {
            // Arrange
            var context = CreateContextWithPath("/favicon.ico");
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_ShouldHandleNullPath()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = new PathString();
            context.Request.Headers.Remove("Authorization");
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            // Should require token for null path if not anonymous
            Assert.False(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_ShouldHandleBearerCaseInsensitive()
        {
            // Arrange
            var token = CreateValidToken();
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = $"bearer {token}"; // lowercase
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.True(nextCalled);
        }

        private HttpContext CreateContextWithPath(string path)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = new PathString(path);
            context.Request.Method = "GET";
            context.Response.Body = new MemoryStream();
            return context;
        }

        private Endpoint CreateEndpointWithAllowAnonymous()
        {
            var endpoint = new Endpoint(
                async (HttpContext context) => await Task.CompletedTask,
                new EndpointMetadataCollection(new AllowAnonymousAttribute()),
                "test");
            return endpoint;
        }

        private string CreateValidToken()
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, "testuser"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string CreateExpiredToken()
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, "testuser"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(-60), // Expired
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string CreateTokenWithWrongIssuer()
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, "testuser"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "WrongIssuer",
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string CreateTokenWithWrongAudience()
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, "testuser"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: "WrongAudience",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string CreateTokenWithWrongSignature()
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, "testuser"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var wrongKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("WrongSecretKeyForJwtTokenGenerationThatIsAtLeast32CharactersLong"));
            var credentials = new SigningCredentials(wrongKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Fact]
        public async Task InvokeAsync_ShouldRejectEmptyBearerToken()
        {
            // Arrange
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = "Bearer ";
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.False(nextCalled);
            Assert.Equal(401, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_ShouldRejectBearerTokenWithOnlyWhitespace()
        {
            // Arrange
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = "Bearer    ";
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.False(nextCalled);
            Assert.Equal(401, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_ShouldRejectTokenWithoutBearerPrefix()
        {
            // Arrange
            var token = CreateValidToken();
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = token; // No "Bearer " prefix
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.False(nextCalled);
            Assert.Equal(401, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_ShouldHandleBearerWithMultipleSpaces()
        {
            // Arrange
            var token = CreateValidToken();
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = $"Bearer    {token}"; // Multiple spaces
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.True(nextCalled); // Trim should handle this
        }

        [Fact]
        public async Task InvokeAsync_ShouldHandleUppercaseBearer()
        {
            // Arrange
            var token = CreateValidToken();
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = $"BEARER {token}";
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_ShouldHandleMixedCaseBearer()
        {
            // Arrange
            var token = CreateValidToken();
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = $"BeArEr {token}";
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_ShouldAllowSwaggerUiPath()
        {
            // Arrange
            var context = CreateContextWithPath("/swagger-ui/index.html");
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_ShouldAllowSwaggerPathWithQueryString()
        {
            // Arrange
            var context = CreateContextWithPath("/swagger/index.html?version=1");
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_ShouldHandleUppercaseAuthPath()
        {
            // Arrange
            var context = CreateContextWithPath("/AUTH/REGISTER");
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.True(nextCalled); // Path comparison is case-insensitive
        }

        [Fact]
        public async Task InvokeAsync_ShouldReturn401ResponseWithCorrectFormatForInvalidToken()
        {
            // Arrange
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = "Bearer invalid.token.here";
            context.Response.Body = new MemoryStream();

            RequestDelegate next = async (HttpContext ctx) => await Task.CompletedTask;

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.Equal(401, context.Response.StatusCode);
            context.Response.Body.Position = 0;
            var reader = new StreamReader(context.Response.Body);
            var body = await reader.ReadToEndAsync();
            Assert.Contains("Invalid or expired authentication token", body);
            Assert.Contains("statusCode", body);
        }

        [Fact]
        public async Task InvokeAsync_ShouldHandlePostRequest()
        {
            // Arrange
            var token = CreateValidToken();
            var context = CreateContextWithPath("/api/cards");
            context.Request.Method = "POST";
            context.Request.Headers["Authorization"] = $"Bearer {token}";
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_ShouldHandlePutRequest()
        {
            // Arrange
            var token = CreateValidToken();
            var context = CreateContextWithPath("/api/cards/123");
            context.Request.Method = "PUT";
            context.Request.Headers["Authorization"] = $"Bearer {token}";
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_ShouldHandleDeleteRequest()
        {
            // Arrange
            var token = CreateValidToken();
            var context = CreateContextWithPath("/api/cards/123");
            context.Request.Method = "DELETE";
            context.Request.Headers["Authorization"] = $"Bearer {token}";
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_ShouldRejectTokenJustExpired()
        {
            // Arrange - Token expired 1 second ago
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, "testuser")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds(-1), // Just expired
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = $"Bearer {tokenString}";
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.False(nextCalled);
            Assert.Equal(401, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_ShouldAllowTokenWithNoNbf()
        {
            // Arrange - Token without "not before" claim, only expiration
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, "testuser")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = $"Bearer {tokenString}";
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_ShouldRejectTokenWithInvalidJsonFormat()
        {
            // Arrange
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = "Bearer not.valid.json";
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.False(nextCalled);
            Assert.Equal(401, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_ShouldRejectTokenWithOnlyTwoParts()
        {
            // Arrange - JWT should have 3 parts separated by dots
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = "Bearer part1.part2";
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.False(nextCalled);
            Assert.Equal(401, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_ShouldRejectTokenWithFourParts()
        {
            // Arrange - JWT should have exactly 3 parts
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = "Bearer part1.part2.part3.part4";
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.False(nextCalled);
            Assert.Equal(401, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_ShouldHandlePathWithTrailingSlash()
        {
            // Arrange
            var context = CreateContextWithPath("/auth/register/");
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            // Should still allow because path starts with "/auth/register"
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_ShouldHandlePathWithQueryParameters()
        {
            // Arrange
            var token = CreateValidToken();
            var context = CreateContextWithPath("/api/cards?limit=10&offset=0");
            context.Request.Headers["Authorization"] = $"Bearer {token}";
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_ShouldRejectTokenWhenSecretKeyIsUseEnvironmentVariable()
        {
            // Arrange - Create validator with "UseEnvironmentVariable" as secret key
            var mockConfig = new Mock<IConfiguration>();
            var jwtSection = new Mock<IConfigurationSection>();
            jwtSection.Setup(x => x["SecretKey"]).Returns("UseEnvironmentVariable");
            jwtSection.Setup(x => x["Issuer"]).Returns(_issuer);
            jwtSection.Setup(x => x["Audience"]).Returns(_audience);
            mockConfig.Setup(x => x.GetSection("Jwt")).Returns(jwtSection.Object);

            var validator = new TokenValidator(mockConfig.Object, _mockLogger.Object);
            var token = CreateValidToken();
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = $"Bearer {token}";
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await validator.InvokeAsync(context, next);

            // Assert
            Assert.False(nextCalled);
            Assert.Equal(401, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_ShouldRejectTokenWhenSecretKeyIsNull()
        {
            // Arrange - Create validator with null secret key
            var mockConfig = new Mock<IConfiguration>();
            var jwtSection = new Mock<IConfigurationSection>();
            jwtSection.Setup(x => x["SecretKey"]).Returns((string?)null);
            jwtSection.Setup(x => x["Issuer"]).Returns(_issuer);
            jwtSection.Setup(x => x["Audience"]).Returns(_audience);
            mockConfig.Setup(x => x.GetSection("Jwt")).Returns(jwtSection.Object);

            var validator = new TokenValidator(mockConfig.Object, _mockLogger.Object);
            var token = CreateValidToken();
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = $"Bearer {token}";
            var nextCalled = false;

            RequestDelegate next = async (HttpContext ctx) =>
            {
                nextCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await validator.InvokeAsync(context, next);

            // Assert
            Assert.False(nextCalled);
            Assert.Equal(401, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_ShouldAllowTokenWhenEnvironmentVariableIsSet()
        {
            // Arrange - Set environment variable and use "UseEnvironmentVariable" in config
            var originalEnv = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
            try
            {
                Environment.SetEnvironmentVariable("JWT_SECRET_KEY", _secretKey);

                var mockConfig = new Mock<IConfiguration>();
                var jwtSection = new Mock<IConfigurationSection>();
                jwtSection.Setup(x => x["SecretKey"]).Returns("UseEnvironmentVariable");
                jwtSection.Setup(x => x["Issuer"]).Returns(_issuer);
                jwtSection.Setup(x => x["Audience"]).Returns(_audience);
                mockConfig.Setup(x => x.GetSection("Jwt")).Returns(jwtSection.Object);

                var validator = new TokenValidator(mockConfig.Object, _mockLogger.Object);
                var token = CreateValidToken();
                var context = CreateContextWithPath("/api/cards");
                context.Request.Headers["Authorization"] = $"Bearer {token}";
                var nextCalled = false;

                RequestDelegate next = async (HttpContext ctx) =>
                {
                    nextCalled = true;
                    await Task.CompletedTask;
                };

                // Act
                await validator.InvokeAsync(context, next);

                // Assert
                Assert.True(nextCalled);
            }
            finally
            {
                if (originalEnv != null)
                {
                    Environment.SetEnvironmentVariable("JWT_SECRET_KEY", originalEnv);
                }
                else
                {
                    Environment.SetEnvironmentVariable("JWT_SECRET_KEY", null);
                }
            }
        }

        [Fact]
        public async Task InvokeAsync_ShouldLogDebugForMissingToken()
        {
            // Arrange
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers.Remove("Authorization");
            var logCalls = new List<string>();

            _mockLogger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>(
                    (level, eventId, state, exception, formatter) =>
                    {
                        if (level == LogLevel.Debug)
                        {
                            logCalls.Add("Debug");
                        }
                    });

            RequestDelegate next = async (HttpContext ctx) => await Task.CompletedTask;

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.Equal(401, context.Response.StatusCode);
            // Verify debug logging was called (at least once)
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task InvokeAsync_ShouldReturnJsonResponseForMissingToken()
        {
            // Arrange
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers.Remove("Authorization");
            context.Response.Body = new MemoryStream();

            RequestDelegate next = async (HttpContext ctx) => await Task.CompletedTask;

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.Equal(401, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);
            context.Response.Body.Position = 0;
            var reader = new StreamReader(context.Response.Body);
            var body = await reader.ReadToEndAsync();
            
            // Verify it's valid JSON
            var json = JsonSerializer.Deserialize<Dictionary<string, object>>(body);
            Assert.NotNull(json);
            Assert.True(json.ContainsKey("message"));
            Assert.True(json.ContainsKey("statusCode"));
        }

        [Fact]
        public async Task InvokeAsync_ShouldReturnJsonResponseForInvalidToken()
        {
            // Arrange
            var context = CreateContextWithPath("/api/cards");
            context.Request.Headers["Authorization"] = "Bearer invalid.token";
            context.Response.Body = new MemoryStream();

            RequestDelegate next = async (HttpContext ctx) => await Task.CompletedTask;

            // Act
            await _tokenValidator.InvokeAsync(context, next);

            // Assert
            Assert.Equal(401, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);
            context.Response.Body.Position = 0;
            var reader = new StreamReader(context.Response.Body);
            var body = await reader.ReadToEndAsync();
            
            // Verify it's valid JSON
            var json = JsonSerializer.Deserialize<Dictionary<string, object>>(body);
            Assert.NotNull(json);
            Assert.True(json.ContainsKey("message"));
            Assert.True(json.ContainsKey("statusCode"));
        }
    }
}
