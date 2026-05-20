namespace Partivex.Domain.Entities;

public class InventoryStockChange
{
    public int Id { get; set; }

    public int? InventoryItemId { get; set; }

    public InventoryItem? InventoryItem { get; set; }

    public int? PartId { get; set; }

    public Part? Part { get; set; }

    public int? VendorId { get; set; }

    public Vendor? Vendor { get; set; }

    public string ChangeType { get; set; } = string.Empty;

    public int QuantityChanged { get; set; }

    public int QuantityAfterChange { get; set; }

    public string ReferenceCode { get; set; } = string.Empty;

    public string ChangedBy { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTimeOffset ChangedAt { get; set; }
}
