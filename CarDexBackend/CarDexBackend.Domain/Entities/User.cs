using System;
using System.Collections.Generic;

namespace CarDexBackend.Domain.Entities
{
    /// <summary>
    /// Represents a user account in the CarDex system.
    /// Users can own cards, packs, manage currency, and participate in trading.
    /// Contains domain behaviors for managing currency, cards, packs, and trades.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique identifier for the user account.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Username chosen by the user for their account.
        /// Used for authentication and display purposes.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Hashed password for user authentication.
        /// Should be stored securely and never exposed in plain text.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Amount of in-game currency the user possesses.
        /// Used to purchase packs, buy cards from trades, or acquire other items.
        /// </summary>
        public int Currency { get; set; }

        /// <summary>
        /// List of cards owned by this user.
        /// Aggregated collection of the user's card collection.
        /// Note: These are ignored in DbContext and stored as arrays in the database.
        /// </summary>
        public List<Card> OwnedCards { get; private set; } = new List<Card>();

        /// <summary>
        /// List of packs owned by this user that have not yet been opened.
        /// Aggregated collection of unopened card packs.
        /// Note: These are ignored in DbContext and stored as arrays in the database.
        /// </summary>
        public List<Pack> OwnedPacks { get; private set; } = new List<Pack>();

        /// <summary>
        /// List of active trade offers created by this user.
        /// Tracks all open trades the user has posted.
        /// Note: These are ignored in DbContext and stored as arrays in the database.
        /// </summary>
        public List<OpenTrade> OpenTrades { get; private set; } = new List<OpenTrade>();

        /// <summary>
        /// List of completed trades involving this user.
        /// Historical record of all completed trade transactions.
        /// Note: These are ignored in DbContext and stored as arrays in the database.
        /// </summary>
        public List<CompletedTrade> TradeHistory { get; private set; } = new List<CompletedTrade>();

        /// <summary>
        /// Parameterless constructor required for Entity Framework Core.
        /// Initializes all properties to their default values.
        /// </summary>
        public User()
        {
            Id = Guid.Empty;
            Username = string.Empty;
            Password = string.Empty;
            Currency = 0;
        }

        /// <summary>
        /// Creates a new User instance with the specified properties.
        /// </summary>
        /// <param name="id">Unique identifier for the user.</param>
        /// <param name="username">Username for the user account.</param>
        /// <param name="password">Hashed password for authentication.</param>
        public User(Guid id, string username, string password)
        {
            Id = id;
            Username = username;
            Password = password;
            Currency = 0;
        }

        /// <summary>
        /// Adds in-game currency to the user's account.
        /// Used when the user earns or receives currency from various sources.
        /// </summary>
        /// <param name="amount">The amount of currency to add. Must be non-negative.</param>
        /// <exception cref="InvalidOperationException">Thrown when amount is negative.</exception>
        public void AddCurrency(int amount)
        {
            if (amount < 0) throw new InvalidOperationException("Amount cannot be negative");
            Currency += amount;
        }

        /// <summary>
        /// Deducts in-game currency from the user's account.
        /// Used when the user purchases packs, cards, or other items.
        /// </summary>
        /// <param name="amount">The amount of currency to deduct.</param>
        /// <exception cref="InvalidOperationException">Thrown when the user has insufficient currency.</exception>
        public void DeductCurrency(int amount)
        {
            if (amount > Currency) throw new InvalidOperationException("Insufficient currency");
            Currency -= amount;
        }

        /// <summary>
        /// Adds a card to the user's collection.
        /// </summary>
        /// <param name="card">The card to add to the user's owned cards.</param>
        public void AddCard(Card card) => OwnedCards.Add(card);

        /// <summary>
        /// Checks whether the user owns a specific card.
        /// </summary>
        /// <param name="cardId">The unique identifier of the card to check.</param>
        /// <returns>True if the user owns the card; otherwise, false.</returns>
        public bool HasCard(Guid cardId) => OwnedCards.Exists(c => c.Id == cardId);

        /// <summary>
        /// Adds a pack to the user's collection of unopened packs.
        /// </summary>
        /// <param name="pack">The pack to add to the user's owned packs.</param>
        public void AddPack(Pack pack) => OwnedPacks.Add(pack);

        /// <summary>
        /// Adds an open trade offer to the user's list of active trades.
        /// </summary>
        /// <param name="trade">The open trade to add.</param>
        public void AddOpenTrade(OpenTrade trade) => OpenTrades.Add(trade);

        /// <summary>
        /// Records a completed trade and removes it from open trades.
        /// Moves the trade from OpenTrades to TradeHistory.
        /// </summary>
        /// <param name="trade">The completed trade to record.</param>
        public void CompleteTrade(CompletedTrade trade)
        {
            TradeHistory.Add(trade);
            OpenTrades.RemoveAll(t => t.Id == trade.Id);
        }
    }
}
