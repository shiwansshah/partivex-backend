namespace Partivex.Domain.Entities;

public class SaleItem
{
    public int Id { get; set; }

    public int SaleId { get; set; }

    public Sale Sale { get; set; } = null!;

    public int InventoryItemId { get; set; }

    public InventoryItem InventoryItem { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }
}
