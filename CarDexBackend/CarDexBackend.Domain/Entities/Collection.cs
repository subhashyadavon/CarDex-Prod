using System;
using System.Collections.Generic;

namespace CarDexBackend.Domain.Entities
{
    /// <summary>
    /// Represents a collection of vehicles that can be packaged and opened by users.
    /// Collections group related vehicles together (e.g., by brand, model series, or theme),
    /// and users can purchase packs from collections to obtain random vehicle cards.
    /// </summary>
    public class Collection
    {
        /// <summary>
        /// Unique identifier for the collection.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the collection (e.g., "Nissan Skyline Series", "Supercar Collection").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Image URL or base64-encoded image representing the collection.
        /// Used for display purposes in the user interface.
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Price in in-game currency required to open a pack from this collection.
        /// Determines the cost for users to obtain cards from this collection.
        /// </summary>
        public int PackPrice { get; set; }

        /// <summary>
        /// Array of vehicle identifiers that belong to this collection.
        /// When a user opens a pack from this collection, they receive a card for one of these vehicles.
        /// Stored as array to match the database schema.
        /// </summary>
        public Guid[] Vehicles { get; set; } = Array.Empty<Guid>();

        /// <summary>
        /// Creates a new Collection instance with the specified properties.
        /// </summary>
        /// <param name="id">Unique identifier for the collection.</param>
        /// <param name="name">Name of the collection.</param>
        /// <param name="image">Image URL or base64-encoded image for the collection.</param>
        /// <param name="packPrice">Price in in-game currency to open a pack from this collection.</param>
        /// <param name="vehicles">Array of vehicle identifiers belonging to this collection.</param>
        public Collection(Guid id, string name, string image, int packPrice, Guid[] vehicles)
        {
            Id = id;
            Name = name;
            Image = image;
            PackPrice = packPrice;
            Vehicles = vehicles ?? Array.Empty<Guid>();
        }

        /// <summary>
        /// Parameterless constructor required for Entity Framework Core.
        /// Initializes all properties to their default values.
        /// </summary>
        public Collection()
        {
            Id = Guid.Empty;
            Name = string.Empty;
            Image = string.Empty;
            PackPrice = 0;
            Vehicles = Array.Empty<Guid>();
        }

        /// <summary>
        /// Adds a vehicle to the collection by its identifier.
        /// If the vehicle is already in the collection, no action is taken.
        /// </summary>
        /// <param name="vehicleId">The unique identifier of the vehicle to add.</param>
        /// <exception cref="ArgumentException">Thrown when vehicleId is an empty GUID.</exception>
        public void AddVehicle(Guid vehicleId)
        {
            if (vehicleId == Guid.Empty) throw new ArgumentException("Invalid vehicle ID");
            var list = Vehicles.ToList();
            if (!list.Contains(vehicleId))
            {
                list.Add(vehicleId);
                Vehicles = list.ToArray();
            }
        }

        /// <summary>
        /// Adds a vehicle to the collection by Vehicle entity reference.
        /// Convenience overload that extracts the ID from the Vehicle object.
        /// </summary>
        /// <param name="vehicle">The Vehicle entity to add to the collection.</param>
        /// <exception cref="ArgumentNullException">Thrown when vehicle is null.</exception>
        public void AddVehicle(Vehicle? vehicle)
        {
            if (vehicle == null) throw new ArgumentNullException(nameof(vehicle));
            AddVehicle(vehicle.Id);
        }

        /// <summary>
        /// Removes a vehicle from the collection by its identifier.
        /// If the vehicle is not in the collection, no action is taken.
        /// </summary>
        /// <param name="vehicleId">The unique identifier of the vehicle to remove.</param>
        public void RemoveVehicle(Guid vehicleId)
        {
            Vehicles = Vehicles.Where(v => v != vehicleId).ToArray();
        }

        /// <summary>
        /// Checks whether the collection contains a specific vehicle.
        /// </summary>
        /// <param name="vehicleId">The unique identifier of the vehicle to check.</param>
        /// <returns>True if the collection contains the vehicle; otherwise, false.</returns>
        public bool HasVehicle(Guid vehicleId) => Vehicles.Contains(vehicleId);
    }
}
