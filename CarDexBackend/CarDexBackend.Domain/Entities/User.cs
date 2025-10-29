using System;
using System.Collections.Generic;

namespace CarDexBackend.Domain.Entities
{
    public class User
    {
        // Primary Identity
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }  
        public int Currency { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Parameterless constructor for EF Core
        public User()
        {
            Id = Guid.Empty;
            Username = string.Empty;
            Password = string.Empty;
            Currency = 0;
            CreatedAt = DateTime.UtcNow;
        }

        // Constructor
        public User(Guid id, string username, string password)
        {
            Id = id;
            Username = username;
            Password = password;
            Currency = 0;
            CreatedAt = DateTime.UtcNow;
        }

        // Domain Behaviors

        // Currency management
        public void AddCurrency(int amount)
        {
            if (amount < 0) throw new InvalidOperationException("Amount cannot be negative");
            Currency += amount;
        }

        public void DeductCurrency(int amount)
        {
            if (amount > Currency) throw new InvalidOperationException("Insufficient currency");
            Currency -= amount;
        }
    }
}

