/// <summary>
/// Marker class used for resource localization within the backend.
/// </summary>
/// <remarks>
/// This class serves as the type parameter for <see cref="IStringLocalizer{T}"/> to
/// identify and load shared resource (.resx) files located in the <c>Resources</c> folder.
/// It does not contain any members and is never instantiated.
/// </remarks>
namespace CarDexBackend.Services.Resources
{
    public class SharedResources { }
}