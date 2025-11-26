using System;
using CarDexBackend.Domain.Enums;

namespace CarDexBackend.Domain.Entities
{

    public class CompletedTrade
    {
        
        public Guid Id { get; set; }
        public TradeEnum Type { get; set; }
        public Guid SellerUserId { get; set; }
        public Guid SellerCardId { get; set; }
        public Guid BuyerUserId { get; set; }
        public Guid? BuyerCardId { get; set; }
        public int Price { get; set; }
        public DateTime ExecutedDate { get; set; }


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

        private void ValidateTrade()
        {
            if (Type == TradeEnum.FOR_CARD && BuyerCardId == null)
                throw new InvalidOperationException("BuyerCardId must be provided for FOR_CARD trades.");

            if (Type == TradeEnum.FOR_PRICE && Price <= 0)
                throw new InvalidOperationException("Price must be greater than 0 for FOR_PRICE trades.");
        }
    }
}
