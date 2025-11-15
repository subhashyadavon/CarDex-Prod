using System.Security.Claims;
using CarDexBackend.Services;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;

namespace CarDexBackend.Api.Services
{
    /// <summary>
    /// Provides access to information about the currently authenticated user, including authentication state and user identifier, based on the HTTP context.
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets a value indicating whether the current user is authenticated.
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                return user?.Identity?.IsAuthenticated ?? false;
            }
        }
        
        /// <summary>
        /// Gets the unique identifier of the currently authenticated user.
        /// </summary>
        public Guid UserId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User
                    ?? throw new UnauthorizedAccessException("No HTTP context available.");

                if (!(user.Identity?.IsAuthenticated ?? false))
                    throw new UnauthorizedAccessException("User is not authenticated.");

                var userIdClaim = user.FindFirst(JwtRegisteredClaimNames.Sub) ?? user.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                    throw new UnauthorizedAccessException("User ID claim not found.");

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                    throw new UnauthorizedAccessException("User ID claim is invalid.");

                return userId;
            }
        }
    }
}
