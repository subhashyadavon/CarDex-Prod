using CarDexBackend.Shared.Dtos.Responses;

namespace CarDexBackend.Services
{
    /// <summary>
    /// Defines operations for retrieving and browsing card collections.
    /// </summary>
    /// <remarks>
    /// This interface abstracts all collection-related data operations for both mock and production implementations.
    /// It provides functionality to list all available collections and retrieve detailed information about a specific one.
    /// </remarks>
    public interface ICollectionService
    {
        /// <summary>
        /// Retrieves a list of all available collections.
        /// </summary>
        /// <returns>
        /// A <see cref="CollectionListResponse"/> containing basic information about each collection.
        /// </returns>
        /// <remarks>
        /// Used to populate collection browsing views or allow users to choose from different collection themes.
        /// </remarks>
        Task<CollectionListResponse> GetAllCollections();

        /// <summary>
        /// Retrieves detailed information about a specific collection.
        /// </summary>
        /// <param name="collectionId">The unique identifier of the collection to retrieve.</param>
        /// <returns>
        /// A <see cref="CollectionDetailedResponse"/> with all cards and metadata associated with the specified collection.
        /// </returns>
        /// <exception cref="KeyNotFoundException">Thrown if the collection cannot be found.</exception>
        Task<CollectionDetailedResponse> GetCollectionById(Guid collectionId);
    }
}
