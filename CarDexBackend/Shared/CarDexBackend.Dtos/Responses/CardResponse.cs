namespace CarDexBackend.Shared.Dtos.Responses
{
    /// <summary>
    /// Represents a summarized view of a collectible card.
    /// </summary>
    /// <remarks>
    /// This DTO is used for listing or lightweight card responses.  
    /// For full details, use <see cref="CardDetailedResponse"/>.
    /// </remarks>
    public class CardResponse
    {
        /// <summary>
        /// The unique identifier of the card.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The display name of the card, usually the vehicle’s name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The rarity or tier classification of the card.
        /// </summary>
        public string Grade { get; set; } = string.Empty;

        /// <summary>
        /// The estimated in-game value of the card in currency units.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// The date and time when this card was created or added to the system.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        public string ImageUrl { get; set; }
    }
}
