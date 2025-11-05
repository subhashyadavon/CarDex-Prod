using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CarDexBackend.Services;
using CarDexBackend.Shared.Dtos.Responses;
using CarDexBackend.Shared.Dtos.Requests;
using CarDexDatabase;
using CarDexBackend.Domain.Enums;
using Npgsql;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarDexBackend.Services.Resources;

namespace DefaultNamespace
{
    public class PackServiceTest : IDisposable
    {
        private readonly CarDexDbContext _context;
        private readonly PackService _packService;

        //Used ChatGPT to get the base code and to get help seeding the data
        public PackServiceTest()
        {
            // Use In-Memory Database for isolated testing
            var options = new DbContextOptionsBuilder<CarDexDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_PackService_" + Guid.NewGuid())
                .Options;

            _context = new CarDexDbContext(options);
            _packService = new PackService(_context, new NullStringLocalizer<SharedResources>());

            // Seed test data
            SeedTestData();
        }

        // Dispose method to clean up the DbContext after each test
        public void Dispose()
        {
            _context.Dispose();
        }

        private void SeedTestData()
        {
            // Add test vehicles first
            var vehicle1 = new CarDexBackend.Domain.Entities.Vehicle
            {
                Id = Guid.NewGuid(), 
                Year = "2021",
                Make = "Tesla",
                Model = "Model S",
                Value = 70000
            };

            var vehicle2 = new CarDexBackend.Domain.Entities.Vehicle
            {
                Id = Guid.NewGuid(),
                Year = "2020",
                Make = "Ford",
                Model = "Mustang",
                Value = 50000
            };

            var vehicle3 = new CarDexBackend.Domain.Entities.Vehicle
            {
                Id = Guid.NewGuid(),
                Year = "2022",
                Make = "Chevrolet",
                Model = "Camaro",
                Value = 60000
            };

            _context.Vehicles.Add(vehicle1);
            _context.Vehicles.Add(vehicle2);
            _context.Vehicles.Add(vehicle3);
            _context.SaveChanges();
            
            // Add test collections with actual vehicle IDs
            var collection1 = new CarDexBackend.Domain.Entities.Collection
            {
                Id = Guid.NewGuid(),
                Name = "Collection 1",
                Vehicles = new Guid[] { vehicle1.Id, vehicle2.Id, vehicle3.Id },  
                PackPrice = 500
            };

            var collection2 = new CarDexBackend.Domain.Entities.Collection
            {
                Id = Guid.NewGuid(),
                Name = "Collection 2",
                Vehicles = new Guid[] { vehicle1.Id },  
                PackPrice = 300
            };

            _context.Collections.Add(collection1);
            _context.Collections.Add(collection2);
            _context.SaveChanges();

            // Add test users including the hardcoded test user from PackService
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var user1 = new CarDexBackend.Domain.Entities.User
            {
                Id = testUserId,
                Username = "TestUser",
                Password = "Password123",
                Currency = 1000 
            };

            _context.Users.Add(user1);
            _context.SaveChanges();
        }

        // Test for PurchasePack
        [Fact]
        public async Task PurchasePack_ShouldDeductUserCurrencyAndCreatePack()
        {
            // Arrange
            var collection = _context.Collections.First();
            var user = _context.Users.First();
            var initialCurrency = user.Currency;

            var request = new PackPurchaseRequest
            {
                CollectionId = collection.Id
            };

            // Act
            var result = await _packService.PurchasePack(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(initialCurrency - collection.PackPrice, result.UserCurrency); 
            Assert.Equal(collection.Id, result.Pack.CollectionId); 
        }

        // Test for GetPackById
        [Fact]
        public async Task GetPackById_ShouldReturnCorrectPackDetails()
        {
            // Arrange - Create pack with Collection 1 which has 3 vehicles
            var collection = _context.Collections.First(c => c.Name == "Collection 1");
            var user = _context.Users.First();
            
            var pack = new CarDexBackend.Domain.Entities.Pack(
                Guid.NewGuid(),
                user.Id,
                collection.Id,
                500); 
            _context.Packs.Add(pack);
            _context.SaveChanges();

            // Act
            var result = await _packService.GetPackById(pack.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(pack.Id, result.Id);
            Assert.Equal(3, result.PreviewCards.Count()); 
            Assert.Equal(pack.CollectionId, result.CollectionId); 
            Assert.False(result.IsOpened); 
        }

        // Test for OpenPack
        [Fact]
        public async Task OpenPack_ShouldGenerateCardsAndMarkPackAsOpened()
        {
            // Arrange - Create pack with existing collection and user
            var collection = _context.Collections.First();
            var user = _context.Users.First();
            
            var pack = new CarDexBackend.Domain.Entities.Pack(
                Guid.NewGuid(),
                user.Id,
                collection.Id,
                500); 
            _context.Packs.Add(pack);
            _context.SaveChanges();

            // Act
            var result = await _packService.OpenPack(pack.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Cards.Count()); 
            Assert.True(result.Pack.IsOpened);
            
            // Verify pack is still in database but marked as opened
            var openedPack = await _context.Packs.FindAsync(pack.Id);
            Assert.NotNull(openedPack);
            Assert.True(openedPack.IsOpened);
        }

        [Fact]
        public async Task PurchasePack_ShouldThrowWhenCollectionNotFound()
        {
            // Arrange
            var request = new PackPurchaseRequest
            {
                CollectionId = Guid.NewGuid() // Non-existent collection
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _packService.PurchasePack(request));
        }

        [Fact]
        public async Task PurchasePack_ShouldThrowWhenUserNotFound()
        {
            // Arrange - Remove the test user
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var user = await _context.Users.FindAsync(testUserId);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            var collection = _context.Collections.First();
            var request = new PackPurchaseRequest { CollectionId = collection.Id };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _packService.PurchasePack(request));
        }

        [Fact]
        public async Task PurchasePack_ShouldThrowWhenInsufficientCurrency()
        {
            // Arrange
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var user = _context.Users.First(u => u.Id == testUserId);
            user.Currency = 100; // Not enough
            _context.SaveChanges();

            var collection = _context.Collections.First();
            var request = new PackPurchaseRequest { CollectionId = collection.Id };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _packService.PurchasePack(request));
        }

