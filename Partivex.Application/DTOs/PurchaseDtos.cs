namespace Partivex.Application.DTOs;

public sealed record PurchaseInvoiceItemDto(
    int Id,
    int InventoryItemId,
    string PartNumber,
    string PartName,
    int Quantity,
    decimal UnitCost,
    decimal SubTotal);

public sealed record PurchaseInvoiceDto(
    int Id,
    string InvoiceNumber,
    string VendorName,
    DateTimeOffset InvoiceDate,
    string Status,
    string CreatedBy,
    string Notes,
    DateTimeOffset CreatedAt,
    decimal TotalAmount,
    IReadOnlyList<PurchaseInvoiceItemDto> Items);

public sealed record CreatePurchaseInvoiceCommand(
    string InvoiceNumber,
    string VendorName,
    DateTimeOffset InvoiceDate,
    string CreatedBy,
    string Notes,
    IReadOnlyList<PurchaseInvoiceLineCommand> Lines);

public sealed record PurchaseInvoiceLineCommand(
    int InventoryItemId,
    int Quantity,
    decimal UnitCost);

public sealed record PurchaseError(string Code, string Description);

public sealed class PurchaseResult<T>
{
    private PurchaseResult(T? value, IReadOnlyCollection<PurchaseError> errors, bool isNotFound)
    {
        Value = value;
        Errors = errors;
        IsNotFound = isNotFound;
    }

    public T? Value { get; }

    public IReadOnlyCollection<PurchaseError> Errors { get; }

    public bool IsNotFound { get; }

    public bool Succeeded => Value is not null && Errors.Count == 0 && !IsNotFound;

    public static PurchaseResult<T> Success(T value) => new(value, [], false);

    public static PurchaseResult<T> Failed(IReadOnlyCollection<PurchaseError> errors) => new(default, errors, false);

    public static PurchaseResult<T> NotFound() => new(default, [], true);
}
