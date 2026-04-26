using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Partivex.Domain.Entities;

namespace Partivex.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Customer>(entity =>
        {
            entity.HasKey(customer => customer.Id);

            entity.Property(customer => customer.FullName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(customer => customer.Phone)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(customer => customer.Email)
                .HasMaxLength(150);

            entity.Property(customer => customer.Address)
                .HasMaxLength(250);

            entity.Property(customer => customer.CreatedAt)
                .IsRequired();

            entity.HasIndex(customer => customer.Phone)
                .IsUnique();
        });

        builder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(vehicle => vehicle.Id);

            entity.Property(vehicle => vehicle.VehicleNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(vehicle => vehicle.Brand)
                .HasMaxLength(100);

            entity.Property(vehicle => vehicle.Model)
                .HasMaxLength(100);

            entity.Property(vehicle => vehicle.VehicleType)
                .HasMaxLength(50);

            entity.Property(vehicle => vehicle.Notes)
                .HasMaxLength(500);

            entity.HasIndex(vehicle => vehicle.VehicleNumber);

            entity.HasOne(vehicle => vehicle.Customer)
                .WithMany(customer => customer.Vehicles)
                .HasForeignKey(vehicle => vehicle.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
