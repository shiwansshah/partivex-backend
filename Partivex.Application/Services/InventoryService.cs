using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;

namespace Partivex.Application.Services;

public sealed class InventoryService : IInventoryService
{
    private const int RecentChangesLimit = 20;
    private const string DefaultChangedBy = "Partivex Admin";

    private readonly IInventoryRepository _inventoryRepository;
    private readonly IPartRepository _partRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IPurchaseRepository _purchaseRepository;

    public InventoryService(
        IInventoryRepository inventoryRepository,
        IPartRepository partRepository,
        IVendorRepository vendorRepository,
        IPurchaseRepository purchaseRepository)
    {
        _inventoryRepository = inventoryRepository;
        _partRepository = partRepository;
        _vendorRepository = vendorRepository;
        _purchaseRepository = purchaseRepository;
    }

    public async Task<InventoryMonitoringDto> GetMonitoringAsync(CancellationToken cancellationToken = default)
    {
        var parts = await _inventoryRepository.GetPartsAsync(cancellationToken);
        var changes = await _inventoryRepository.GetRecentStockChangesAsync(RecentChangesLimit, cancellationToken);

        var itemDtos = parts.Select(MapPart).ToArray();
        var changeDtos = changes.Select(MapChange).ToArray();

        var summary = new InventorySummaryDto(
            itemDtos.Length,
            itemDtos.Sum(item => item.QuantityInStock),
            itemDtos.Count(item => item.StockStatus is "Low Stock" or "Out of Stock"),
            itemDtos
                .Select(item => (DateTimeOffset?)item.UpdatedAt)
                .OrderByDescending(updatedAt => updatedAt)
                .FirstOrDefault());

        return new InventoryMonitoringDto(summary, itemDtos, changeDtos);
    }

    public async Task<IReadOnlyCollection<InventoryItemDto>> GetItemsAsync(CancellationToken cancellationToken = default)
    {
        var parts = await _inventoryRepository.GetPartsAsync(cancellationToken);
        return parts.Select(MapPart).ToArray();
    }

    public async Task<InventoryResult<PurchaseInvoiceDto>> AddStockAsync(
        AddStockCommand command,
        CancellationToken cancellationToken = default)
    {
        var errors = await ValidateAddStockCommandAsync(command, cancellationToken);
        if (errors.Count > 0)
        {
            return InventoryResult<PurchaseInvoiceDto>.Failed(errors);
        }

        var vendor = await _vendorRepository.GetActiveByIdAsync(command.VendorId, cancellationToken);
        if (vendor is null)
        {
            return InventoryResult<PurchaseInvoiceDto>.NotFound();
        }

        var partsById = new Dictionary<int, Part>();
        foreach (var line in command.Lines)
        {
            var part = await _partRepository.GetActiveByIdAsync(line.PartId, cancellationToken);
            if (part is null)
            {
                return InventoryResult<PurchaseInvoiceDto>.NotFound();
            }

            partsById[line.PartId] = part;
        }

        var invoiceNumber = string.IsNullOrWhiteSpace(command.InvoiceNumber)
            ? await GenerateInvoiceNumberAsync(cancellationToken)
            : command.InvoiceNumber.Trim().ToUpperInvariant();
        var changedBy = string.IsNullOrWhiteSpace(command.ChangedBy)
            ? DefaultChangedBy
            : command.ChangedBy.Trim();
        var remarks = command.Remarks.Trim();

        var invoice = new PurchaseInvoice
        {
            InvoiceNumber = invoiceNumber,
            VendorId = vendor.Id,
            VendorName = vendor.Name,
            InvoiceDate = command.PurchaseDate,
            Status = "Confirmed",
            CreatedBy = changedBy,
            Notes = remarks,
            CreatedAt = DateTimeOffset.UtcNow
        };

        foreach (var line in command.Lines)
        {
            var part = partsById[line.PartId];
            part.CurrentStock += line.PurchaseQuantity;
            part.UpdatedAt = DateTime.UtcNow;

            var inventoryItem = await _inventoryRepository.GetByPartNumberAsync(part.PartCode, cancellationToken);
            if (inventoryItem is null)
            {
                inventoryItem = new InventoryItem
                {
                    Id = part.Id,
                    PartNumber = part.PartCode,
                    Name = part.Name,
                    Category = part.Category,
                    VendorName = vendor.Name,
                    StorageLocation = "Main Store",
                    QuantityInStock = part.CurrentStock,
                    ReorderLevel = part.MinimumStockLevel,
                    UnitCost = part.UnitPrice,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                await _inventoryRepository.AddInventoryItemAsync(inventoryItem, cancellationToken);
            }
            else
            {
                inventoryItem.Name = part.Name;
                inventoryItem.Category = part.Category;
                inventoryItem.VendorName = vendor.Name;
                inventoryItem.QuantityInStock = part.CurrentStock;
                inventoryItem.ReorderLevel = part.MinimumStockLevel;
                inventoryItem.UnitCost = part.UnitPrice;
                inventoryItem.UpdatedAt = DateTimeOffset.UtcNow;
            }

            invoice.Items.Add(new PurchaseInvoiceItem
            {
                PartId = part.Id,
                Quantity = line.PurchaseQuantity,
                UnitPrice = part.UnitPrice
            });

            await _inventoryRepository.AddStockChangeAsync(new InventoryStockChange
            {
                PartId = part.Id,
                VendorId = vendor.Id,
                ChangeType = "Purchase",
                QuantityChanged = line.PurchaseQuantity,
                QuantityAfterChange = part.CurrentStock,
                ReferenceCode = invoiceNumber,
                ChangedBy = changedBy,
                Notes = remarks,
                ChangedAt = DateTimeOffset.UtcNow
            }, cancellationToken);
        }

        await _purchaseRepository.AddAsync(invoice, cancellationToken);
        await _purchaseRepository.SaveChangesAsync(cancellationToken);

        var saved = await _purchaseRepository.GetByIdAsync(invoice.Id, cancellationToken);
        return InventoryResult<PurchaseInvoiceDto>.Success(MapInvoice(saved!));
    }

    public async Task<IReadOnlyCollection<InventoryStockChangeDto>> GetRecentStockChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var changes = await _inventoryRepository.GetRecentStockChangesAsync(RecentChangesLimit, cancellationToken);
        return changes.Select(MapChange).ToArray();
    }

