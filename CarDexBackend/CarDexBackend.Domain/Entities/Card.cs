using System;
using CarDexBackend.Domain.Enums;

namespace CarDexBackend.Domain.Entities
{
    /// <summary>
    /// Represents a trading card owned by a user in the CarDex system.
    /// Each card is associated with a specific vehicle, collection, and has a grade that determines its rarity and value.
    /// Cards can be traded between users or sold for in-game currency.
    /// </summary>
    public class Card
    {
        /// <summary>
        /// Unique identifier for the card.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Unique identifier of the user who owns this card.
        /// Public setter is required for Entity Framework Core and trade transfers.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Unique identifier of the vehicle associated with this card.
        /// Links to the Vehicle entity that represents the car depicted on the card.
        /// </summary>
        public Guid VehicleId { get; set; }

        /// <summary>
        /// Unique identifier of the collection this card belongs to.
        /// Cards are organized into collections, typically grouped by brand, model series, or theme.
        /// </summary>
        public Guid CollectionId { get; set; }

        /// <summary>
        /// The rarity grade of the card, which affects its value and desirability.
        /// Higher grades are rarer and more valuable.
        /// Possible values: FACTORY, LIMITED_RUN, NISMO
        /// </summary>
        public GradeEnum Grade { get; set; }

        /// <summary>
        /// Current market value of the card in in-game currency.
        /// This value may fluctuate based on market conditions, demand, and rarity.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Parameterless constructor required for Entity Framework Core.
        /// Initializes all properties to their default values.
        /// </summary>
        public Card()
        {
            Id = Guid.Empty;
            UserId = Guid.Empty;
            VehicleId = Guid.Empty;
            CollectionId = Guid.Empty;
            Grade = GradeEnum.FACTORY;
            Value = 0;
        }

        /// <summary>
        /// Creates a new Card instance with the specified properties.
        /// </summary>
        /// <param name="id">Unique identifier for the card.</param>
        /// <param name="userId">Unique identifier of the card owner.</param>
        /// <param name="vehicleId">Unique identifier of the associated vehicle.</param>
        /// <param name="collectionId">Unique identifier of the associated collection.</param>
        /// <param name="grade">Rarity grade of the card.</param>
        /// <param name="value">Market value of the card in in-game currency.</param>
        public Card(Guid id, Guid userId, Guid vehicleId, Guid collectionId, GradeEnum grade, int value)
        {
            Id = id;
            UserId = userId;
            VehicleId = vehicleId;
            CollectionId = collectionId;
            Grade = grade;
            Value = value;
        }

        /// <summary>
        /// Updates the market value of the card.
        /// This can be used to reflect changes in market conditions, demand, or other factors.
        /// </summary>
        /// <param name="newValue">The new market value. Must be non-negative.</param>
        /// <exception cref="InvalidOperationException">Thrown when newValue is negative.</exception>
        public void UpdateValue(int newValue)
        {
            if (newValue < 0) throw new InvalidOperationException("Value cannot be negative");
            Value = newValue;
        }

        /// <summary>
        /// Upgrades the card to a higher grade/rarity level.
        /// This operation is irreversible and increases the card's desirability.
        /// </summary>
        /// <param name="newGrade">The new grade to upgrade to. Must be higher than the current grade.</param>
        /// <exception cref="InvalidOperationException">Thrown when attempting to downgrade or keep the same grade.</exception>
        public void UpgradeGrade(GradeEnum newGrade)
        {
            if ((int)newGrade <= (int)Grade)
                throw new InvalidOperationException("Cannot downgrade or keep the same grade");
            Grade = newGrade;
        }
    }
}
