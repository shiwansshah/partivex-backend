namespace Partivex.Domain.Entities;

public class CustomerPartPurchaseInvoiceItem
{
    public int Id { get; set; }

    public int CustomerPartPurchaseInvoiceId { get; set; }

    public CustomerPartPurchaseInvoice CustomerPartPurchaseInvoice { get; set; } = null!;

    public int PartId { get; set; }

    public Part Part { get; set; } = null!;

    public string PartCode { get; set; } = string.Empty;

    public string PartName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal SubTotal { get; set; }
}
