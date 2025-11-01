using CarDexBackend.Shared.Dtos.Responses;

namespace CarDexBackend.Services
{
    /// <summary>
    /// Defines methods for retrieving and browsing card data.
    /// </summary>
    /// <remarks>
    /// This interface abstracts card-related operations used by both mock and production implementations.
    /// It provides functionality for browsing, filtering, and retrieving detailed information about cards.
    /// </remarks>
    public interface ICardService
    {
        /// <summary>
        /// Retrieves a list of cards with optional filtering and pagination.
        /// </summary>
        /// <param name="userId">Optional filter to return only cards owned by a specific user.</param>
        /// <param name="collectionId">Optional filter to return only cards belonging to a specific collection.</param>
        /// <param name="vehicleId">Optional filter to return only cards for a specific vehicle.</param>
        /// <param name="grade">Optional filter to return only cards of a specific grade (e.g., FACTORY, LIMITED_RUN, NISMO).</param>
        /// <param name="minValue">Optional minimum card value filter.</param>
        /// <param name="maxValue">Optional maximum card value filter.</param>
        /// <param name="sortBy">Optional sort order (value_asc, value_desc, grade_asc, grade_desc, date_asc, date_desc).</param>
        /// <param name="limit">The maximum number of results to return per page. Defaults to 50.</param>
        /// <param name="offset">The number of results to skip for pagination. Defaults to 0.</param>
        /// <returns>
        /// A <see cref="CardListResponse"/> containing filtered and paginated card results.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if pagination parameters are invalid.</exception>
        Task<CardListResponse> GetAllCards(
            Guid? userId = null,
            Guid? collectionId = null,
            Guid? vehicleId = null,
            string? grade = null,
            int? minValue = null,
            int? maxValue = null,
            string? sortBy = "date_desc",
            int limit = 50,
            int offset = 0);

        /// <summary>
        /// Retrieves detailed information about a specific card.
        /// </summary>
        /// <param name="cardId">The unique identifier of the card to retrieve.</param>
        /// <returns>
        /// A <see cref="CardDetailedResponse"/> containing detailed information about the specified card.
        /// </returns>
        /// <exception cref="KeyNotFoundException">Thrown if the card does not exist.</exception>
        Task<CardDetailedResponse> GetCardById(Guid cardId);
    }
}
