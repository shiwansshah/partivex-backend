using Partivex.Domain.Entities;

namespace Partivex.Infrastructure.Data;

public static class InventorySeed
{
    public static async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (dbContext.InventoryItems.Any())
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var items = new[]
        {
            new InventoryItem
            {
                PartNumber = "BRK-1001",
                Name = "Front Brake Pad Set",
                Category = "Braking",
                VendorName = "Himal Auto Traders",
                StorageLocation = "Rack A1",
                QuantityInStock = 8,
                ReorderLevel = 10,
                UnitCost = 2400m,
                UpdatedAt = now.AddHours(-2)
            },
            new InventoryItem
            {
                PartNumber = "FLT-2040",
                Name = "Engine Oil Filter",
                Category = "Engine",
                VendorName = "Valley Spare Hub",
                StorageLocation = "Rack B3",
                QuantityInStock = 27,
                ReorderLevel = 12,
                UnitCost = 650m,
                UpdatedAt = now.AddHours(-5)
            },
            new InventoryItem
            {
                PartNumber = "BAT-7780",
                Name = "12V Battery",
                Category = "Electrical",
                VendorName = "Everest Battery House",
                StorageLocation = "Floor C2",
                QuantityInStock = 5,
                ReorderLevel = 10,
                UnitCost = 9800m,
                UpdatedAt = now.AddDays(-1)
            },
            new InventoryItem
            {
                PartNumber = "TYR-3321",
                Name = "All-Season Tire 15in",
                Category = "Tyres",
                VendorName = "RoadGrip Suppliers",
                StorageLocation = "Rack D1",
                QuantityInStock = 18,
                ReorderLevel = 8,
                UnitCost = 7200m,
                UpdatedAt = now.AddHours(-9)
            },
            new InventoryItem
            {
                PartNumber = "CLT-4100",
                Name = "Clutch Release Bearing",
                Category = "Transmission",
                VendorName = "Himal Auto Traders",
                StorageLocation = "Rack A4",
                QuantityInStock = 11,
                ReorderLevel = 6,
                UnitCost = 3100m,
                UpdatedAt = now.AddDays(-2)
            }
        };

        dbContext.InventoryItems.AddRange(items);
        await dbContext.SaveChangesAsync(cancellationToken);

        var stockChanges = new[]
        {
            CreateChange(items[0], "Sale", -4, "INV-2045", "Counter Staff", "Issued for routine brake service.", now.AddHours(-2)),
            CreateChange(items[0], "Purchase", +12, "PO-1008", "Admin", "Purchase invoice recorded from Himal Auto Traders.", now.AddDays(-4)),
            CreateChange(items[1], "Sale", -6, "INV-2041", "Counter Staff", "Fast-moving service stock released.", now.AddHours(-5)),
            CreateChange(items[1], "Adjustment", +3, "ADJ-310", "Storekeeper", "Shelf count aligned with physical verification.", now.AddDays(-1)),
            CreateChange(items[2], "Sale", -2, "INV-2032", "Counter Staff", "Battery replacement sale completed.", now.AddDays(-1)),
            CreateChange(items[2], "Purchase", +7, "PO-998", "Admin", "Emergency restock received from vendor.", now.AddDays(-8)),
            CreateChange(items[3], "Sale", -4, "INV-2040", "Counter Staff", "Tire set sold for customer request.", now.AddHours(-9)),
            CreateChange(items[3], "Purchase", +10, "PO-1005", "Admin", "Bulk tire purchase added into inventory.", now.AddDays(-6)),
            CreateChange(items[4], "Sale", -1, "INV-2019", "Counter Staff", "Issued for transmission repair.", now.AddDays(-2)),
            CreateChange(items[4], "Purchase", +6, "PO-991", "Admin", "Replenished from regular supplier.", now.AddDays(-10))
        };

        dbContext.InventoryStockChanges.AddRange(stockChanges);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static InventoryStockChange CreateChange(
        InventoryItem item,
        string changeType,
        int quantityChanged,
        string referenceCode,
        string changedBy,
        string notes,
        DateTimeOffset changedAt)
    {
        return new InventoryStockChange
        {
            InventoryItemId = item.Id,
            ChangeType = changeType,
            QuantityChanged = quantityChanged,
            QuantityAfterChange = item.QuantityInStock,
            ReferenceCode = referenceCode,
            ChangedBy = changedBy,
            Notes = notes,
            ChangedAt = changedAt
        };
    }
}
