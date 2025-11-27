namespace CarDexBackend.Shared.Dtos.Responses
{
    /// <summary>
    /// Represents a detailed view of a collectible card, extending <see cref="CardResponse"/>.
    /// </summary>
    /// <remarks>
    /// This DTO is returned when fetching full card details, including ownership and collection
    /// </remarks>
    public class CardDetailedResponse : CardResponse
    {
        /// <summary>
        /// A descriptive summary of the card or the vehicle it represents.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The unique identifier of the vehicle this card represents.
        /// </summary>
        public string VehicleId { get; set; } = string.Empty;

        /// <summary>
        /// The unique identifier of the collection this card belongs to.
        /// </summary>
        public string CollectionId { get; set; } = string.Empty;

        /// <summary>
        /// The unique identifier of the user who owns this card.
        /// </summary>
        public string OwnerId { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

    }
}
