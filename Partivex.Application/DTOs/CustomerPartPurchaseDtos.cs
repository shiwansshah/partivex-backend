using System.ComponentModel.DataAnnotations;

namespace Partivex.Application.DTOs;

public sealed record CustomerPartCatalogDto(
    int Id,
    string Name,
    string PartCode,
    string Category,
    string CompatibleVehicle,
    decimal UnitPrice,
    int CurrentStock,
    int MinimumStockLevel,
    string StockStatus,
    string ImageUrl);

public sealed record CustomerPartInvoiceItemDto(
    int Id,
    int PartId,
    string PartCode,
    string PartName,
    int Quantity,
    decimal UnitPrice,
    decimal SubTotal);

public sealed record CustomerPartInvoiceDto(
    int Id,
    string InvoiceNumber,
    string CustomerId,
    string CustomerName,
    string CustomerEmail,
    DateTimeOffset InvoiceDate,
    string Source,
    string Status,
    decimal SubTotal,
    decimal DiscountAmount,
    decimal TotalAmount,
    string CreatedBy,
    Guid? PartRequestId,
    DateTimeOffset CreatedAt,
    IReadOnlyList<CustomerPartInvoiceItemDto> Items);

public sealed class CustomerPartCheckoutRequest
{
    [Required]
    [MinLength(1)]
    public List<CustomerPartCheckoutLineRequest> Items { get; init; } = [];
}

public sealed class CustomerPartCheckoutLineRequest
{
    [Range(1, int.MaxValue)]
    public int PartId { get; init; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }
}

public sealed class ApprovePartRequestDto
{
    [EmailAddress]
    [MaxLength(160)]
    public string? Email { get; init; }

    [Range(1, int.MaxValue)]
    public int? PartId { get; init; }
}

public sealed class SendCustomerPartInvoiceEmailDto
{
    [Required]
    [EmailAddress]
    [MaxLength(160)]
    public string Email { get; init; } = string.Empty;
}

public sealed record CustomerPartInvoiceEmailResult(string Message, bool EmailSent);
