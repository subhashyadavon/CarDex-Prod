using System;

namespace CarDexBackend.Domain.Entities
{
    /// <summary>
    /// Represents a vehicle (car) that can appear on trading cards in the CarDex system.
    /// Vehicles have various attributes including year, make, model, performance stats, and market value.
    /// Cards are created from vehicles and can be collected, traded, and displayed.
    /// </summary>
    public class Vehicle
    {
       
        public Guid Id { get; set; }
        public string Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Stat1 { get; set; }
        public int Stat2 { get; set; }
        public int Stat3 { get; set; }
        public int Value { get; set; }

        /// <summary>
        /// Image URL or base64-encoded image representing the vehicle.
        /// Used for display purposes on cards and in the UI.
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Parameterless constructor required for Entity Framework Core.
        /// Initializes all properties to their default values.
        /// </summary>
        public Vehicle()
        {
            Id = Guid.Empty;
            Year = string.Empty;
            Make = string.Empty;
            Model = string.Empty;
            Stat1 = 0;
            Stat2 = 0;
            Stat3 = 0;
            Value = 0;
            Image = string.Empty;
        }

        /// <summary>
        /// Creates a new Vehicle instance with the specified properties.
        /// </summary>
        /// <param name="id">Unique identifier for the vehicle.</param>
        /// <param name="year">Year of manufacture.</param>
        /// <param name="make">Manufacturer or brand.</param>
        /// <param name="model">Model name.</param>
        /// <param name="stat1">First performance statistic.</param>
        /// <param name="stat2">Second performance statistic.</param>
        /// <param name="stat3">Third performance statistic.</param>
        /// <param name="value">Market or rarity value in in-game currency.</param>
        /// <param name="image">Image URL or base64-encoded image.</param>
        public Vehicle(Guid id, string year, string make, string model, int stat1, int stat2, int stat3, int value, string image)
        {
            Id = id;
            Year = year;
            Make = make;
            Model = model;
            Stat1 = stat1;
            Stat2 = stat2;
            Stat3 = stat3;
            Value = value;
            Image = image;
        }

        /// <summary>
        /// Calculates an overall performance rating for the vehicle.
        /// Currently returns a simple average of the three statistics.
        /// Can be extended to use weighted formulas or more complex calculations.
        /// </summary>
        /// <returns>The calculated performance rating.</returns>
        public int CalculateRating()
        {
            return (Stat1 + Stat2 + Stat3) / 3;
        }

        /// <summary>
        /// Updates the market value of the vehicle based on demand, rarity, or other factors.
        /// This affects the base value when new cards of this vehicle are created.
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