        [Fact]
        public async Task GetPackById_ShouldThrowWhenPackNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _packService.GetPackById(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetPackById_ShouldThrowWhenCollectionNotFound()
        {
            // Arrange
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var pack = new CarDexBackend.Domain.Entities.Pack(
                Guid.NewGuid(), testUserId, Guid.NewGuid(), 500);
            _context.Packs.Add(pack);
            _context.SaveChanges();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _packService.GetPackById(pack.Id));
        }

        [Fact]
        public async Task GetPackById_ShouldReturnPackWithPreviewCards()
        {
            // Arrange
            var collection = _context.Collections.First(c => c.Name == "Collection 1");
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var pack = new CarDexBackend.Domain.Entities.Pack(
                Guid.NewGuid(), testUserId, collection.Id, 500);
            _context.Packs.Add(pack);
            _context.SaveChanges();

            // Act
            var result = await _packService.GetPackById(pack.Id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.PreviewCards.Count() <= 3);
        }

        [Fact]
        public async Task OpenPack_ShouldThrowWhenPackNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _packService.OpenPack(Guid.NewGuid()));
        }

        [Fact]
        public async Task OpenPack_ShouldThrowWhenCollectionNotFound()
        {
            // Arrange
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var pack = new CarDexBackend.Domain.Entities.Pack(
                Guid.NewGuid(), testUserId, Guid.NewGuid(), 500);
            _context.Packs.Add(pack);
            _context.SaveChanges();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _packService.OpenPack(pack.Id));
        }

        [Fact]
        public async Task OpenPack_ShouldThrowWhenNoVehiclesInCollection()
        {
            // Arrange
            var collection = new CarDexBackend.Domain.Entities.Collection
            {
                Id = Guid.NewGuid(),
                Name = "Empty Collection",
                Vehicles = Array.Empty<Guid>(),
                PackPrice = 500
            };
            _context.Collections.Add(collection);
            _context.SaveChanges();

            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var pack = new CarDexBackend.Domain.Entities.Pack(
                Guid.NewGuid(), testUserId, collection.Id, 500);
            _context.Packs.Add(pack);
            _context.SaveChanges();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _packService.OpenPack(pack.Id));
        }

        [Fact]
        public async Task OpenPack_ShouldGenerateDifferentGrades()
        {
            // Arrange
            var collection = _context.Collections.First();
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var pack = new CarDexBackend.Domain.Entities.Pack(
                Guid.NewGuid(), testUserId, collection.Id, 500);
            _context.Packs.Add(pack);
            _context.SaveChanges();

            // Act - Run multiple times to test grade distribution
            var allGrades = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                var testPack = new CarDexBackend.Domain.Entities.Pack(
                    Guid.NewGuid(), testUserId, collection.Id, 500);
                _context.Packs.Add(testPack);
                _context.SaveChanges();
                
                var result = await _packService.OpenPack(testPack.Id);
                allGrades.AddRange(result.Cards.Select(c => c.Grade));
            }

            // Assert - Should have variety of grades
            Assert.Contains("FACTORY", allGrades);
        }
    }
}
