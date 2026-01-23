namespace CarDexBackend.Shared.Dtos.Responses
{
    /// <summary>
    /// Represents the result of a successful card quick-sell transaction.
    /// </summary>
    public class CardQuickSellResponse
    {
        /// <summary>
        /// The unique identifier of the card that was sold.
        /// </summary>
        public Guid CardId { get; set; }

        /// <summary>
        /// The amount of currency received for selling the card.
        /// </summary>
        public int SellPrice { get; set; }

        /// <summary>
        /// The user's new in-game currency balance after the sale.
        /// </summary>
        public int NewUserCurrency { get; set; }
    }
}
