using System;
using CarDexBackend.Domain.Enums;

namespace CarDexBackend.Domain.Entities
{
    /// <summary>
    /// Represents an active trade offer in the CarDex trading system.
    /// An open trade indicates a user is willing to exchange a card for either another card or in-game currency.
    /// These trades are visible to other users who can accept them to complete the transaction.
    /// </summary>
    public class OpenTrade
    {
        /// <summary>
        /// Unique identifier for the open trade offer.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Type of trade being offered.
        /// FOR_CARD: User wants to exchange their card for a specific card.
        /// FOR_PRICE: User wants to sell their card for in-game currency.
        /// </summary>
        public TradeEnum Type { get; set; }

        /// <summary>
        /// Unique identifier of the user who created this trade offer.
        /// This is the owner of the card being offered for trade.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Unique identifier of the card being offered in this trade.
        /// This card will be transferred to the buyer upon trade completion.
        /// </summary>
        public Guid CardId { get; set; }

        /// <summary>
        /// Price in in-game currency requested for the card (only for FOR_PRICE trades).
        /// When Type is FOR_PRICE, this is the amount the seller wants for their card.
        /// When Type is FOR_CARD, this is typically 0.
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// Unique identifier of the card wanted in exchange (only for FOR_CARD trades).
        /// When Type is FOR_CARD, this specifies which card the seller wants to receive.
        /// When Type is FOR_PRICE, this is null.
        /// </summary>
        public Guid? WantCardId { get; set; }

        /// <summary>
        /// Parameterless constructor required for Entity Framework Core.
        /// Initializes all properties to their default values.
        /// </summary>
        public OpenTrade()
        {
            Id = Guid.Empty;
            Type = TradeEnum.FOR_PRICE;
            UserId = Guid.Empty;
            CardId = Guid.Empty;
            Price = 0;
            WantCardId = null;
        }

        /// <summary>
        /// Creates a new OpenTrade instance with the specified properties.
        /// </summary>
        /// <param name="id">Unique identifier for the open trade.</param>
        /// <param name="type">Type of trade (FOR_CARD or FOR_PRICE).</param>
        /// <param name="userId">Unique identifier of the user creating the trade.</param>
        /// <param name="cardId">Unique identifier of the card being offered.</param>
        /// <param name="price">Price in in-game currency (for FOR_PRICE trades). Defaults to 0.</param>
        /// <param name="wantCardId">Unique identifier of the desired card (for FOR_CARD trades). Defaults to null.</param>
        public OpenTrade(Guid id, TradeEnum type, Guid userId, Guid cardId, int price = 0, Guid? wantCardId = null)
        {
            Id = id;
            Type = type;
            UserId = userId;
            CardId = cardId;
            Price = price;
            WantCardId = wantCardId;

            ValidateTrade();
        }

        /// <summary>
        /// Validates the trade offer to ensure all required fields are provided based on the trade type.
        /// Ensures data integrity for open trade offers.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when FOR_CARD trade is missing WantCardId, or when FOR_PRICE trade has invalid price.
        /// </exception>
        private void ValidateTrade()
        {
            if (Type == TradeEnum.FOR_CARD && WantCardId == null)
                throw new InvalidOperationException("WantCardId must be provided for FOR_CARD trades.");
            
            if (Type == TradeEnum.FOR_PRICE && Price <= 0)
                throw new InvalidOperationException("Price must be greater than 0 for FOR_PRICE trades.");
        }

        /// <summary>
        /// Updates the price for a FOR_PRICE trade.
        /// Allows the seller to modify the asking price of their trade offer.
        /// </summary>
        /// <param name="newPrice">The new price to set. Must be greater than 0.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to update price on a FOR_CARD trade, or when newPrice is 0 or negative.
        /// </exception>
        public void UpdatePrice(int newPrice)
        {
            if (Type != TradeEnum.FOR_PRICE) throw new InvalidOperationException("Only FOR_PRICE trades can update price.");
            if (newPrice <= 0) throw new InvalidOperationException("Price must be greater than 0.");
            Price = newPrice;
        }
    }
}
