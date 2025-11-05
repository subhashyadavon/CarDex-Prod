using Microsoft.AspNetCore.Mvc;
using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;
using CarDexBackend.Services;
using Microsoft.EntityFrameworkCore;

namespace CarDexBackend.Api.Controllers
{
    /// <summary>
    /// Handles user authentication and account management operations.
    /// </summary>
    /// <remarks>
    /// Provides endpoints for user registration, login, and logout.
    /// In development, this uses a mock auth service for local testing.
    /// </remarks>
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="authService">The authentication service used for user management.</param>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user account and issues a JWT token.
        /// </summary>
        /// <param name="request">The registration request containing username and password.</param>
        /// <returns>
        /// 200 Ok on success, 400 Bad Request for invalid input,
        /// or 409 Conflict if the username already exists.
        /// </returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var response = await _authService.Register(request);
            return Ok(response);
        }
        
        /// <summary>
        /// Authenticates a user and issues an access token.
        /// </summary>
        /// <param name="request">The login request containing credentials.</param>
        /// <returns>
        /// 200 OK with login response on success, or 401 Unauthorized if credentials are invalid.
        /// </returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.Login(request);
            return Ok(response);
        }

        /// <summary>
        /// Logs out the current user by invalidating their session/token.
        /// </summary>
        /// <remarks>
        /// Extracts the user ID from the JWT token claims.
        /// For JWT, logout is primarily a client-side operation (removing the token).
        /// </remarks>
        /// <returns>204 No Content if logout is successful.</returns>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            // Extract user ID from JWT claim
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) 
                ?? User.FindFirst("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new ErrorResponse { Message = "Invalid token." });
            }

            await _authService.Logout(userId);
            return NoContent();
        }
    }
}
