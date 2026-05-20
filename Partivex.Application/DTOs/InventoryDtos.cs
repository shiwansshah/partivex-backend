namespace Partivex.Application.DTOs;

public sealed record InventorySummaryDto(
    int TotalParts,
    int TotalUnits,
    int LowStockItems,
    DateTimeOffset? LastUpdatedAt);

public sealed record InventoryItemDto(
    int Id,
    string PartNumber,
    string Name,
    string Category,
    string CompatibleVehicle,
    int QuantityInStock,
    int ReorderLevel,
    decimal UnitPrice,
    string ImageUrl,
    DateTimeOffset UpdatedAt,
    string StockStatus);

public sealed record InventoryStockChangeDto(
    int Id,
    int PartId,
    string PartName,
    string PartNumber,
    string VendorName,
    string ChangeType,
    int QuantityChanged,
    int QuantityAfterChange,
    string ReferenceCode,
    string ChangedBy,
    string Notes,
    DateTimeOffset ChangedAt);

public sealed record InventoryMonitoringDto(
    InventorySummaryDto Summary,
    IReadOnlyCollection<InventoryItemDto> Items,
    IReadOnlyCollection<InventoryStockChangeDto> RecentChanges);

public sealed record AddStockLineCommand(
    int PartId,
    int PurchaseQuantity);

public sealed record AddStockCommand(
    int VendorId,
    DateTimeOffset PurchaseDate,
    string InvoiceNumber,
    string ChangedBy,
    string Remarks,
    IReadOnlyList<AddStockLineCommand> Lines);

public sealed record InventoryError(string Code, string Description);

public sealed class InventoryResult<T>
{
    private InventoryResult(T? value, IReadOnlyCollection<InventoryError> errors, bool isNotFound)
    {
        Value = value;
        Errors = errors;
        IsNotFound = isNotFound;
    }

    public T? Value { get; }

    public IReadOnlyCollection<InventoryError> Errors { get; }

    public bool IsNotFound { get; }

    public bool Succeeded => Value is not null && Errors.Count == 0 && !IsNotFound;

    public static InventoryResult<T> Success(T value) => new(value, [], false);

    public static InventoryResult<T> Failed(IReadOnlyCollection<InventoryError> errors) => new(default, errors, false);

    public static InventoryResult<T> NotFound() => new(default, [], true);
}
