using CarDexBackend.Services;

/// <summary>
/// A dummy implementation of <see cref="ICurrentUserService"/> used for unit testing scenarios where a mockable current user context is required.
/// </summary>
public class TestCurrentUserService : ICurrentUserService
{
    public Guid UserId { get; set; }
    public bool IsAuthenticated { get; set; } = true;
}