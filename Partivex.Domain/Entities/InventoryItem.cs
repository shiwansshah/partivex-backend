namespace Partivex.Domain.Entities;

public class InventoryItem
{
    public int Id { get; set; }

    public string PartNumber { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string VendorName { get; set; } = string.Empty;

    public string StorageLocation { get; set; } = string.Empty;

    public int QuantityInStock { get; set; }

    public int ReorderLevel { get; set; } = 10;

    public decimal UnitCost { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<InventoryStockChange> StockChanges { get; set; } = [];
}
