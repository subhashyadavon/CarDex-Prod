using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;
using CarDexDatabase;
using CarDexBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using BCrypt.Net;
using Microsoft.Extensions.Logging;

namespace CarDexBackend.Services
{
    public class AuthService : IAuthService
    {
        private readonly CarDexDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(CarDexDbContext db, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _db = db;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginResponse> Register(RegisterRequest request)
        {
            // Validate username and password are not empty
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                throw new InvalidOperationException("Username is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            {
                throw new InvalidOperationException("Password must be at least 6 characters long.");
            }

            // Check if username already exists
            try
            {
                _logger.LogInformation("Checking for existing user: {Username}", request.Username);
                var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
                if (existingUser != null)
                {
                    _logger.LogWarning("Username already exists: {Username}", request.Username);
                    throw new InvalidOperationException("Username already exists.");
                }
                _logger.LogInformation("Username available: {Username}", request.Username);
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw username exists exception
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error while checking username: {Username}", request.Username);
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
                Currency = 0 // Default starting currency
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

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
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            // Find user by username
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);

            // Return login response
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
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
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
                throw new InvalidOperationException("JWT_SECRET_KEY environment variable is not set.");
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