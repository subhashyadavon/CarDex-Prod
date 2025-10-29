using System;
using Microsoft.EntityFrameworkCore;
using CarDexBackend.Domain.Entities;
using CarDexBackend.Domain.Enums;

namespace CarDexDatabase
{
    public class CarDexDbContext : DbContext
    {
        // Constructor
        public CarDexDbContext(DbContextOptions<CarDexDbContext> options) : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<User> Users { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<Pack> Packs { get; set; }
        public DbSet<OpenTrade> OpenTrades { get; set; }
        public DbSet<CompletedTrade> CompletedTrades { get; set; }
        public DbSet<Reward> Rewards { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();
                
                entity.Property(e => e.Username)
                    .HasColumnName("username")
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(e => e.Password)
                    .HasColumnName("password")
                    .IsRequired()
                    .HasMaxLength(255);
                
                entity.Property(e => e.Currency)
                    .HasColumnName("currency")
                    .HasDefaultValue(0);
                
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("now()");
            });

            // Configure Card entity
            modelBuilder.Entity<Card>(entity =>
            {
                entity.ToTable("card");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();
                
                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();
                
                entity.Property(e => e.VehicleId)
                    .HasColumnName("vehicle_id")
                    .IsRequired();
                
                entity.Property(e => e.CollectionId)
                    .HasColumnName("collection_id")
                    .IsRequired();
                
                entity.Property(e => e.Grade)
                    .HasColumnName("grade")
                    .HasColumnType("grade_enum")
                    .IsRequired();
                
                entity.Property(e => e.Value)
                    .HasColumnName("value")
                    .HasDefaultValue(0);
                
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("now()");

                // Foreign keys
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Vehicle>()
                    .WithMany()
                    .HasForeignKey(e => e.VehicleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Collection>()
                    .WithMany()
                    .HasForeignKey(e => e.CollectionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Vehicle entity
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.ToTable("vehicle");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();
                
                entity.Property(e => e.Year)
                    .HasColumnName("year")
                    .IsRequired()
                    .HasMaxLength(10);
                
                entity.Property(e => e.Make)
                    .HasColumnName("make")
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(e => e.Model)
                    .HasColumnName("model")
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(e => e.Stat1)
                    .HasColumnName("stat1")
                    .HasDefaultValue(0);
                
                entity.Property(e => e.Stat2)
                    .HasColumnName("stat2")
                    .HasDefaultValue(0);
                
                entity.Property(e => e.Stat3)
                    .HasColumnName("stat3")
                    .HasDefaultValue(0);
                
                entity.Property(e => e.Value)
                    .HasColumnName("value")
                    .HasDefaultValue(0);
                
                entity.Property(e => e.Image)
                    .HasColumnName("image")
                    .HasColumnType("text");
            });

            // Configure Collection entity
            modelBuilder.Entity<Collection>(entity =>
            {
                entity.ToTable("collection");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();
                
                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .IsRequired()
                    .HasMaxLength(200);
                
                entity.Property(e => e.Image)
                    .HasColumnName("image")
                    .HasColumnType("text");
                
                entity.Property(e => e.PackPrice)
                    .HasColumnName("pack_price")
                    .HasDefaultValue(0);
                
                // Map Vehicles array to database array column
                entity.Property(e => e.Vehicles)
                    .HasColumnName("vehicles");
                
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("now()");
            });

            // Configure Pack entity
            modelBuilder.Entity<Pack>(entity =>
            {
                entity.ToTable("pack");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();
                
                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();
                
                entity.Property(e => e.CollectionId)
                    .HasColumnName("collection_id")
                    .IsRequired();
                
                entity.Property(e => e.Value)
                    .HasColumnName("value")
                    .HasDefaultValue(0);
                
                entity.Property(e => e.IsOpened)
                    .HasColumnName("is_opened")
                    .HasDefaultValue(false);
                
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("now()");

                // Foreign keys
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Collection>()
                    .WithMany()
                    .HasForeignKey(e => e.CollectionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure OpenTrade entity
            modelBuilder.Entity<OpenTrade>(entity =>
            {
                entity.ToTable("open_trade");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();
                
                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasColumnType("trade_enum")
                    .IsRequired();
                
                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();
                
                entity.Property(e => e.CardId)
                    .HasColumnName("card_id")
                    .IsRequired();
                
                entity.Property(e => e.Price)
                    .HasColumnName("price")
                    .HasDefaultValue(0);
                
                entity.Property(e => e.WantCardId)
                    .HasColumnName("want_card_id");
                
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("now()");

                // Foreign keys
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Card>()
                    .WithMany()
                    .HasForeignKey(e => e.CardId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CompletedTrade entity
            modelBuilder.Entity<CompletedTrade>(entity =>
            {
                entity.ToTable("completed_trade");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();
                
                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasColumnType("trade_enum")
                    .IsRequired();
                
                entity.Property(e => e.SellerUserId)
                    .HasColumnName("seller_user_id")
                    .IsRequired();
                
                entity.Property(e => e.SellerCardId)
                    .HasColumnName("seller_card_id")
                    .IsRequired();
                
                entity.Property(e => e.BuyerUserId)
                    .HasColumnName("buyer_user_id")
                    .IsRequired();
                
                entity.Property(e => e.BuyerCardId)
                    .HasColumnName("buyer_card_id");
                
                entity.Property(e => e.Price)
                    .HasColumnName("price")
                    .HasDefaultValue(0);
                
                entity.Property(e => e.ExecutedDate)
                    .HasColumnName("executed_date")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Foreign keys
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.SellerUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.BuyerUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Card>()
                    .WithMany()
                    .HasForeignKey(e => e.SellerCardId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Reward entity
            modelBuilder.Entity<Reward>(entity =>
            {
                entity.ToTable("rewards");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();
                
                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();
                
                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasColumnType("reward_enum")
                    .IsRequired();
                
                entity.Property(e => e.ItemId)
                    .HasColumnName("item_id");
                
                entity.Property(e => e.Amount)
                    .HasColumnName("amount")
                    .HasDefaultValue(0);
                
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("now()");
                
                entity.Property(e => e.ClaimedAt)
                    .HasColumnName("claimed_at");

                // Foreign keys
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure PostgreSQL enum types for EF Core
            // Specify the database enum type name and map to C# enum
            modelBuilder.HasPostgresEnum<GradeEnum>("grade_enum");
            modelBuilder.HasPostgresEnum<TradeEnum>("trade_enum");
            modelBuilder.HasPostgresEnum<RewardEnum>("reward_enum");
        }
    }
}
