using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;

namespace Partivex.Application.Services;

public sealed class InventoryService : IInventoryService
{
    private const int RecentChangesLimit = 12;
    private const string DefaultChangedBy = "Admin";

    private readonly IInventoryRepository _inventoryRepository;

    public InventoryService(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<InventoryMonitoringDto> GetMonitoringAsync(CancellationToken cancellationToken = default)
    {
        var items = await _inventoryRepository.GetItemsAsync(cancellationToken);
        var changes = await _inventoryRepository.GetRecentStockChangesAsync(RecentChangesLimit, cancellationToken);

        var itemDtos = items.Select(MapItem).ToArray();
        var changeDtos = changes.Select(MapChange).ToArray();

        var summary = new InventorySummaryDto(
            itemDtos.Length,
            itemDtos.Sum(item => item.QuantityInStock),
            itemDtos.Count(item => item.IsLowStock),
            itemDtos
                .Select(item => (DateTimeOffset?)item.UpdatedAt)
                .OrderByDescending(updatedAt => updatedAt)
                .FirstOrDefault());

        return new InventoryMonitoringDto(summary, itemDtos, changeDtos);
    }

    public async Task<IReadOnlyCollection<InventoryItemDto>> GetItemsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _inventoryRepository.GetItemsAsync(cancellationToken);
        return items.Select(MapItem).ToArray();
    }

    public async Task<InventoryResult<InventoryItemDto>> CreateItemAsync(
        UpsertInventoryItemCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = await ValidateCommandAsync(command, null, cancellationToken);
        if (validationErrors.Count > 0)
        {
            return InventoryResult<InventoryItemDto>.Failed(validationErrors);
        }

        var sanitized = SanitizeCommand(command);
        var item = new InventoryItem
        {
            PartNumber = sanitized.PartNumber,
            Name = sanitized.Name,
            Category = sanitized.Category,
            VendorName = sanitized.VendorName,
            StorageLocation = sanitized.StorageLocation,
            QuantityInStock = sanitized.QuantityInStock,
            ReorderLevel = sanitized.ReorderLevel,
            UnitCost = sanitized.UnitCost,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _inventoryRepository.AddItemAsync(item, cancellationToken);
        await _inventoryRepository.SaveChangesAsync(cancellationToken);

        if (item.QuantityInStock > 0)
        {
            var initialChange = BuildStockChange(
                item,
                item.QuantityInStock,
                sanitized,
                "Initial Stock",
                $"OPEN-{item.PartNumber}");

            await _inventoryRepository.AddStockChangeAsync(initialChange, cancellationToken);
            await _inventoryRepository.SaveChangesAsync(cancellationToken);
        }

        return InventoryResult<InventoryItemDto>.Success(MapItem(item));
    }

    public async Task<InventoryResult<InventoryItemDto>> UpdateItemAsync(
        int id,
        UpsertInventoryItemCommand command,
        CancellationToken cancellationToken = default)
    {
        var item = await _inventoryRepository.GetItemByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return InventoryResult<InventoryItemDto>.NotFound();
        }

        var validationErrors = await ValidateCommandAsync(command, id, cancellationToken);
        if (validationErrors.Count > 0)
        {
            return InventoryResult<InventoryItemDto>.Failed(validationErrors);
        }

        var sanitized = SanitizeCommand(command);
        var quantityDifference = sanitized.QuantityInStock - item.QuantityInStock;

        item.PartNumber = sanitized.PartNumber;
        item.Name = sanitized.Name;
        item.Category = sanitized.Category;
        item.VendorName = sanitized.VendorName;
        item.StorageLocation = sanitized.StorageLocation;
        item.QuantityInStock = sanitized.QuantityInStock;
        item.ReorderLevel = sanitized.ReorderLevel;
        item.UnitCost = sanitized.UnitCost;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        await _inventoryRepository.SaveChangesAsync(cancellationToken);

        if (quantityDifference != 0)
        {
            var stockChange = BuildStockChange(
                item,
                quantityDifference,
                sanitized,
                quantityDifference > 0 ? "Adjustment In" : "Adjustment Out",
                $"ADJ-{item.Id:0000}");

            await _inventoryRepository.AddStockChangeAsync(stockChange, cancellationToken);
            await _inventoryRepository.SaveChangesAsync(cancellationToken);
        }

        return InventoryResult<InventoryItemDto>.Success(MapItem(item));
    }

