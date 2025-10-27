using System;
using CarDexBackend.Domain.Enums;

namespace CarDexBackend.Domain.Entities
{
    /// <summary>
    /// Represents a completed trade transaction between two users in the CarDex system.
    /// Records the details of successful trades, including what was exchanged and when.
    /// Used for trade history and tracking completed transactions.
    /// </summary>
    public class CompletedTrade
    {
        /// <summary>
        /// Unique identifier for the completed trade record.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Type of trade that was completed.
        /// FOR_CARD: Trade involved exchanging cards between users.
        /// FOR_PRICE: Trade involved selling a card for in-game currency.
        /// </summary>
        public TradeEnum Type { get; set; }

        /// <summary>
        /// Unique identifier of the user who initiated the trade (seller).
        /// This user owned the card that was offered for trade.
        /// </summary>
        public Guid SellerUserId { get; set; }

        /// <summary>
        /// Unique identifier of the card that was offered and sold by the seller.
        /// This card changes ownership from SellerUserId to BuyerUserId.
        /// </summary>
        public Guid SellerCardId { get; set; }

        /// <summary>
        /// Unique identifier of the user who accepted and completed the trade (buyer).
        /// This user receives the card from the seller.
        /// </summary>
        public Guid BuyerUserId { get; set; }

        /// <summary>
        /// Unique identifier of the card received by the seller (only for FOR_CARD trades).
        /// When Type is FOR_CARD, this is the card given to the seller in exchange.
        /// When Type is FOR_PRICE, this is null.
        /// </summary>
        public Guid? BuyerCardId { get; set; }

        /// <summary>
        /// Price paid in in-game currency (only for FOR_PRICE trades).
        /// When Type is FOR_PRICE, this is the amount the buyer paid for the card.
        /// When Type is FOR_CARD, this is typically 0.
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// Timestamp indicating when the trade was completed and executed.
        /// Used for trade history and audit purposes.
        /// </summary>
        public DateTime ExecutedDate { get; set; }

        /// <summary>
        /// Parameterless constructor required for Entity Framework Core.
        /// Initializes all properties to their default values.
        /// </summary>
        public CompletedTrade()
        {
            Id = Guid.Empty;
            Type = TradeEnum.FOR_PRICE;
            SellerUserId = Guid.Empty;
            SellerCardId = Guid.Empty;
            BuyerUserId = Guid.Empty;
            Price = 0;
            BuyerCardId = null;
            ExecutedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a new CompletedTrade instance with the specified properties.
        /// </summary>
        /// <param name="id">Unique identifier for the completed trade.</param>
        /// <param name="type">Type of trade (FOR_CARD or FOR_PRICE).</param>
        /// <param name="sellerUserId">Unique identifier of the seller.</param>
        /// <param name="sellerCardId">Unique identifier of the card being sold.</param>
        /// <param name="buyerUserId">Unique identifier of the buyer.</param>
        /// <param name="price">Price paid in in-game currency (for FOR_PRICE trades). Defaults to 0.</param>
        /// <param name="buyerCardId">Unique identifier of the card received by seller (for FOR_CARD trades). Defaults to null.</param>
        public CompletedTrade(
            Guid id,
            TradeEnum type,
            Guid sellerUserId,
            Guid sellerCardId,
            Guid buyerUserId,
            int price = 0,
            Guid? buyerCardId = null)
        {
            Id = id;
            Type = type;
            SellerUserId = sellerUserId;
            SellerCardId = sellerCardId;
            BuyerUserId = buyerUserId;
            Price = price;
            BuyerCardId = buyerCardId;
            ExecutedDate = DateTime.UtcNow;

            ValidateTrade();
        }

        /// <summary>
        /// Validates the trade to ensure all required fields are provided based on the trade type.
        /// Ensures data integrity for completed trades.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when FOR_CARD trade is missing BuyerCardId, or when FOR_PRICE trade has invalid price.
        /// </exception>
        private void ValidateTrade()
        {
            if (Type == TradeEnum.FOR_CARD && BuyerCardId == null)
                throw new InvalidOperationException("BuyerCardId must be provided for FOR_CARD trades.");

            if (Type == TradeEnum.FOR_PRICE && Price <= 0)
                throw new InvalidOperationException("Price must be greater than 0 for FOR_PRICE trades.");
        }
    }
}
