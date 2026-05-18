namespace Partivex.Domain.Entities;

public class Part
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string PartCode { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string CompatibleVehicle { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int MinimumStockLevel { get; set; }

    public int CurrentStock { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<InventoryStockChange> StockChanges { get; set; } = [];
}