    public async Task<InventoryResult<InventoryDeletedResponse>> DeleteItemAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var item = await _inventoryRepository.GetItemByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return InventoryResult<InventoryDeletedResponse>.NotFound();
        }

        var response = new InventoryDeletedResponse(item.Id, item.Name);
        _inventoryRepository.RemoveItem(item);
        await _inventoryRepository.SaveChangesAsync(cancellationToken);

        return InventoryResult<InventoryDeletedResponse>.Success(response);
    }

    public async Task<IReadOnlyCollection<InventoryStockChangeDto>> GetRecentStockChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var changes = await _inventoryRepository.GetRecentStockChangesAsync(RecentChangesLimit, cancellationToken);
        return changes.Select(MapChange).ToArray();
    }

    private static InventoryItemDto MapItem(InventoryItem item)
    {
        return new InventoryItemDto(
            item.Id,
            item.PartNumber,
            item.Name,
            item.Category,
            item.VendorName,
            item.StorageLocation,
            item.QuantityInStock,
            item.ReorderLevel,
            item.UnitCost,
            item.UpdatedAt,
            item.QuantityInStock < item.ReorderLevel);
    }

    private static InventoryStockChangeDto MapChange(InventoryStockChange change)
    {
        return new InventoryStockChangeDto(
            change.Id,
            change.InventoryItemId,
            change.InventoryItem.Name,
            change.InventoryItem.PartNumber,
            change.ChangeType,
            change.QuantityChanged,
            change.QuantityAfterChange,
            change.ReferenceCode,
            change.ChangedBy,
            change.Notes,
            change.ChangedAt);
    }

    private async Task<IReadOnlyCollection<InventoryError>> ValidateCommandAsync(
        UpsertInventoryItemCommand command,
        int? existingId,
        CancellationToken cancellationToken)
    {
        var errors = new List<InventoryError>();
        var sanitized = SanitizeCommand(command);

        if (string.IsNullOrWhiteSpace(sanitized.PartNumber))
        {
            errors.Add(new InventoryError(nameof(command.PartNumber), "Part number is required."));
        }

        if (string.IsNullOrWhiteSpace(sanitized.Name))
        {
            errors.Add(new InventoryError(nameof(command.Name), "Part name is required."));
        }

        if (string.IsNullOrWhiteSpace(sanitized.Category))
        {
            errors.Add(new InventoryError(nameof(command.Category), "Category is required."));
        }

        if (string.IsNullOrWhiteSpace(sanitized.VendorName))
        {
            errors.Add(new InventoryError(nameof(command.VendorName), "Vendor name is required."));
        }

        if (string.IsNullOrWhiteSpace(sanitized.StorageLocation))
        {
            errors.Add(new InventoryError(nameof(command.StorageLocation), "Storage location is required."));
        }

        if (string.IsNullOrWhiteSpace(sanitized.ChangedBy))
        {
            errors.Add(new InventoryError(nameof(command.ChangedBy), "Handled by is required."));
        }

        if (sanitized.QuantityInStock < 0)
        {
            errors.Add(new InventoryError(nameof(command.QuantityInStock), "Stock quantity cannot be negative."));
        }

        if (sanitized.ReorderLevel < 0)
        {
            errors.Add(new InventoryError(nameof(command.ReorderLevel), "Reorder level cannot be negative."));
        }

        if (sanitized.UnitCost < 0)
        {
            errors.Add(new InventoryError(nameof(command.UnitCost), "Unit cost cannot be negative."));
        }

        if (!string.IsNullOrWhiteSpace(sanitized.PartNumber) &&
            await _inventoryRepository.PartNumberExistsAsync(
                sanitized.PartNumber,
                existingId,
                cancellationToken))
        {
            errors.Add(new InventoryError(nameof(command.PartNumber), "This part number already exists."));
        }

        return errors;
    }

    private static UpsertInventoryItemCommand SanitizeCommand(UpsertInventoryItemCommand command)
    {
        return command with
        {
            PartNumber = command.PartNumber.Trim().ToUpperInvariant(),
            Name = command.Name.Trim(),
            Category = command.Category.Trim(),
            VendorName = command.VendorName.Trim(),
            StorageLocation = command.StorageLocation.Trim(),
            ChangedBy = string.IsNullOrWhiteSpace(command.ChangedBy) ? DefaultChangedBy : command.ChangedBy.Trim(),
            ReferenceCode = command.ReferenceCode.Trim(),
            Notes = command.Notes.Trim(),
            StockChangeType = command.StockChangeType.Trim()
        };
    }

    private static InventoryStockChange BuildStockChange(
        InventoryItem item,
        int quantityChanged,
        UpsertInventoryItemCommand command,
        string fallbackType,
        string fallbackReferenceCode)
    {
        var changeType = string.IsNullOrWhiteSpace(command.StockChangeType)
            ? fallbackType
            : command.StockChangeType;

        var referenceCode = string.IsNullOrWhiteSpace(command.ReferenceCode)
            ? fallbackReferenceCode
            : command.ReferenceCode;

        var notes = string.IsNullOrWhiteSpace(command.Notes)
            ? "Inventory record updated from the administration panel."
            : command.Notes;

        return new InventoryStockChange
        {
            InventoryItemId = item.Id,
            ChangeType = changeType,
            QuantityChanged = quantityChanged,
            QuantityAfterChange = item.QuantityInStock,
            ReferenceCode = referenceCode,
            ChangedBy = string.IsNullOrWhiteSpace(command.ChangedBy) ? DefaultChangedBy : command.ChangedBy,
            Notes = notes,
            ChangedAt = DateTimeOffset.UtcNow
        };
    }
}
