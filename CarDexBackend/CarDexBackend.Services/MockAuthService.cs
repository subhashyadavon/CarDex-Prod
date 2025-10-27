using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;
using System.Linq;

namespace CarDexBackend.Services
{
    /// <summary>
    /// Mock implementation of <see cref="IAuthService"/> used for local development and testing.
    /// </summary>
    /// <remarks>
    /// This mock service simulates authentication functionality without a real database or JWT system.
    /// Users are stored in an in-memory dictionary for temporary session-based testing.
    /// </remarks>
    public class MockAuthService : IAuthService
    {
        /// <summary>
        /// Simulated in-memory user storage.
        /// </summary>
        /// <remarks>
        /// The dictionary maps usernames to their corresponding password and user profile.
        /// This is volatile data that resets whenever the API restarts.
        /// Using static to persist across service instances (for testing).
        /// </remarks>
        private static readonly Dictionary<string, (string Password, UserResponse User)> _users = new();

        /// <summary>
        /// Registers a new user with the provided credentials.
        /// </summary>
        /// <param name="request">The registration request containing username and password.</param>
        /// <returns>
        /// A <see cref="UserResponse"/> object representing the newly registered user.
        /// </returns>
        /// <exception cref="InvalidOperationException">Thrown when the username already exists.</exception>
        public Task<LoginResponse> Register(RegisterRequest request)
        {
            if (_users.ContainsKey(request.Username))
            {
                throw new InvalidOperationException("Username already exists.");
            }

            var user = new UserResponse
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Currency = 1000, // default starting currency for new users
                CreatedAt = DateTime.UtcNow
            };

            _users[request.Username] = (request.Password, user);
            
            var response = new LoginResponse
            {
                AccessToken = Guid.NewGuid().ToString(), // mock token; no real JWT generation
                ExpiresIn = 3600, // mock expiry time (1 hour)
                User = user
            };
            
            return Task.FromResult(response);
        }

        /// <summary>
        /// Authenticates a user with the provided login credentials.
        /// </summary>
        /// <param name="request">The login request containing username and password.</param>
        /// <returns>
        /// A <see cref="LoginResponse"/> containing an access token and associated user details.
        /// </returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when credentials are invalid.</exception>
        public Task<LoginResponse> Login(LoginRequest request)
        {
            if (!_users.TryGetValue(request.Username, out var record))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }
            
            // Simple password comparison (since mock stores plain password)
            if (record.Password != request.Password)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            var response = new LoginResponse
            {
                AccessToken = Guid.NewGuid().ToString(), // mock token; no real JWT generation
                ExpiresIn = 3600, // mock expiry time (1 hour)
                User = record.User
            };

            return Task.FromResult(response);
        }

        /// <summary>
        /// Logs out a user by invalidating their token.
        /// </summary>
        /// <param name="userId">The user ID associated with the token to invalidate.</param>
        /// <remarks>
        /// In this mock implementation, logout performs no action.
        /// The real implementation would remove or blacklist the token from cache or database.
        /// </remarks>
        public Task Logout(Guid userId)
        {
            // No action required for the mock — tokens aren't persisted.
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets all users stored in memory (for debugging purposes only).
        /// </summary>
        public Task<List<UserResponse>> GetAllUsers()
        {
            var users = _users.Values.Select(u => u.User).ToList();
            return Task.FromResult(users);
        }
    }
}
