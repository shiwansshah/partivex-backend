namespace Partivex.Domain.Entities;

public class CustomerPartPurchaseInvoice
{
    public int Id { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public string CustomerId { get; set; } = string.Empty;

    public ApplicationUser Customer { get; set; } = null!;

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public DateTimeOffset InvoiceDate { get; set; }

    public string Source { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending";

    public decimal SubTotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public Guid? PartRequestId { get; set; }

    public PartRequest? PartRequest { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<CustomerPartPurchaseInvoiceItem> Items { get; set; } = [];
}
