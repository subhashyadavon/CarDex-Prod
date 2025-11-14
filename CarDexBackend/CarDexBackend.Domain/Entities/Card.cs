using System;
using CarDexBackend.Domain.Enums;

namespace CarDexBackend.Domain.Entities
{
    public class Card
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }        // Owner (public setter needed for trade transfers)
        public Guid VehicleId { get; set; }     // Vehicle associated with the card
        public Guid CollectionId { get; set; }  // Collection associated with the card
        public GradeEnum Grade { get; set; }    // Rarity/grade
        public int Value { get; set; }          // Current market value
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties for Entity Framework Core
        public Vehicle Vehicle { get; set; } = null!;  // Navigation to Vehicle entity
        public Collection Collection { get; set; } = null!;  // Navigation to Collection entity
        public User User { get; set; } = null!;  // Navigation to User entity

        // Parameterless constructor for EF Core
        public Card()
        {
            Id = Guid.Empty;
            UserId = Guid.Empty;
            VehicleId = Guid.Empty;
            CollectionId = Guid.Empty;
            Grade = GradeEnum.FACTORY;
            Value = 0;
            CreatedAt = DateTime.UtcNow;
            Vehicle = null!;
            Collection = null!;
            User = null!;
        }

        // Constructor
        public Card(Guid id, Guid userId, Guid vehicleId, Guid collectionId, GradeEnum grade, int value)
        {
            Id = id;
            UserId = userId;
            VehicleId = vehicleId;
            CollectionId = collectionId;
            Grade = grade;
            Value = value;
            CreatedAt = DateTime.UtcNow;
            Vehicle = null!;
            Collection = null!;
            User = null!;
        }

        // Domain behavior: update value (e.g., based on market)
        public void UpdateValue(int newValue)
        {
            if (newValue < 0) throw new InvalidOperationException("Value cannot be negative");
            Value = newValue;
        }

        // Domain behavior: upgrade grade
        public void UpgradeGrade(GradeEnum newGrade)
        {
            if ((int)newGrade <= (int)Grade)
                throw new InvalidOperationException("Cannot downgrade or keep the same grade");
            Grade = newGrade;
        }
    }
}
