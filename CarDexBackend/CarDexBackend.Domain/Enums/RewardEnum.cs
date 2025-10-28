namespace CarDexBackend.Domain.Enums
{
    /// <summary>
    /// Represents the type of reward that can be given to users in the CarDex system.
    /// Rewards are distributed for various achievements, activities, and completed trades.
    /// Each reward type has different redemption mechanics and value.
    /// </summary>
    public enum RewardEnum
    {
        /// <summary>
        /// Pack reward - the user receives a card pack that can be opened to obtain random vehicle cards.
        /// When claimed, the ItemId references the specific pack being awarded.
        /// </summary>
        PACK,

        /// <summary>
        /// Currency reward - the user receives in-game currency directly.
        /// The Amount property specifies how much currency is awarded.
        /// </summary>
        CURRENCY,

        /// <summary>
        /// Card from trade - the user receives a specific card obtained through a completed trade transaction.
        /// When claimed, the ItemId references the specific card being awarded.
        /// </summary>
        CARD_FROM_TRADE,

        /// <summary>
        /// Currency from trade - the user receives in-game currency from completing a trade.
        /// Used to track currency earned specifically through trading activities.
        /// </summary>
        CURRENCY_FROM_TRADE
    }
}
