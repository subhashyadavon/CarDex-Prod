using System;
using System.Collections.Generic;

namespace CarDexBackend.Domain.Entities
{
    public class Collection
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; } // URL or base64
        public int PackPrice { get; set; } // Price for opening a pack
        public Guid[] Vehicles { get; set; } = Array.Empty<Guid>(); // Array of vehicle UUIDs (matches DB schema)

        // Constructor
        public Collection(Guid id, string name, string image, int packPrice, Guid[] vehicles)
        {
            Id = id;
            Name = name;
            Image = image;
            PackPrice = packPrice;
            Vehicles = vehicles ?? Array.Empty<Guid>();
        }

        // Parameterless constructor for EF Core
        public Collection()
        {
            Id = Guid.Empty;
            Name = string.Empty;
            Image = string.Empty;
            PackPrice = 0;
            Vehicles = Array.Empty<Guid>();
        }

       // Domain Behavior

        // Add a vehicle to the collection
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

        // Add a vehicle to the collection 
        public void AddVehicle(Vehicle? vehicle)
        {
            if (vehicle == null) throw new ArgumentNullException(nameof(vehicle));
            AddVehicle(vehicle.Id);
        }

        // Remove a vehicle from the collection
        public void RemoveVehicle(Guid vehicleId)
        {
            Vehicles = Vehicles.Where(v => v != vehicleId).ToArray();
        }

        // Check if collection has a specific vehicle
        public bool HasVehicle(Guid vehicleId) => Vehicles.Contains(vehicleId);
    }
}
