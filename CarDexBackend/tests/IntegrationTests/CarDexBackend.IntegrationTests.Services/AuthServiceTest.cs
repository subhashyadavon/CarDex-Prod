using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CarDexBackend.Services;
using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;
using CarDexDatabase;
using Xunit;
using System;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using CarDexBackend.Services.Resources;

using CarDexBackend.Repository.Implementations;
using CarDexBackend.Repository.Interfaces;

namespace DefaultNamespace
{
    public class AuthServiceTest : IDisposable
    {
        private readonly CarDexDbContext _context;
        private readonly AuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly IUserRepository _userRepo;

        public AuthServiceTest()
        {
            var options = new DbContextOptionsBuilder<CarDexDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_AuthService_" + Guid.NewGuid())
                .Options;

            _context = new CarDexDbContext(options);

            // Setup configuration
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"Jwt:SecretKey", "TestSecretKeyForJwtTokenGenerationThatIsAtLeast32CharactersLong"},
                {"Jwt:Issuer", "CarDex"},
                {"Jwt:Audience", "CarDexUsers"},
                {"Jwt:ExpirationMinutes", "60"}
            });
            _configuration = configurationBuilder.Build();

            _logger = new LoggerFactory().CreateLogger<AuthService>();
            _userRepo = new UserRepository(_context);

            _authService = new AuthService(_userRepo, _configuration, _logger, new NullStringLocalizer<SharedResources>());
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task Register_ShouldCreateUserAndReturnToken()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "TestUser",
                Password = "Password123"
            };

            // Act
            var result = await _authService.Register(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.AccessToken);
            Assert.Equal("Bearer", result.TokenType);
            Assert.NotNull(result.User);
            Assert.Equal("TestUser", result.User.Username);
            Assert.Equal(5000000, result.User.Currency);

            // Verify user was saved
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "TestUser");
            Assert.NotNull(user);
            Assert.NotEqual(Guid.Empty, user.Id);
        }

        [Fact]
        public async Task Register_ShouldThrowWhenUsernameIsEmpty()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "",
                Password = "Password123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.Register(request));
        }

        [Fact]
        public async Task Register_ShouldThrowWhenUsernameIsWhitespace()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "   ",
                Password = "Password123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.Register(request));
        }

        [Fact]
        public async Task Register_ShouldThrowWhenPasswordIsTooShort()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "TestUser",
                Password = "12345" // Less than 6 characters
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.Register(request));
        }

        [Fact]
        public async Task Register_ShouldThrowWhenPasswordIsEmpty()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "TestUser",
                Password = ""
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.Register(request));
        }

        [Fact]
        public async Task Register_ShouldThrowWhenUsernameAlreadyExists()
        {
            // Arrange
            var existingUser = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "ExistingUser",
                Password = "HashedPassword",
                Currency = 0
            };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var request = new RegisterRequest
            {
                Username = "ExistingUser",
                Password = "Password123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.Register(request));
        }

        [Fact]
        public async Task Login_ShouldReturnTokenForValidCredentials()
        {
            // Arrange
            var password = "Password123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "TestUser",
                Password = hashedPassword,
                Currency = 100
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new LoginRequest
            {
                Username = "TestUser",
                Password = password
            };

            // Act
            var result = await _authService.Login(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.AccessToken);
            Assert.Equal("Bearer", result.TokenType);
            Assert.NotNull(result.User);
            Assert.Equal("TestUser", result.User.Username);
            Assert.Equal(100, result.User.Currency);
        }

        [Fact]
        public async Task Login_ShouldThrowWhenUserNotFound()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "NonExistentUser",
                Password = "Password123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.Login(request));
        }

        [Fact]
        public async Task Login_ShouldThrowWhenPasswordIsIncorrect()
        {
            // Arrange
            var password = "Password123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "TestUser",
                Password = hashedPassword,
                Currency = 100
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new LoginRequest
            {
                Username = "TestUser",
                Password = "WrongPassword"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.Login(request));
        }

        [Fact]
        public async Task Logout_ShouldCompleteSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act & Assert - Should not throw
            await _authService.Logout(userId);
        }

        [Fact]
        public async Task Register_ShouldGenerateValidJwtToken()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "TestUser",
                Password = "Password123"
            };

            // Act
            var result = await _authService.Register(request);

            // Assert - Verify token can be decoded
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(result.AccessToken);
            
            Assert.NotNull(token);
            Assert.Equal("CarDex", token.Issuer);
            Assert.Contains("CarDexUsers", token.Audiences);
            Assert.True(token.ValidTo > DateTime.UtcNow);
        }

        [Fact]
        public async Task Register_ShouldUseExpirationFromConfiguration()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "TestUser",
                Password = "Password123"
            };

            // Act
            var result = await _authService.Register(request);

            // Assert
            Assert.Equal(3600, result.ExpiresIn); // 60 minutes * 60 seconds
        }

        [Fact]
        public async Task Login_ShouldUseExpirationFromConfiguration()
        {
            // Arrange
            var password = "Password123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "TestUser",
                Password = hashedPassword,
                Currency = 100
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new LoginRequest
            {
                Username = "TestUser",
                Password = password
            };

            // Act
            var result = await _authService.Login(request);

            // Assert
            Assert.Equal(3600, result.ExpiresIn);
        }

        [Fact]
        public async Task Register_ShouldSetUserCreatedAtFromDatabase()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "TestUser",
                Password = "Password123"
            };

            // Act
            var result = await _authService.Register(request);

            // Assert
            Assert.NotNull(result.User.CreatedAt);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "TestUser");
            Assert.Equal(user.CreatedAt, result.User.CreatedAt);
        }
    }
}
