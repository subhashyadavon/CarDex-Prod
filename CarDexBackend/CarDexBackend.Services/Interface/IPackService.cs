using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;

namespace CarDexBackend.Services
{
    /// <summary>
    /// Defines operations for purchasing, retrieving, and opening card packs.
    /// </summary>
    /// <remarks>
    /// This interface abstracts pack-related operations for both mock and production implementations.
    /// It handles purchasing packs with in-game currency, retrieving specific pack details, and opening packs to generate cards.
    /// </remarks>
    public interface IPackService
    {
        /// <summary>
        /// Purchases a new pack for the specified collection.
        /// </summary>
        /// <param name="request">The purchase request containing the target collection ID and user information.</param>
        /// <returns>
        /// A <see cref="PackPurchaseResponse"/> containing the purchased pack details and updated user currency balance.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the collection ID is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the user does not have enough currency to complete the purchase.</exception>
        Task<PackPurchaseResponse> PurchasePack(PackPurchaseRequest request);

        /// <summary>
        /// Retrieves details of a specific pack by ID.
        /// </summary>
        /// <param name="packId">The unique identifier of the pack to retrieve.</param>
        /// <returns>
        /// A <see cref="PackDetailedResponse"/> containing information about the specified pack, including its collection, purchase date, and value.
        /// </returns>
        /// <exception cref="KeyNotFoundException">Thrown if the pack cannot be found.</exception>
        Task<PackDetailedResponse> GetPackById(Guid packId);

        /// <summary>
        /// Opens a pack to generate and return its contained cards.
        /// </summary>
        /// <param name="packId">The unique identifier of the pack to open.</param>
        /// <returns>
        /// A <see cref="PackOpenResponse"/> containing the cards obtained from opening the pack.
        /// </returns>
        /// <exception cref="KeyNotFoundException">Thrown if the pack cannot be found.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the pack has already been opened.</exception>
        Task<PackOpenResponse> OpenPack(Guid packId);
    }
}
