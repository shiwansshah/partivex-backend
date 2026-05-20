namespace Partivex.Domain.Entities;

public class SalesInvoice
{
    public int Id { get; set; }

    public int SaleId { get; set; }

    public Sale Sale { get; set; } = null!;

    public string InvoiceNumber { get; set; } = string.Empty;

    public DateOnly InvoiceDate { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string VehicleNo { get; set; } = string.Empty;

    public string IssuedBy { get; set; } = string.Empty;

    public decimal SubtotalAmount { get; set; }

    public int DiscountPercentage { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }
}
