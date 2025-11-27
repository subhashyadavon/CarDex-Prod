namespace CarDexBackend.Shared.Dtos.Responses
{
    /// <summary>
    /// Represents summary information about a collection of cards.
    /// </summary>
    /// <remarks>
    /// This DTO provides collection details such as name, theme, and card count.  
    /// Use <see cref="CollectionDetailedResponse"/> to retrieve the full list of cards within a specific collection.
    /// </remarks>
    public class CollectionResponse
    {
        /// <summary>
        /// The unique identifier of the collection.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The display name of the collection.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The general theme or category of the collection.
        /// </summary>
        public string Theme { get; set; } = string.Empty;

        /// <summary>
        /// A descriptive summary of the collection and its focus.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The total number of cards included in this collection.
        /// </summary>
        public int CardCount { get; set; }

        /// <summary>
        /// The price of the collection.
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// The string URL of the image for the collection.
        /// </summary>
        public string ImageUrl { get; set; }
    }
}
