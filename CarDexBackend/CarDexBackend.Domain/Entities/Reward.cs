using System;
using CarDexBackend.Domain.Enums;

namespace CarDexBackend.Domain.Entities
{
    /// <summary>
    /// Represents a reward that can be claimed by a user in the CarDex system.
    /// Rewards can be packs, currency, or items received from trades and are trackable
    /// with a claimed/unclaimed status. Once claimed, the reward is recorded with a timestamp.
    /// </summary>
    public class Reward
    {
        /// <summary>
        /// Unique identifier for the reward.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Unique identifier of the user who is eligible to claim this reward.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Type of reward being offered.
        /// PACK: The reward is a card pack.
        /// CURRENCY: The reward is in-game currency.
        /// CARD_FROM_TRADE: The reward is a card received from a trade.
        /// CURRENCY_FROM_TRADE: The reward is currency received from a trade.
        /// </summary>
        public RewardEnum Type { get; set; }

        /// <summary>
        /// Unique identifier of the item being rewarded (for pack/card rewards).
        /// When Type is PACK or CARD_FROM_TRADE, this references the pack or card ID.
        /// When Type is CURRENCY or CURRENCY_FROM_TRADE, this is null.
        /// </summary>
        public Guid? ItemId { get; set; }

        /// <summary>
        /// Quantity or amount of the reward.
        /// For currency rewards, this is the amount of in-game currency.
        /// For pack/card rewards, this typically represents the quantity being rewarded.
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// Timestamp indicating when the reward was claimed by the user.
        /// Null until the reward is claimed. Once set, the reward cannot be claimed again.
        /// </summary>
        public DateTime? ClaimedAt { get; set; }

        /// <summary>
        /// Parameterless constructor required for Entity Framework Core.
        /// Initializes all properties to their default values.
        /// </summary>
        public Reward()
        {
            Id = Guid.Empty;
            UserId = Guid.Empty;
            Type = RewardEnum.PACK;
            Amount = 0;
            ItemId = null;
            ClaimedAt = null;
        }

        /// <summary>
        /// Creates a new Reward instance with the specified properties.
        /// </summary>
        /// <param name="id">Unique identifier for the reward.</param>
        /// <param name="userId">Unique identifier of the user eligible for this reward.</param>
        /// <param name="type">Type of reward being offered.</param>
        /// <param name="amount">Quantity or amount of the reward.</param>
        /// <param name="itemId">Unique identifier of the item being rewarded (for pack/card rewards). Defaults to null.</param>
        public Reward(Guid id, Guid userId, RewardEnum type, int amount, Guid? itemId = null)
        {
            Id = id;
            UserId = userId;
            Type = type;
            Amount = amount;
            ItemId = itemId;
            ClaimedAt = null;
        }

        /// <summary>
        /// Marks the reward as claimed by setting the ClaimedAt timestamp.
        /// Once claimed, a reward cannot be claimed again.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when attempting to claim an already claimed reward.</exception>
        public void Claim()
        {
            if (ClaimedAt != null) throw new InvalidOperationException("Reward already claimed");
            ClaimedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks whether this reward has been claimed by the user.
        /// </summary>
        /// <returns>True if the reward has been claimed; otherwise, false.</returns>
        public bool IsClaimed() => ClaimedAt != null;
    }
}
