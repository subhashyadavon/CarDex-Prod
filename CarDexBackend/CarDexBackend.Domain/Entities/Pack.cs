using System;

namespace CarDexBackend.Domain.Entities
{
    /// <summary>
    /// Represents a pack containing vehicle cards in the CarDex system.
    /// Users can acquire packs and open them to obtain random cards from the associated collection.
    /// Packs can be purchased with in-game currency or received as rewards.
    /// </summary>
    public class Pack
    {
        /// <summary>
        /// Unique identifier for the pack.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Unique identifier of the user who owns this pack.
        /// Only the owner can open the pack to reveal the cards inside.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Unique identifier of the collection this pack belongs to.
        /// When opened, the pack will contain a random card from this collection.
        /// </summary>
        public Guid CollectionId { get; set; }

        /// <summary>
        /// Current market value of the pack in in-game currency.
        /// This may reflect the collection's rarity, demand, or pack contents potential.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Parameterless constructor required for Entity Framework Core.
        /// Initializes all properties to their default values.
        /// </summary>
        public Pack()
        {
            Id = Guid.Empty;
            UserId = Guid.Empty;
            CollectionId = Guid.Empty;
            Value = 0;
        }

        /// <summary>
        /// Creates a new Pack instance with the specified properties.
        /// </summary>
        /// <param name="id">Unique identifier for the pack.</param>
        /// <param name="userId">Unique identifier of the pack owner.</param>
        /// <param name="collectionId">Unique identifier of the associated collection.</param>
        /// <param name="value">Market value of the pack in in-game currency.</param>
        public Pack(Guid id, Guid userId, Guid collectionId, int value)
        {
            Id = id;
            UserId = userId;
            CollectionId = collectionId;
            Value = value;
        }

        /// <summary>
        /// Updates the market value of the pack.
        /// This can be used to reflect changes in market conditions or pack scarcity.
        /// </summary>
        /// <param name="newValue">The new market value. Must be non-negative.</param>
        /// <exception cref="InvalidOperationException">Thrown when newValue is negative.</exception>
        public void UpdateValue(int newValue)
        {
            if (newValue < 0) throw new InvalidOperationException("Value cannot be negative");
            Value = newValue;
        }
    }
}
