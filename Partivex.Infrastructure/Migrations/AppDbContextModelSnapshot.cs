using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Partivex.Infrastructure.Data;

#nullable disable

namespace Partivex.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
            {
                b.Property<string>("Id")
                    .HasColumnType("text");

                b.Property<string>("ConcurrencyStamp")
                    .IsConcurrencyToken()
                    .HasColumnType("text");

                b.Property<string>("Name")
                    .HasMaxLength(256)
                    .HasColumnType("character varying(256)");

                b.Property<string>("NormalizedName")
                    .HasMaxLength(256)
                    .HasColumnType("character varying(256)");

                b.HasKey("Id");

                b.HasIndex("NormalizedName")
                    .IsUnique()
                    .HasDatabaseName("RoleNameIndex");

                b.ToTable("AspNetRoles", (string)null);
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("integer")
                    .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                b.Property<string>("ClaimType")
                    .HasColumnType("text");

                b.Property<string>("ClaimValue")
                    .HasColumnType("text");

                b.Property<string>("RoleId")
                    .IsRequired()
                    .HasColumnType("text");

                b.HasKey("Id");

                b.HasIndex("RoleId");

                b.ToTable("AspNetRoleClaims", (string)null);
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("integer")
                    .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                b.Property<string>("ClaimType")
                    .HasColumnType("text");

                b.Property<string>("ClaimValue")
                    .HasColumnType("text");

                b.Property<string>("UserId")
                    .IsRequired()
                    .HasColumnType("text");

                b.HasKey("Id");

                b.HasIndex("UserId");

                b.ToTable("AspNetUserClaims", (string)null);
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
            {
                b.Property<string>("LoginProvider")
                    .HasColumnType("text");

                b.Property<string>("ProviderKey")
                    .HasColumnType("text");

                b.Property<string>("ProviderDisplayName")
                    .HasColumnType("text");

                b.Property<string>("UserId")
                    .IsRequired()
                    .HasColumnType("text");

                b.HasKey("LoginProvider", "ProviderKey");

                b.HasIndex("UserId");

                b.ToTable("AspNetUserLogins", (string)null);
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
            {
                b.Property<string>("UserId")
                    .HasColumnType("text");

                b.Property<string>("RoleId")
                    .HasColumnType("text");

                b.HasKey("UserId", "RoleId");

                b.HasIndex("RoleId");

                b.ToTable("UserRoles", (string)null);
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
            {
                b.Property<string>("UserId")
                    .HasColumnType("text");

                b.Property<string>("LoginProvider")
                    .HasColumnType("text");

                b.Property<string>("Name")
                    .HasColumnType("text");

                b.Property<string>("Value")
                    .HasColumnType("text");

                b.HasKey("UserId", "LoginProvider", "Name");

                b.ToTable("AspNetUserTokens", (string)null);
            });

            modelBuilder.Entity("Partivex.Domain.Entities.ApplicationUser", b =>
            {
                b.Property<string>("Id")
                    .HasColumnType("text");

                b.Property<int>("AccessFailedCount")
                    .HasColumnType("integer");

                b.Property<string>("ConcurrencyStamp")
                    .IsConcurrencyToken()
                    .HasColumnType("text");

                b.Property<string>("Email")
                    .HasMaxLength(256)
                    .HasColumnType("character varying(256)");

                b.Property<bool>("EmailConfirmed")
                    .HasColumnType("boolean");

                b.Property<string>("FullName")
                    .IsRequired()
                    .HasColumnType("text");

                b.Property<bool>("LockoutEnabled")
                    .HasColumnType("boolean");

                b.Property<DateTimeOffset?>("LockoutEnd")
                    .HasColumnType("timestamp with time zone");

                b.Property<string>("NormalizedEmail")
                    .HasMaxLength(256)
                    .HasColumnType("character varying(256)");

                b.Property<string>("NormalizedUserName")
                    .HasMaxLength(256)
                    .HasColumnType("character varying(256)");

                b.Property<string>("PasswordHash")
                    .HasColumnType("text");

                b.Property<string>("PhoneNumber")
                    .HasColumnType("text");

                b.Property<bool>("PhoneNumberConfirmed")
                    .HasColumnType("boolean");

                b.Property<string>("SecurityStamp")
                    .HasColumnType("text");

                b.Property<bool>("TwoFactorEnabled")
                    .HasColumnType("boolean");

                b.Property<string>("UserName")
                    .HasMaxLength(256)
                    .HasColumnType("character varying(256)");

                b.HasKey("Id");

                b.HasIndex("NormalizedEmail")
                    .HasDatabaseName("EmailIndex");

                b.HasIndex("NormalizedUserName")
                    .IsUnique()
                    .HasDatabaseName("UserNameIndex");

                b.ToTable("Customers", (string)null);
            });

            modelBuilder.Entity("Partivex.Domain.Entities.InventoryItem", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("integer")
                    .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                b.Property<string>("Category")
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnType("character varying(80)");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(120)
                    .HasColumnType("character varying(120)");

                b.Property<string>("PartNumber")
                    .IsRequired()
                    .HasMaxLength(40)
                    .HasColumnType("character varying(40)");

                b.Property<int>("QuantityInStock")
                    .HasColumnType("integer");

                b.Property<int>("ReorderLevel")
                    .HasColumnType("integer");

                b.Property<string>("StorageLocation")
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnType("character varying(80)");

                b.Property<decimal>("UnitCost")
                    .HasPrecision(18, 2)
                    .HasColumnType("numeric(18,2)");

                b.Property<DateTimeOffset>("UpdatedAt")
                    .HasColumnType("timestamp with time zone");

                b.Property<string>("VendorName")
                    .IsRequired()
                    .HasMaxLength(120)
                    .HasColumnType("character varying(120)");

                b.HasKey("Id");

                b.HasIndex("PartNumber")
                    .IsUnique();

                b.ToTable("InventoryItems", (string)null);
            });

            modelBuilder.Entity("Partivex.Domain.Entities.InventoryStockChange", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("integer")
                    .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                b.Property<DateTimeOffset>("ChangedAt")
                    .HasColumnType("timestamp with time zone");

                b.Property<string>("ChangedBy")
                    .IsRequired()
                    .HasMaxLength(120)
                    .HasColumnType("character varying(120)");

                b.Property<string>("ChangeType")
                    .IsRequired()
                    .HasMaxLength(40)
                    .HasColumnType("character varying(40)");

                b.Property<int>("InventoryItemId")
                    .HasColumnType("integer");

                b.Property<string>("Notes")
                    .IsRequired()
                    .HasMaxLength(240)
                    .HasColumnType("character varying(240)");

                b.Property<int>("QuantityAfterChange")
                    .HasColumnType("integer");

                b.Property<int>("QuantityChanged")
                    .HasColumnType("integer");

                b.Property<string>("ReferenceCode")
                    .IsRequired()
                    .HasMaxLength(40)
                    .HasColumnType("character varying(40)");

                b.HasKey("Id");

                b.HasIndex("InventoryItemId");

                b.ToTable("InventoryStockChanges", (string)null);
            });

            modelBuilder.Entity("Partivex.Domain.Entities.PurchaseInvoice", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("integer")
                    .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                b.Property<DateTimeOffset>("CreatedAt")
                    .HasColumnType("timestamp with time zone");

                b.Property<string>("CreatedBy")
                    .IsRequired()
                    .HasMaxLength(120)
                    .HasColumnType("character varying(120)");

                b.Property<DateTimeOffset>("InvoiceDate")
                    .HasColumnType("timestamp with time zone");

                b.Property<string>("InvoiceNumber")
                    .IsRequired()
                    .HasMaxLength(40)
                    .HasColumnType("character varying(40)");

                b.Property<string>("Notes")
                    .IsRequired()
                    .HasMaxLength(500)
                    .HasColumnType("character varying(500)");

                b.Property<string>("Status")
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("character varying(20)");

                b.Property<string>("VendorName")
                    .IsRequired()
                    .HasMaxLength(120)
                    .HasColumnType("character varying(120)");

                b.HasKey("Id");

                b.HasIndex("InvoiceNumber")
                    .IsUnique();

                b.ToTable("PurchaseInvoices", (string)null);
            });

            modelBuilder.Entity("Partivex.Domain.Entities.PurchaseInvoiceItem", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("integer")
                    .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                b.Property<int>("InventoryItemId")
                    .HasColumnType("integer");

                b.Property<int>("PurchaseInvoiceId")
                    .HasColumnType("integer");

                b.Property<int>("Quantity")
                    .HasColumnType("integer");

                b.Property<decimal>("UnitCost")
                    .HasPrecision(18, 2)
                    .HasColumnType("numeric(18,2)");

                b.HasKey("Id");

                b.HasIndex("InventoryItemId");

                b.HasIndex("PurchaseInvoiceId");

                b.ToTable("PurchaseInvoiceItems", (string)null);
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
            {
                b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                    .WithMany()
                    .HasForeignKey("RoleId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
            {
                b.HasOne("Partivex.Domain.Entities.ApplicationUser", null)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
            {
                b.HasOne("Partivex.Domain.Entities.ApplicationUser", null)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
            {
                b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                    .WithMany()
                    .HasForeignKey("RoleId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("Partivex.Domain.Entities.ApplicationUser", null)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
            {
                b.HasOne("Partivex.Domain.Entities.ApplicationUser", null)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("Partivex.Domain.Entities.InventoryStockChange", b =>
            {
                b.HasOne("Partivex.Domain.Entities.InventoryItem", "InventoryItem")
                    .WithMany("StockChanges")
                    .HasForeignKey("InventoryItemId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("InventoryItem");
            });

            modelBuilder.Entity("Partivex.Domain.Entities.InventoryItem", b =>
            {
                b.Navigation("StockChanges");
            });

            modelBuilder.Entity("Partivex.Domain.Entities.PurchaseInvoiceItem", b =>
            {
                b.HasOne("Partivex.Domain.Entities.PurchaseInvoice", "PurchaseInvoice")
                    .WithMany("Items")
                    .HasForeignKey("PurchaseInvoiceId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("Partivex.Domain.Entities.InventoryItem", "InventoryItem")
                    .WithMany()
                    .HasForeignKey("InventoryItemId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Navigation("PurchaseInvoice");
                b.Navigation("InventoryItem");
            });

            modelBuilder.Entity("Partivex.Domain.Entities.PurchaseInvoice", b =>
            {
                b.Navigation("Items");
            });
        }
    }
}
