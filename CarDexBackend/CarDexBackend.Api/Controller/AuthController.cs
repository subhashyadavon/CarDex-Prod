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
        /// 201 Created on success, 400 Bad Request for invalid input,
        /// or 409 Conflict if the username already exists.
        /// </returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(LoginResponse), 201)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 409)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var response = await _authService.Register(request);
                return CreatedAtAction(nameof(Register), new { id = response.User.Id }, response);
            }
            catch (DbUpdateException ex)
            {
                // Database-specific errors (transient failures, connection issues, etc.)
                return StatusCode(503, new ErrorResponse 
                { 
                    Message = "Database error occurred. Please try again later." 
                });
            }
            catch (InvalidOperationException ex)
            {
                // Check if this is actually a database connection error
                if (ex.Message.Contains("transient failure") || ex.Message.Contains("exception has been raised"))
                {
                    return StatusCode(503, new ErrorResponse 
                    { 
                        Message = "Database connection failed. Please try again later." 
                    });
                }
                // Username already exists or validation error
                return Conflict(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Invalid input or unknown error
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
        }
        
        /// <summary>
        /// Authenticates a user and issues an access token.
        /// </summary>
        /// <param name="request">The login request containing credentials.</param>
        /// <returns>
        /// 200 OK with login response on success, or 401 Unauthorized if credentials are invalid.
        /// </returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.Login(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Invalid credentials
                return Unauthorized(new ErrorResponse { Message = ex.Message });
            }
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
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
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
