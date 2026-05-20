namespace Partivex.Domain.Entities;

public class Sale
{
    public int Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string VehicleNo { get; set; } = string.Empty;

    public string SoldBy { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateOnly SaleDate { get; set; }

    public decimal SubtotalAmount { get; set; }

    public int DiscountPercentage { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<SaleItem> Items { get; set; } = [];

    public SalesInvoice? Invoice { get; set; }
}
