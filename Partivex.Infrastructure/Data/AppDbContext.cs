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

    public DbSet<Vehicle> Vehicles { get; set; } = null!; // Stores vehicle rows.

    public DbSet<CustomerHistory> CustomerHistories { get; set; } = null!; // Stores history rows.

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().ToTable("Customers");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");

        builder.Entity<Vehicle>(entity => // Configures vehicle entity.
        { // Begins vehicle configuration.
            entity.ToTable("Vehicles"); // Maps vehicles table.

            entity.HasKey(vehicle => vehicle.Id); // Configures primary key.

            entity.Property(vehicle => vehicle.CustomerId).IsRequired(); // Requires customer id.

            entity.Property(vehicle => vehicle.VehicleNumber).IsRequired(); // Requires vehicle number.

            entity.Property(vehicle => vehicle.Model).IsRequired(false); // Allows optional model.

            entity.HasOne<ApplicationUser>() // Configures customer relation.
                .WithMany() // Uses no navigation collection.
                .HasForeignKey(vehicle => vehicle.CustomerId) // Uses customer id FK.
                .OnDelete(DeleteBehavior.Cascade); // Deletes vehicles with customer.
        }); // Ends vehicle configuration.

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
