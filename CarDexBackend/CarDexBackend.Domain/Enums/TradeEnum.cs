namespace CarDexBackend.Domain.Enums
{
    /// <summary>
    /// Represents the type of trade offer in the CarDex trading system.
    /// Determines what a user is offering their card in exchange for:
    /// either another card or in-game currency.
    /// </summary>
    public enum TradeEnum
    {
        /// <summary>
        /// Card-for-card trade - the user wants to exchange their card for a specific other card.
        /// Requires specifying the card they want (WantCardId in OpenTrade or BuyerCardId in CompletedTrade).
        /// </summary>
        FOR_CARD,

        /// <summary>
        /// Card-for-currency trade - the user wants to sell their card for in-game currency.
        /// Requires specifying the sale price (Price property).
        /// </summary>
        FOR_PRICE
    }
}
