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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().ToTable("Customers");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");

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
    }
}
