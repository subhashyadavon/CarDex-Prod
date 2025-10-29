using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;

namespace CarDexBackend.Services
{
    /// <summary>
    /// Defines authentication-related operations for user registration, login, and logout.
    /// </summary>
    /// <remarks>
    /// This interface is implemented by both mock and real authentication services.
    /// It provides methods to create new user accounts, verify credentials,
    /// and manage session or token-based authentication state.
    /// </remarks>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="request">The registration details including username and password.</param>
        /// <returns>A <see cref="LoginResponse"/> containing JWT token and user information.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the username is already taken.</exception>
        Task<LoginResponse> Register(RegisterRequest request);

        /// <summary>
        /// Authenticates a user with provided credentials.
        /// </summary>
        /// <param name="request">The login credentials (username and password).</param>
        /// <returns>
        /// A <see cref="LoginResponse"/> containing authentication token and user information.
        /// </returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if credentials are invalid.</exception>
        Task<LoginResponse> Login(LoginRequest request);

        /// <summary>
        /// Logs out a user and invalidates their authentication session.
        /// </summary>
        /// <param name="userId">The unique ID of the user to log out.</param>
        Task Logout(Guid userId);
    }
}
