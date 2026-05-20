namespace Partivex.Domain.Entities;

public class SalesReport
{
    public int Id { get; set; }

    public DateOnly ReportDate { get; set; }

    public int TotalSalesCount { get; set; }

    public int TotalItemsSold { get; set; }

    public decimal GrossRevenue { get; set; }

    public decimal TotalDiscount { get; set; }

    public decimal NetRevenue { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
