using System;
using CarDexBackend.Domain.Enums;

namespace CarDexBackend.Domain.Entities
{
    public class OpenTrade
    {
        public Guid Id { get; set; }
        public TradeEnum Type { get; set; }
        public Guid UserId { get; set; }       // The user who initiated the trade
        public Guid CardId { get; set; }       // Card offered in the trade
        public int Price { get; set; }         // Used if Type == FOR_PRICE
        public Guid? WantCardId { get; set; }  // Used if Type == FOR_CARD
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Parameterless constructor for EF Core
        public OpenTrade()
        {
            Id = Guid.Empty;
            Type = TradeEnum.FOR_PRICE;
            UserId = Guid.Empty;
            CardId = Guid.Empty;
            Price = 0;
            WantCardId = null;
            CreatedAt = DateTime.UtcNow;
        }

        // Constructor
        public OpenTrade(Guid id, TradeEnum type, Guid userId, Guid cardId, int price = 0, Guid? wantCardId = null)
        {
            Id = id;
            Type = type;
            UserId = userId;
            CardId = cardId;
            Price = price;
            WantCardId = wantCardId;
            CreatedAt = DateTime.UtcNow;

            ValidateTrade();
        }

        // Domain behavior: validate trade fields
        private void ValidateTrade()
        {
            if (Type == TradeEnum.FOR_CARD && WantCardId == null)
                throw new InvalidOperationException("WantCardId must be provided for FOR_CARD trades.");
            
            if (Type == TradeEnum.FOR_PRICE && Price <= 0)
                throw new InvalidOperationException("Price must be greater than 0 for FOR_PRICE trades.");
        }

        // Update trade price (for FOR_PRICE trades)
        public void UpdatePrice(int newPrice)
        {
            if (Type != TradeEnum.FOR_PRICE) throw new InvalidOperationException("Only FOR_PRICE trades can update price.");
            if (newPrice <= 0) throw new InvalidOperationException("Price must be greater than 0.");
            Price = newPrice;
        }
    }
}
