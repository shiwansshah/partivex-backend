namespace Partivex.Domain.Entities;

public class PurchaseInvoice
{
    public int Id { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public string VendorName { get; set; } = string.Empty;

    public DateTimeOffset InvoiceDate { get; set; }

    public string Status { get; set; } = "Draft";

    public string CreatedBy { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<PurchaseInvoiceItem> Items { get; set; } = [];
}
