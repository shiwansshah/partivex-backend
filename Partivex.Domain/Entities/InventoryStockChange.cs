namespace Partivex.Domain.Entities;

public class InventoryStockChange
{
    public int Id { get; set; }

    public int InventoryItemId { get; set; }

    public InventoryItem InventoryItem { get; set; } = null!;

    public string ChangeType { get; set; } = string.Empty;

    public int QuantityChanged { get; set; }

    public int QuantityAfterChange { get; set; }

    public string ReferenceCode { get; set; } = string.Empty;

    public string ChangedBy { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTimeOffset ChangedAt { get; set; }
}
