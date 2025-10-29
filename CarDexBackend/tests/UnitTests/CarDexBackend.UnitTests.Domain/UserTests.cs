
using System;
using Xunit;
using CarDexBackend.Domain.Entities;
using CarDexBackend.Domain.Enums;

namespace CarDexBackend.Domain.Tests
{
    public class UserTests
    {
        [Fact]
        public void AddCurrency_ShouldIncreaseBalance_WhenAmountIsPositive()
        {
            var user = new User(Guid.NewGuid(), "TestUser", "pass");
            user.AddCurrency(100);

            Assert.Equal(100, user.Currency);
        }

          [Fact]
        public void AddCurrency_ShouldThrow_WhenAmountIsNegative()
        {
            var user = new User(Guid.NewGuid(), "TestUser", "pass");
            Assert.Throws<InvalidOperationException>(() => user.AddCurrency(-10));
        }

        [Fact]
        public void DeductCurrency_ShouldReduceBalance_WhenEnoughCurrency()
        {
            var user = new User(Guid.NewGuid(), "TestUser", "pass");
            user.AddCurrency(100);
            user.DeductCurrency(40);

            Assert.Equal(60, user.Currency);
        }

        [Fact]
        public void DeductCurrency_ShouldThrow_WhenInsufficientFunds()
        {
            var user = new User(Guid.NewGuid(), "TestUser", "pass");
            Assert.Throws<InvalidOperationException>(() => user.DeductCurrency(50));
        }

        // NOTE: Card ownership, pack ownership, and trade management are now handled
        // via foreign keys in the database (card.user_id, pack.user_id, trade.seller_user_id).
        // These relationships are managed by the Service layer and database queries,
        // not by the User entity itself.
        
        [Fact]
        public void Constructor_ShouldInitializeWithZeroCurrency()
        {
            var user = new User(Guid.NewGuid(), "TestUser", "pass");
            Assert.Equal(0, user.Currency);
        }

        [Fact]
        public void Constructor_ShouldSetUsername()
        {
            var user = new User(Guid.NewGuid(), "TestUser", "pass");
            Assert.Equal("TestUser", user.Username);
        }

    }
}




