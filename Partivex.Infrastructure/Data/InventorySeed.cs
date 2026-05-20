using Partivex.Domain.Entities;

namespace Partivex.Infrastructure.Data;

public static class InventorySeed
{
    public static async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var vendor = dbContext.Vendors.FirstOrDefault(v => v.Email == "inventory@himalauto.test");
        if (vendor is null)
        {
            vendor = new Vendor
            {
                Name = "Himal Auto Traders",
                ContactPerson = "Store Manager",
                Email = "inventory@himalauto.test",
                Phone = "+977-9800000000",
                Address = "Kathmandu",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Vendors.Add(vendor);
        }

        var now = DateTime.UtcNow;
        var seedParts = new[]
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

        var existingPartCodes = dbContext.Parts
            .Select(part => part.PartCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var part in seedParts.Where(part => !existingPartCodes.Contains(part.PartCode)))
        {
            dbContext.Parts.Add(part);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var activeParts = dbContext.Parts
            .Where(part => part.IsActive)
            .ToArray();

        var inventoryItemsByPartNumber = dbContext.InventoryItems
            .ToDictionary(item => item.PartNumber, StringComparer.OrdinalIgnoreCase);

        var inventoryChanged = false;
        foreach (var part in activeParts)
        {
            if (!inventoryItemsByPartNumber.TryGetValue(part.PartCode, out var inventoryItem))
            {
                inventoryItem = new InventoryItem
                {
                    PartNumber = part.PartCode,
                    Name = part.Name,
                    Category = part.Category,
                    VendorName = vendor.Name,
                    StorageLocation = "Main Store",
                    QuantityInStock = part.CurrentStock,
                    ReorderLevel = part.MinimumStockLevel,
                    UnitCost = part.UnitPrice,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                dbContext.InventoryItems.Add(inventoryItem);
                inventoryChanged = true;
                continue;
            }

            inventoryItem.Name = part.Name;
            inventoryItem.Category = part.Category;
            inventoryItem.VendorName = vendor.Name;
            inventoryItem.QuantityInStock = part.CurrentStock;
            inventoryItem.ReorderLevel = part.MinimumStockLevel;
            inventoryItem.UnitCost = part.UnitPrice;
            inventoryItem.UpdatedAt = DateTimeOffset.UtcNow;
            inventoryChanged = true;
        }

        if (inventoryChanged)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
