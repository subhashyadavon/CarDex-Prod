namespace CarDexBackend.Services
{
    /// <summary>
    /// Provides access to information about the currently authenticated user, including authentication state and user identifier, based on the HTTP context.
    /// </summary>
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        bool IsAuthenticated { get; }
    }
}
