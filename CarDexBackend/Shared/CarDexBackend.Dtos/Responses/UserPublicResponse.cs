namespace CarDexBackend.Shared.Dtos.Responses
{
    /// <summary>
    /// Represents public profile information for a user.
    /// </summary>
    /// <remarks>
    /// This lightweight DTO is used when displaying non-sensitive user information,  
    /// such as in leaderboards, trade listings, or public profile views.  
    /// Returned by endpoints like <c>GET /users/{userId}</c>.
    /// </remarks>
    public class UserPublicResponse
    {
        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The publicly visible username of the user.
        /// </summary>
        /// <remarks>
        /// Sensitive information such as email or currency balance is intentionally excluded.
        /// </remarks>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The timestamp when the user account was originally created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The current currency balance of the user.
        /// </summary>
        /// <remarks>
        /// This is only populated when the user is viewing their own profile.
        /// </remarks>
        public int? Currency { get; set; }
    }
}
