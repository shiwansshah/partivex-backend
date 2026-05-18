using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;

namespace Partivex.Application.Services;

public sealed class PurchaseService : IPurchaseService
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly IPartRepository _partRepository;
    private readonly IVendorRepository _vendorRepository;

    public PurchaseService(
        IPurchaseRepository purchaseRepository,
        IPartRepository partRepository,
        IVendorRepository vendorRepository)
    {
        _purchaseRepository = purchaseRepository;
        _partRepository = partRepository;
        _vendorRepository = vendorRepository;
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

        var vendor = await _vendorRepository.GetActiveByIdAsync(command.VendorId, cancellationToken);
        if (vendor is null)
        {
            return PurchaseResult<PurchaseInvoiceDto>.NotFound();
        }

        var invoice = new PurchaseInvoice
        {
            InvoiceNumber = command.InvoiceNumber.Trim().ToUpperInvariant(),
            VendorId = vendor.Id,
            VendorName = vendor.Name,
            InvoiceDate = command.InvoiceDate,
            Status = "Confirmed",
            CreatedBy = command.CreatedBy.Trim(),
            Notes = command.Notes.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        foreach (var line in command.Lines)
        {
            var part = await _partRepository.GetActiveByIdAsync(line.PartId, cancellationToken);
            if (part is null)
            {
                continue;
            }

            invoice.Items.Add(new PurchaseInvoiceItem
            {
                PartId = line.PartId,
                Quantity = line.Quantity,
                UnitPrice = part.UnitPrice
            });
        }

        await _purchaseRepository.AddAsync(invoice, cancellationToken);
        await _purchaseRepository.SaveChangesAsync(cancellationToken);

        var saved = await _purchaseRepository.GetByIdAsync(invoice.Id, cancellationToken);
        return PurchaseResult<PurchaseInvoiceDto>.Success(MapInvoice(saved!));
    }

    public Task<PurchaseResult<PurchaseInvoiceDto>> ConfirmAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(PurchaseResult<PurchaseInvoiceDto>.Failed(
        [
            new PurchaseError("Status", "Purchase invoices are created automatically when stock is added.")
        ]));
    }

    public async Task<PurchaseResult<int>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoice = await _purchaseRepository.GetByIdAsync(id, cancellationToken);
        if (invoice is null)
        {
            return PurchaseResult<int>.NotFound();
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

        if (command.VendorId <= 0)
        {
            errors.Add(new PurchaseError(nameof(command.VendorId), "Vendor is required."));
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
}
