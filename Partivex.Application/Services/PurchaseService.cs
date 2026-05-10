using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;

namespace Partivex.Application.Services;

public sealed class PurchaseService : IPurchaseService
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly IInventoryRepository _inventoryRepository;

    public PurchaseService(IPurchaseRepository purchaseRepository, IInventoryRepository inventoryRepository)
    {
        _purchaseRepository = purchaseRepository;
        _inventoryRepository = inventoryRepository;
    }

    public async Task<IReadOnlyCollection<PurchaseInvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await _purchaseRepository.GetAllAsync(cancellationToken);
        return invoices.Select(MapInvoice).ToArray();
    }

    public async Task<PurchaseResult<PurchaseInvoiceDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoice = await _purchaseRepository.GetByIdAsync(id, cancellationToken);
        return invoice is null
            ? PurchaseResult<PurchaseInvoiceDto>.NotFound()
            : PurchaseResult<PurchaseInvoiceDto>.Success(MapInvoice(invoice));
    }

    public async Task<PurchaseResult<PurchaseInvoiceDto>> CreateAsync(
        CreatePurchaseInvoiceCommand command,
        CancellationToken cancellationToken = default)
    {
        var errors = await ValidateCommandAsync(command, cancellationToken);
        if (errors.Count > 0)
        {
            return PurchaseResult<PurchaseInvoiceDto>.Failed(errors);
        }

        var invoice = new PurchaseInvoice
        {
            InvoiceNumber = command.InvoiceNumber.Trim().ToUpperInvariant(),
            VendorName = command.VendorName.Trim(),
            InvoiceDate = command.InvoiceDate,
            Status = "Draft",
            CreatedBy = command.CreatedBy.Trim(),
            Notes = command.Notes.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        foreach (var line in command.Lines)
        {
            var inventoryItem = await _inventoryRepository.GetItemByIdAsync(line.InventoryItemId, cancellationToken);
            if (inventoryItem is null)
            {
                continue;
            }

            invoice.Items.Add(new PurchaseInvoiceItem
            {
                InventoryItemId = line.InventoryItemId,
                Quantity = line.Quantity,
                UnitCost = line.UnitCost
            });
        }

        await _purchaseRepository.AddAsync(invoice, cancellationToken);
        await _purchaseRepository.SaveChangesAsync(cancellationToken);

        var saved = await _purchaseRepository.GetByIdAsync(invoice.Id, cancellationToken);
        return PurchaseResult<PurchaseInvoiceDto>.Success(MapInvoice(saved!));
    }

    public async Task<PurchaseResult<PurchaseInvoiceDto>> ConfirmAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoice = await _purchaseRepository.GetByIdAsync(id, cancellationToken);
        if (invoice is null)
        {
            return PurchaseResult<PurchaseInvoiceDto>.NotFound();
        }

        if (invoice.Status == "Confirmed")
        {
            return PurchaseResult<PurchaseInvoiceDto>.Failed(
            [
                new PurchaseError("Status", "This invoice has already been confirmed.")
            ]);
        }

        invoice.Status = "Confirmed";

        foreach (var line in invoice.Items)
        {
            var inventoryItem = await _inventoryRepository.GetItemByIdAsync(line.InventoryItemId, cancellationToken);
            if (inventoryItem is null)
            {
                continue;
            }

            inventoryItem.QuantityInStock += line.Quantity;
            inventoryItem.UpdatedAt = DateTimeOffset.UtcNow;

            var stockChange = new InventoryStockChange
            {
                InventoryItemId = inventoryItem.Id,
                ChangeType = "Purchase",
                QuantityChanged = line.Quantity,
                QuantityAfterChange = inventoryItem.QuantityInStock,
                ReferenceCode = invoice.InvoiceNumber,
                ChangedBy = invoice.CreatedBy,
                Notes = $"Stock received from purchase invoice {invoice.InvoiceNumber} — {invoice.VendorName}.",
                ChangedAt = DateTimeOffset.UtcNow
            };

            await _inventoryRepository.AddStockChangeAsync(stockChange, cancellationToken);
        }

        await _purchaseRepository.SaveChangesAsync(cancellationToken);
        await _inventoryRepository.SaveChangesAsync(cancellationToken);

        return PurchaseResult<PurchaseInvoiceDto>.Success(MapInvoice(invoice));
    }

    public async Task<PurchaseResult<int>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoice = await _purchaseRepository.GetByIdAsync(id, cancellationToken);
        if (invoice is null)
        {
            return PurchaseResult<int>.NotFound();
        }

        if (invoice.Status == "Confirmed")
        {
            return PurchaseResult<int>.Failed(
            [
                new PurchaseError("Status", "Confirmed invoices cannot be deleted.")
            ]);
        }

        _purchaseRepository.Remove(invoice);
        await _purchaseRepository.SaveChangesAsync(cancellationToken);

        return PurchaseResult<int>.Success(id);
    }

    private async Task<IReadOnlyCollection<PurchaseError>> ValidateCommandAsync(
        CreatePurchaseInvoiceCommand command,
        CancellationToken cancellationToken)
    {
        var errors = new List<PurchaseError>();

        if (string.IsNullOrWhiteSpace(command.InvoiceNumber))
        {
            errors.Add(new PurchaseError(nameof(command.InvoiceNumber), "Invoice number is required."));
        }

        if (string.IsNullOrWhiteSpace(command.VendorName))
        {
            errors.Add(new PurchaseError(nameof(command.VendorName), "Vendor name is required."));
        }

        if (string.IsNullOrWhiteSpace(command.CreatedBy))
        {
            errors.Add(new PurchaseError(nameof(command.CreatedBy), "Created by is required."));
        }

        if (command.Lines is null || command.Lines.Count == 0)
        {
            errors.Add(new PurchaseError("Lines", "At least one line item is required."));
        }
        else if (command.Lines.Any(l => l.Quantity <= 0))
        {
            errors.Add(new PurchaseError("Lines", "All line items must have a quantity greater than zero."));
        }
        else if (command.Lines.Any(l => l.UnitCost < 0))
        {
            errors.Add(new PurchaseError("Lines", "Unit cost cannot be negative."));
        }

        if (!string.IsNullOrWhiteSpace(command.InvoiceNumber) &&
            await _purchaseRepository.InvoiceNumberExistsAsync(
                command.InvoiceNumber.Trim().ToUpperInvariant(), cancellationToken))
        {
            errors.Add(new PurchaseError(nameof(command.InvoiceNumber), "This invoice number already exists."));
        }

        return errors;
    }

    private static PurchaseInvoiceDto MapInvoice(PurchaseInvoice invoice)
    {
        var itemDtos = invoice.Items
            .Select(item => new PurchaseInvoiceItemDto(
                item.Id,
                item.InventoryItemId,
                item.InventoryItem?.PartNumber ?? string.Empty,
                item.InventoryItem?.Name ?? string.Empty,
                item.Quantity,
                item.UnitCost,
                item.Quantity * item.UnitCost))
            .ToArray();

        return new PurchaseInvoiceDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.VendorName,
            invoice.InvoiceDate,
            invoice.Status,
            invoice.CreatedBy,
            invoice.Notes,
            invoice.CreatedAt,
            itemDtos.Sum(i => i.SubTotal),
            itemDtos);
    }
}
