namespace CarDexBackend.Services
{

        /*

    Hi Ian, add XML commenting to this file before you commit anything.

    or else you'll embarass yourself ;)



    */
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        bool IsAuthenticated { get; }
    }
}
