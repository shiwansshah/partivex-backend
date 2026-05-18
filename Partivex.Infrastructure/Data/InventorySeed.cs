using Partivex.Domain.Entities;

namespace Partivex.Infrastructure.Data;

public static class InventorySeed
{
    public static async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (dbContext.Parts.Any() || dbContext.Vendors.Any())
        {
            return;
        }

        var vendor = new Vendor
        {
            Name = "Himal Auto Traders",
            ContactPerson = "Store Manager",
            Email = "inventory@himalauto.test",
            Phone = "+977-9800000000",
            Address = "Kathmandu",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var now = DateTime.UtcNow;
        var parts = new[]
        {
            new Part
            {
                Name = "Front Brake Pad Set",
                PartCode = "BRK-1001",
                Category = "Brake",
                CompatibleVehicle = "Passenger cars",
                UnitPrice = 2400m,
                MinimumStockLevel = 10,
                CurrentStock = 8,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Name = "Engine Oil Filter",
                PartCode = "FLT-2040",
                Category = "Engine",
                CompatibleVehicle = "Petrol vehicles",
                UnitPrice = 650m,
                MinimumStockLevel = 12,
                CurrentStock = 27,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        dbContext.Vendors.Add(vendor);
        dbContext.Parts.AddRange(parts);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
