namespace Partivex.Application.DTOs;

public sealed record SaleItemDto(
    int Id,
    int InventoryItemId,
    string PartNumber,
    string PartName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record SalesInvoiceDto(
    int Id,
    int SaleId,
    string InvoiceNumber,
    DateOnly InvoiceDate,
    DateTimeOffset GeneratedAt,
    string CustomerName,
    string VehicleNo,
    string IssuedBy,
    decimal SubtotalAmount,
    int DiscountPercentage,
    decimal DiscountAmount,
    decimal TotalAmount);

public sealed record SaleDto(
    int Id,
    string CustomerName,
    string VehicleNo,
    string SoldBy,
    DateOnly SaleDate,
    string Notes,
    DateTimeOffset CreatedAt,
    decimal SubtotalAmount,
    int DiscountPercentage,
    decimal DiscountAmount,
    decimal TotalAmount,
    bool LoyaltyDiscountApplied,
    SalesInvoiceDto Invoice,
    IReadOnlyList<SaleItemDto> Items);

public sealed record CreateSaleCommand(
    string CustomerName,
    string VehicleNo,
    string SoldBy,
    DateOnly SaleDate,
    string Notes,
    IReadOnlyList<CreateSaleLineCommand> Lines);

public sealed record CreateSaleLineCommand(
    int InventoryItemId,
    int Quantity,
    decimal UnitPrice);

public sealed record DailySalesReportDto(
    DateOnly ReportDate,
    int TotalSalesCount,
    int TotalItemsSold,
    decimal GrossRevenue,
    decimal TotalDiscount,
    decimal NetRevenue,
    DateTimeOffset UpdatedAt);

public sealed record SalesReportSummaryDto(
    int TotalSalesCount,
    int TotalItemsSold,
    decimal GrossRevenue,
    decimal TotalDiscount,
    decimal NetRevenue,
    IReadOnlyList<DailySalesReportDto> DailyReports);

public sealed record SalesError(string Code, string Description);

public sealed class SalesResult<T>
{
    private SalesResult(T? value, IReadOnlyCollection<SalesError> errors, bool isNotFound)
    {
        Value = value;
        Errors = errors;
        IsNotFound = isNotFound;
    }

    public T? Value { get; }

    public IReadOnlyCollection<SalesError> Errors { get; }

    public bool IsNotFound { get; }

    public bool Succeeded => Value is not null && Errors.Count == 0 && !IsNotFound;

    public static SalesResult<T> Success(T value) => new(value, [], false);

    public static SalesResult<T> Failed(IReadOnlyCollection<SalesError> errors) => new(default, errors, false);

    public static SalesResult<T> NotFound() => new(default, [], true);
}
