using System;
using CarDexBackend.Domain.Enums;

namespace CarDexBackend.Domain.Entities
{
    public class Reward
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public RewardEnum Type { get; set; }
        public Guid? ItemId { get; set; } 
        public int Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClaimedAt { get; set; } // Null until claimed

        // Parameterless constructor for EF Core
        public Reward()
        {
            Id = Guid.Empty;
            UserId = Guid.Empty;
            Type = RewardEnum.PACK;
            Amount = 0;
            ItemId = null;
            CreatedAt = DateTime.UtcNow;
            ClaimedAt = null;
        }

        // Constructor
        public Reward(Guid id, Guid userId, RewardEnum type, int amount, Guid? itemId = null)
        {
            Id = id;
            UserId = userId;
            Type = type;
            Amount = amount;
            ItemId = itemId;
            CreatedAt = DateTime.UtcNow;
            ClaimedAt = null;
        }

        // Domain behavior: claim reward
        public void Claim()
        {
            if (ClaimedAt != null) throw new InvalidOperationException("Reward already claimed");
            ClaimedAt = DateTime.UtcNow;
        }

        // Check if reward is already claimed
        public bool IsClaimed() => ClaimedAt != null;
    }
}
