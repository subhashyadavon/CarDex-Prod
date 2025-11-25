namespace CarDexBackend.Shared.Dtos.Responses
{
    /// <summary>
    /// Represents a reward that a user has earned through gameplay or trading.
    /// </summary>
    /// <remarks>
    /// Returned by the <c>GET /users/{userId}/rewards</c> endpoint.  
    /// Rewards can be of various types, such as packs, currency bonuses, or items received from trades or achievements.
    /// </remarks>
    public class UserRewardResponse
    {
        /// <summary>
        /// The unique identifier of the reward.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The unique identifier of the user who owns this reward.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The type of reward earned by the user.
        /// </summary>
        /// <remarks>
        /// Possible values include:
        /// <list type="bullet">
        /// <item><c>PACK</c> – a collectible pack reward</item>
        /// <item><c>CURRENCY</c> – in-game currency bonus</item>
        /// <item><c>CARD_FROM_TRADE</c> – card received through trade completion</item>
        /// <item><c>CURRENCY_FROM_TRADE</c> – currency gained through trade completion</item>
        /// </list>
        /// </remarks>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// The identifier of the item associated with the reward, if applicable.
        /// </summary>
        /// <remarks>
        /// For example, this could reference a pack or card rewarded to the user.  
        /// This field is <c>null</c> for non-item rewards such as pure currency.
        /// </remarks>
        public Guid? ItemId { get; set; }

        /// <summary>
        /// The numerical amount associated with the reward.
        /// </summary>
        /// <remarks>
        /// This value is typically used for currency-based rewards (e.g., credits, coins).  
        /// It may be <c>null</c> for item-based rewards such as packs or cards.
        /// </remarks>
        public int? Amount { get; set; }

        /// <summary>
        /// The timestamp when the reward was created or granted to the user.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The timestamp when the user claimed the reward, if applicable.
        /// </summary>
        /// <remarks>
        /// This field is <c>null</c> if the reward has not yet been claimed.
        /// </remarks>
        public DateTime? ClaimedAt { get; set; }
    }



    /// <summary>
    /// Represents a list of rewards earned by a user.
    /// </summary>
    /// <remarks>
    /// Returned by the <c>GET /users/{userId}/rewards</c> endpoint.  
    /// Can be filtered to show only claimed or unclaimed rewards.
    /// </remarks>
    public class UserRewardListResponse
    {
        /// <summary>
        /// The list of rewards belonging to the user.
        /// </summary>
        public IEnumerable<UserRewardResponse> Rewards { get; set; } = new List<UserRewardResponse>();

        /// <summary>
        /// The total number of rewards in the user's account matching the query.
        /// </summary>
        public int Total { get; set; }
    }
}
