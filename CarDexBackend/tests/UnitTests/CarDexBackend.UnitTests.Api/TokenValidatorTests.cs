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
    }
}
