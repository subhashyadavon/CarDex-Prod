namespace CarDexBackend.Shared.Dtos.Responses
{
    /// <summary>
    /// Represents a trade created by a user.
    /// </summary>
    /// <remarks>
    /// Each trade can either be listed for a card exchange (<c>FOR_CARD</c>)  
    /// or for sale at a specific price (<c>FOR_PRICE</c>).  
    /// Returned by the <c>GET /users/{userId}/trades</c> endpoint.
    /// </remarks>
    public class UserTradeResponse
    {
        /// <summary>
        /// The unique identifier of the trade.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The type of trade, indicating how the user wishes to trade the card.
        /// </summary>
        /// <remarks>
        /// Possible values include:
        /// <list type="bullet">
        /// <item><c>FOR_PRICE</c> – trade is for currency</item>
        /// <item><c>FOR_CARD</c> – trade is for another card</item>
        /// </list>
        /// </remarks>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// The identifier of the card being offered in this trade.
        /// </summary>
        public Guid CardId { get; set; }

        /// <summary>
        /// The asking price for the card, if this is a <c>FOR_PRICE</c> trade.
        /// </summary>
        /// <remarks>
        /// This field is <c>null</c> for card-for-card trades (<c>FOR_CARD</c>).
        /// </remarks>
        public int? Price { get; set; }

        /// <summary>
        /// The identifier of the card the user wants in exchange, if this is a <c>FOR_CARD</c> trade.
        /// </summary>
        /// <remarks>
        /// This field is <c>null</c> for trades based on price.
        /// </remarks>
        public Guid? WantCardId { get; set; }

        /// <summary>
        /// The timestamp when the trade was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Represents a list of active trades owned by a user.
    /// </summary>
    /// <remarks>
    /// Returned by the <c>GET /users/{userId}/trades</c> endpoint.  
    /// Does not include completed trades, which are accessed via <c>/trade-history</c>.
    /// </remarks>
    public class UserTradeListResponse
    {
        /// <summary>
        /// The list of active trades belonging to the user.
        /// </summary>
        public IEnumerable<UserTradeResponse> Trades { get; set; } = new List<UserTradeResponse>();

        /// <summary>
        /// The total number of active trades for the user.
        /// </summary>
        public int Total { get; set; }
    }
}
