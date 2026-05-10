namespace Partivex.Domain.Entities;

public class PurchaseInvoiceItem
{
    public int Id { get; set; }

    public int PurchaseInvoiceId { get; set; }

    public PurchaseInvoice PurchaseInvoice { get; set; } = null!;

    public int InventoryItemId { get; set; }

    public InventoryItem InventoryItem { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitCost { get; set; }
}
