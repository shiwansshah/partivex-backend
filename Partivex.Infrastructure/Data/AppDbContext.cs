using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Partivex.Domain.Entities;

namespace Partivex.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<CustomerHistory> CustomerHistories { get; set; } = null!; // Stores history rows.

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    public DbSet<InventoryStockChange> InventoryStockChanges => Set<InventoryStockChange>();

    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();

    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems => Set<PurchaseInvoiceItem>();

    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().ToTable("Customers");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");

        builder.Entity<ApplicationUser>()
            .HasIndex(user => user.PhoneNumber)
            .IsUnique();

        builder.Entity<ActivityLog>(entity =>
        {
            entity.ToTable("ActivityLogs");
            entity.HasKey(log => log.Id);
            entity.Property(log => log.UserId).HasMaxLength(450);
            entity.Property(log => log.UserName).HasMaxLength(256);
            entity.Property(log => log.Role).HasMaxLength(80);
            entity.Property(log => log.Action).IsRequired().HasMaxLength(120);
            entity.Property(log => log.EntityName).HasMaxLength(120);
            entity.Property(log => log.EntityId).HasMaxLength(120);
            entity.Property(log => log.Description).HasMaxLength(500);
            entity.Property(log => log.IpAddress).HasMaxLength(80);
            entity.Property(log => log.CreatedAt).IsRequired();
            entity.HasIndex(log => log.CreatedAt);
            entity.HasIndex(log => log.Action);
        });

        builder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");
            entity.HasKey(permission => permission.Id);
            entity.Property(permission => permission.Name).IsRequired().HasMaxLength(120);
            entity.Property(permission => permission.Description).HasMaxLength(300);
            entity.Property(permission => permission.CreatedAt).IsRequired();
            entity.HasIndex(permission => permission.Name).IsUnique();
        });

        builder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(rolePermission => rolePermission.Id);
            entity.Property(rolePermission => rolePermission.RoleName).IsRequired().HasMaxLength(80);
            entity.Property(rolePermission => rolePermission.CreatedAt).IsRequired();
            entity.HasIndex(rolePermission => new { rolePermission.RoleName, rolePermission.PermissionId }).IsUnique();
            entity.HasOne(rolePermission => rolePermission.Permission)
                .WithMany(permission => permission.RolePermissions)
                .HasForeignKey(rolePermission => rolePermission.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<InventoryItem>(entity =>
        {
            entity.ToTable("InventoryItems");
            entity.Property(item => item.PartNumber).HasMaxLength(40);
            entity.Property(item => item.Name).HasMaxLength(120);
            entity.Property(item => item.Category).HasMaxLength(80);
            entity.Property(item => item.VendorName).HasMaxLength(120);
            entity.Property(item => item.StorageLocation).HasMaxLength(80);
            entity.Property(item => item.UnitCost).HasPrecision(18, 2);
            entity.HasIndex(item => item.PartNumber).IsUnique();
        });

        builder.Entity<PurchaseInvoice>(entity =>
        {
            entity.ToTable("PurchaseInvoices");
            entity.Property(inv => inv.InvoiceNumber).HasMaxLength(40);
            entity.Property(inv => inv.VendorName).HasMaxLength(120);
            entity.Property(inv => inv.Status).HasMaxLength(20);
            entity.Property(inv => inv.CreatedBy).HasMaxLength(120);
            entity.Property(inv => inv.Notes).HasMaxLength(500);
            entity.HasIndex(inv => inv.InvoiceNumber).IsUnique();
        });

        builder.Entity<PurchaseInvoiceItem>(entity =>
        {
            entity.ToTable("PurchaseInvoiceItems");
            entity.Property(item => item.UnitCost).HasPrecision(18, 2);
            entity.HasOne(item => item.PurchaseInvoice)
                .WithMany(inv => inv.Items)
                .HasForeignKey(item => item.PurchaseInvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.InventoryItem)
                .WithMany()
                .HasForeignKey(item => item.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<InventoryStockChange>(entity =>
        {
            entity.ToTable("InventoryStockChanges");
            entity.Property(change => change.ChangeType).HasMaxLength(40);
            entity.Property(change => change.ReferenceCode).HasMaxLength(40);
            entity.Property(change => change.ChangedBy).HasMaxLength(120);
            entity.Property(change => change.Notes).HasMaxLength(240);
            entity.HasOne(change => change.InventoryItem)
                .WithMany(item => item.StockChanges)
                .HasForeignKey(change => change.InventoryItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("Vehicles");
            entity.HasKey(v => v.Id);
            entity.Property(v => v.Name).IsRequired().HasMaxLength(100);
            entity.Property(v => v.Number).IsRequired().HasMaxLength(50);
            entity.Property(v => v.ImageUrl).HasMaxLength(500);

            entity.HasOne(v => v.Customer)
                .WithMany()
                .HasForeignKey(v => v.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CustomerHistory>(entity => // Configures history entity.
        { // Begins history configuration.
            entity.ToTable("CustomerHistories"); // Maps history table.

            entity.HasKey(history => history.Id); // Configures primary key.

            entity.Property(history => history.CustomerId).IsRequired(); // Requires customer id.

            entity.Property(history => history.Description).IsRequired(); // Requires description.

            entity.Property(history => history.CreatedAt).IsRequired(); // Requires creation timestamp.

            entity.HasOne<ApplicationUser>() // Configures customer relation.
                .WithMany() // Uses no navigation collection.
                .HasForeignKey(history => history.CustomerId) // Uses customer id FK.
                .OnDelete(DeleteBehavior.Cascade); // Deletes history with customer.
        }); // Ends history configuration.
    }
}
