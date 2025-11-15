using CarDexBackend.Services;


    /*

    Hi Ian, add XML commenting to this file before you commit anything.

    or else you'll embarass yourself ;)



    */
public class TestCurrentUserService : ICurrentUserService
{
    public Guid UserId { get; set; }
    public bool IsAuthenticated { get; set; } = true;
}