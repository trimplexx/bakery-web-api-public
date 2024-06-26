﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using bakery_web_api;

#nullable disable

namespace bakery_web_api.Migrations
{
    [DbContext(typeof(BakeryDbContext))]
    [Migration("20240316215114_MySqlInitial")]
    partial class MySqlInitial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseCollation("utf8mb4_0900_ai_ci")
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.HasCharSet(modelBuilder, "utf8mb4");

            modelBuilder.Entity("bakery_web_api.Models.Database.BlackListSession", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("ExpiredAt")
                        .HasColumnType("datetime(3)");

                    b.Property<string>("Token")
                        .HasColumnType("longtext");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .HasName("PRIMARY");

                    b.HasIndex(new[] { "UserId" }, "FK_BlackListSession_Users_UserId");

                    b.ToTable("BlackListSession", (string)null);
                });

            modelBuilder.Entity("bakery_web_api.Models.Database.Order", b =>
                {
                    b.Property<int>("OrderId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime?>("OrderDate")
                        .HasColumnType("datetime(3)");

                    b.Property<string>("Phone")
                        .HasColumnType("longtext");

                    b.Property<int?>("Status")
                        .HasColumnType("int");

                    b.HasKey("OrderId")
                        .HasName("PRIMARY");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("bakery_web_api.Models.Database.OrderProduct", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("OrderId")
                        .HasColumnType("int");

                    b.Property<int?>("ProductId")
                        .HasColumnType("int");

                    b.Property<int>("ProductQuantity")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .HasName("PRIMARY");

                    b.HasIndex(new[] { "OrderId" }, "FK_OrderProducts_Orders");

                    b.HasIndex(new[] { "ProductId" }, "FK_OrderProducts_Products");

                    b.ToTable("OrderProducts");
                });

            modelBuilder.Entity("bakery_web_api.Models.Database.Product", b =>
                {
                    b.Property<int>("ProductId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<string>("Image")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<decimal?>("Price")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<int?>("Weight")
                        .HasColumnType("int");

                    b.HasKey("ProductId")
                        .HasName("PRIMARY");

                    b.ToTable("Products", (string)null);
                });

            modelBuilder.Entity("bakery_web_api.Models.Database.ProductCategory", b =>
                {
                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.HasKey("ProductId", "CategoryId");

                    b.HasIndex("CategoryId");

                    b.ToTable("ProductCategories");
                });

            modelBuilder.Entity("bakery_web_api.Models.Database.ProductCategoryName", b =>
                {
                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.HasKey("CategoryId")
                        .HasName("PRIMARY");

                    b.ToTable("ProductCategoryName", (string)null);
                });

            modelBuilder.Entity("bakery_web_api.Models.Database.ProductsAvailability", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime?>("Date")
                        .HasColumnType("datetime(6)");

                    b.Property<int?>("OrderedQuantity")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<int?>("Quantity")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .HasName("PRIMARY");

                    b.HasIndex(new[] { "ProductId" }, "FK_ProductsAvailability_Products");

                    b.ToTable("ProductsAvailability", (string)null);
                });

            modelBuilder.Entity("bakery_web_api.Models.Database.ProductsNutritionalValue", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<double?>("Carbohydrates")
                        .HasColumnType("double");

                    b.Property<double?>("Fat")
                        .HasColumnType("double");

                    b.Property<int?>("Kcal")
                        .HasColumnType("int");

                    b.Property<int?>("Kj")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<double?>("Proteins")
                        .HasColumnType("double");

                    b.Property<double?>("Salt")
                        .HasColumnType("double");

                    b.Property<double?>("SaturatedFat")
                        .HasColumnType("double");

                    b.Property<double?>("Sugars")
                        .HasColumnType("double");

                    b.HasKey("Id")
                        .HasName("PRIMARY");

                    b.HasIndex(new[] { "ProductId" }, "FK_ProductsNutritionalValue_Products_ProductId");

                    b.ToTable("ProductsNutritionalValue", (string)null);
                });

            modelBuilder.Entity("bakery_web_api.Models.Database.Session", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime?>("ExpiredTime")
                        .HasColumnType("datetime(3)");

                    b.Property<string>("Token")
                        .HasColumnType("longtext");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .HasName("PRIMARY");

                    b.HasIndex(new[] { "UserId" }, "FK_Session_Users_UserId");

                    b.ToTable("Session", (string)null);
                });

            modelBuilder.Entity("bakery_web_api.Models.Database.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .HasColumnType("longtext");

                    b.Property<string>("FirstName")
                        .HasMaxLength(30)
                        .HasColumnType("varchar(30)");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsVerify")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("LastName")
                        .HasMaxLength(30)
                        .HasColumnType("varchar(30)");

                    b.Property<string>("Password")
                        .HasColumnType("longtext");

                    b.Property<string>("Phone")
                        .HasMaxLength(13)
                        .HasColumnType("varchar(13)");

                    b.Property<string>("Token")
                        .HasColumnType("longtext");

                    b.HasKey("UserId")
                        .HasName("PRIMARY");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("bakery_web_api.Models.Database.BlackListSession", b =>
                {
                    b.HasOne("bakery_web_api.Models.Database.User", "User")
                        .WithMany("BlackListSessions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("bakery_web_api.Models.Database.OrderProduct", b =>
                {
                    b.HasOne("bakery_web_api.Models.Database.Order", "Order")
                        .WithMany("OrderProducts")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_OrderProducts_Orders");

                    b.HasOne("bakery_web_api.Models.Database.Product", "Product")
                        .WithMany("OrderProducts")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("FK_OrderProducts_Products");

                    b.Navigation("Order");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("bakery_web_api.Models.Database.ProductCategory", b =>
                {
                    b.HasOne("bakery_web_api.Models.Database.ProductCategoryName", "CategoryName")
                        .WithMany("ProductCategories")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("bakery_web_api.Models.Database.Product", "Product")
                        .WithMany("ProductCategories")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_Products_ProductCategory");

                    b.Navigation("CategoryName");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("bakery_web_api.Models.Database.ProductsAvailability", b =>
                {
                    b.HasOne("bakery_web_api.Models.Database.Product", "Product")
                        .WithMany("ProductsAvailabilities")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_ProductsAvailability_Products");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("bakery_web_api.Models.Database.ProductsNutritionalValue", b =>
                {
                    b.HasOne("bakery_web_api.Models.Database.Product", "Product")
                        .WithMany("ProductsNutritionalValues")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Product");
                });

            modelBuilder.Entity("bakery_web_api.Models.Database.Session", b =>
                {
                    b.HasOne("bakery_web_api.Models.Database.User", "User")
                        .WithMany("Sessions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("bakery_web_api.Models.Database.Order", b =>
                {
                    b.Navigation("OrderProducts");
                });

            modelBuilder.Entity("bakery_web_api.Models.Database.Product", b =>
                {
                    b.Navigation("OrderProducts");

                    b.Navigation("ProductCategories");

                    b.Navigation("ProductsAvailabilities");

                    b.Navigation("ProductsNutritionalValues");
                });

            modelBuilder.Entity("bakery_web_api.Models.Database.ProductCategoryName", b =>
                {
                    b.Navigation("ProductCategories");
                });

            modelBuilder.Entity("bakery_web_api.Models.Database.User", b =>
                {
                    b.Navigation("BlackListSessions");

                    b.Navigation("Sessions");
                });
#pragma warning restore 612, 618
        }
    }
}
