using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;
using CarDexBackend.Domain.Entities;
using CarDexBackend.Repository.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using BCrypt.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using CarDexBackend.Services.Resources;

namespace CarDexBackend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IStringLocalizer<SharedResources> _sr;
        private readonly IUserRepository _userRepo;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepo,
            IConfiguration configuration,
            ILogger<AuthService> logger,
            IStringLocalizer<SharedResources> sr)
        {
            _userRepo = userRepo;
            _configuration = configuration;
            _logger = logger;
            _sr = sr;
        }

        public async Task<LoginResponse> Register(RegisterRequest request)
        {
            // Validate username and password are not empty
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                throw new InvalidOperationException(_sr["UsernameRequired"]);
            }

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            {
                throw new InvalidOperationException(_sr["PasswordRequired"]);
            }

            // Check if username already exists
            try
            {
                _logger.LogInformation(_sr["CheckingForExistingUser"], request.Username);
                var existingUser = await _userRepo.GetByUsernameAsync(request.Username);
                if (existingUser != null)
                {
                    _logger.LogWarning(_sr["UsernameAlreadyExistsLog"], request.Username);
                    throw new InvalidOperationException(_sr["UsernameAlreadyExistsError"]);
                }
                _logger.LogInformation(_sr["UsernameAvailable"], request.Username);
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw username exists exception
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _sr["DatabaseErrorCheckingUsername"], request.Username);
                throw;
            }

            // Hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create new user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Password = hashedPassword,
                Currency = 5000// Default starting currency
            };

            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();

            // Reload user to get the database-set CreatedAt timestamp
            await _userRepo.ReloadAsync(user);

            // Generate JWT token for the newly registered user
            var token = GenerateJwtToken(user);

            // Return login response with JWT token
            return new LoginResponse
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresIn = _configuration.GetSection("Jwt")["ExpirationMinutes"] != null
                    ? int.Parse(_configuration.GetSection("Jwt")["ExpirationMinutes"]!) * 60
                    : 3600,
                User = new UserResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Currency = user.Currency,
                    CreatedAt = user.CreatedAt
                }
            };
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            // Find user by username
            var user = await _userRepo.GetByUsernameAsync(request.Username);
            if (user == null)
            {
                throw new UnauthorizedAccessException(_sr["InvalidCredentialsError"]);
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                throw new UnauthorizedAccessException(_sr["InvalidCredentialsError"]);
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);

            // Return login response with actual database values
            return new LoginResponse
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresIn = _configuration.GetSection("Jwt")["ExpirationMinutes"] != null
                    ? int.Parse(_configuration.GetSection("Jwt")["ExpirationMinutes"]!) * 60
                    : 3600,
                User = new UserResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Currency = user.Currency,
                    CreatedAt = user.CreatedAt
                }
            };
        }

        public async Task Logout(Guid userId)
        {
            // For JWT, logout is typically handled client-side by removing the token
            // No server-side action needed unless using token blacklisting
            // For now, this is a no-op
            await Task.CompletedTask;
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            // Try to get secret key from environment variable first, fallback to configuration
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                ?? jwtSettings["SecretKey"];

            // Validate we have a secret key
            if (string.IsNullOrEmpty(secretKey) || secretKey == "UseEnvironmentVariable")
            {
                throw new InvalidOperationException(_sr["JwtSecretKeyNotSetError"]);
            }

            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}