    private async Task<IReadOnlyCollection<InventoryError>> ValidateAddStockCommandAsync(
        AddStockCommand command,
        CancellationToken cancellationToken)
    {
        var errors = new List<InventoryError>();

        if (command.VendorId <= 0)
        {
            errors.Add(new InventoryError(nameof(command.VendorId), "Select an active vendor."));
        }

        if (command.Lines is null || command.Lines.Count == 0)
        {
            errors.Add(new InventoryError("Lines", "Add at least one stock line."));
        }
        else
        {
            if (command.Lines.Any(line => line.PartId <= 0))
            {
                errors.Add(new InventoryError("Lines", "Select an active part for every stock line."));
            }

            if (command.Lines.Any(line => line.PurchaseQuantity <= 0))
            {
                errors.Add(new InventoryError("Lines", "Purchase quantity must be greater than zero for every stock line."));
            }

            var duplicatePartIds = command.Lines
                .GroupBy(line => line.PartId)
                .Where(group => group.Key > 0 && group.Count() > 1)
                .Select(group => group.Key)
                .ToArray();

            if (duplicatePartIds.Length > 0)
            {
                errors.Add(new InventoryError("Lines", "Each part can only appear once per stock update."));
            }
        }

        if (!string.IsNullOrWhiteSpace(command.InvoiceNumber) &&
            await _purchaseRepository.InvoiceNumberExistsAsync(command.InvoiceNumber.Trim().ToUpperInvariant(), cancellationToken))
        {
            errors.Add(new InventoryError(nameof(command.InvoiceNumber), "This invoice number already exists."));
        }

        return errors;
    }

    private async Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken)
    {
        var counter = 1;
        while (true)
        {
            var candidate = $"PUR-{counter:0000}";
            if (!await _purchaseRepository.InvoiceNumberExistsAsync(candidate, cancellationToken))
            {
                return candidate;
            }

            counter++;
        }
    }

    private static InventoryItemDto MapPart(Part part)
    {
        return new InventoryItemDto(
            part.Id,
            part.PartCode,
            part.Name,
            part.Category,
            part.CompatibleVehicle,
            part.CurrentStock,
            part.MinimumStockLevel,
            part.UnitPrice,
            part.ImageUrl,
            part.UpdatedAt,
            GetStockStatus(part.CurrentStock, part.MinimumStockLevel));
    }

    private static InventoryStockChangeDto MapChange(InventoryStockChange change)
    {
        return new InventoryStockChangeDto(
            change.Id,
            change.PartId ?? 0,
            change.Part?.Name ?? string.Empty,
            change.Part?.PartCode ?? string.Empty,
            change.Vendor?.Name ?? string.Empty,
            change.ChangeType,
            change.QuantityChanged,
            change.QuantityAfterChange,
            change.ReferenceCode,
            change.ChangedBy,
            change.Notes,
            change.ChangedAt);
    }

    private static PurchaseInvoiceDto MapInvoice(PurchaseInvoice invoice)
    {
        var itemDtos = invoice.Items
            .Select(item => new PurchaseInvoiceItemDto(
                item.Id,
                item.PartId,
                item.Part?.PartCode ?? string.Empty,
                item.Part?.Name ?? string.Empty,
                item.Quantity,
                item.UnitPrice,
                item.Quantity * item.UnitPrice))
            .ToArray();

        return new PurchaseInvoiceDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.VendorId,
            invoice.VendorName,
            invoice.InvoiceDate,
            invoice.Status,
            invoice.CreatedBy,
            invoice.Notes,
            invoice.CreatedAt,
            itemDtos.Sum(i => i.SubTotal),
            itemDtos);
    }

    private static string GetStockStatus(int currentStock, int minimumStockLevel)
    {
        if (currentStock == 0) return "Out of Stock";
        return currentStock <= minimumStockLevel ? "Low Stock" : "In Stock";
    }
}
