using System;
using System.Collections.Generic;
using bakery_web_api.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace bakery_web_api;

public partial class BakeryDbContext : DbContext
{
    public BakeryDbContext()
    {
    }

    public BakeryDbContext(DbContextOptions<BakeryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BlackListSession> BlackListSessions { get; set; }
    
    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderProduct> OrderProducts { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategoryName> ProductCategoryNames { get; set; }
    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<ProductsAvailability> ProductsAvailabilities { get; set; }

    public virtual DbSet<ProductsNutritionalValue> ProductsNutritionalValues { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySql("name=ConnectionStrings:bakeryDbCon", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.36-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<BlackListSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("BlackListSession");

            entity.HasIndex(e => e.UserId, "FK_BlackListSession_Users_UserId");

            entity.Property(e => e.ExpiredAt).HasColumnType("datetime(3)");

            entity.HasOne(d => d.User).WithMany(p => p.BlackListSessions).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PRIMARY");

            entity.Property(e => e.OrderDate).HasColumnType("datetime(3)");
        });

        modelBuilder.Entity<OrderProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => e.OrderId, "FK_OrderProducts_Orders");

            entity.HasIndex(e => e.ProductId, "FK_OrderProducts_Products");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderProducts)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_OrderProducts_Orders");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderProducts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_OrderProducts_Products");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PRIMARY");

            entity.Property(e => e.Image).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Price).HasPrecision(18, 2);

            entity.HasMany(d => d.ProductCategories)
                .WithOne(p => p.Product)
                .HasForeignKey(p => p.ProductId)
                .HasConstraintName("FK_Products_ProductCategory");

            entity.ToTable("Products");
        });

        modelBuilder.Entity<ProductCategoryName>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PRIMARY");

            entity.ToTable("ProductCategoryName");

            entity.Property(e => e.CategoryId).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(50);
        });
        
        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => new { e.ProductId, e.CategoryId });

            entity.HasOne(d => d.Product)
                .WithMany(p => p.ProductCategories)
                .HasForeignKey(d => d.ProductId);

            entity.HasOne(d => d.CategoryName)
                .WithMany(p => p.ProductCategories)
                .HasForeignKey(d => d.CategoryId);
        });

        modelBuilder.Entity<ProductsAvailability>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ProductsAvailability");

            entity.HasIndex(e => e.ProductId, "FK_ProductsAvailability_Products");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductsAvailabilities)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_ProductsAvailability_Products");
        });

        modelBuilder.Entity<ProductsNutritionalValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ProductsNutritionalValue");

            entity.HasIndex(e => e.ProductId, "FK_ProductsNutritionalValue_Products_ProductId");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductsNutritionalValues).HasForeignKey(d => d.ProductId);
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Session");

            entity.HasIndex(e => e.UserId, "FK_Session_Users_UserId");

            entity.Property(e => e.ExpiredTime).HasColumnType("datetime(3)");

            entity.HasOne(d => d.User).WithMany(p => p.Sessions).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.Property(e => e.FirstName).HasMaxLength(30);
            entity.Property(e => e.LastName).HasMaxLength(30);
            entity.Property(e => e.Phone).HasMaxLength(13);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
