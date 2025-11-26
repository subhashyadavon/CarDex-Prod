namespace CarDexBackend.Shared.Dtos.Responses
{
    /// <summary>
    /// Represents a user's progress in a specific collection.
    /// </summary>
    /// <remarks>
    /// Shows how many unique vehicles the user owns from a collection
    /// and calculates completion percentage.
    /// </remarks>
    public class CollectionProgressDto
    {
        /// <summary>
        /// Unique identifier of the collection.
        /// </summary>
        public Guid CollectionId { get; set; }

        /// <summary>
        /// Name of the collection (e.g., "JDM Legends", "90s Icons").
        /// </summary>
        public string CollectionName { get; set; } = string.Empty;

        /// <summary>
        /// Image URL or base64-encoded image representing the collection.
        /// </summary>
        public string CollectionImage { get; set; } = string.Empty;

        /// <summary>
        /// Number of unique vehicles the user owns from this collection.
        /// </summary>
        public int OwnedVehicles { get; set; }

        /// <summary>
        /// Total number of unique vehicles available in this collection.
        /// </summary>
        public int TotalVehicles { get; set; }

        /// <summary>
        /// Completion percentage (0-100).
        /// Calculated as (OwnedVehicles / TotalVehicles) * 100.
        /// </summary>
        public int Percentage { get; set; }
    }

    /// <summary>
    /// Response containing collection progress for all collections where user owns at least one card.
    /// </summary>
    /// <remarks>
    /// Returned by <c>GET /users/{userId}/collection-progress</c>.
    /// Only includes collections where the user has at least one card.
    /// Collections are typically sorted by completion percentage (highest first).
    /// </remarks>
    public class CollectionProgressResponse
    {
        /// <summary>
        /// List of collection progress data.
        /// </summary>
        public IEnumerable<CollectionProgressDto> Collections { get; set; } = new List<CollectionProgressDto>();

        /// <summary>
        /// Total number of collections the user has cards from.
        /// </summary>
        public int TotalCollections { get; set; }
    }
}
