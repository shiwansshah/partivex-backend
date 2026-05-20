using System.Security.Cryptography;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;

namespace Partivex.Application.Services;

public sealed class SalesService : ISalesService
{
    private const decimal LoyaltyThreshold = 5000m;
    private const int LoyaltyDiscountPercentage = 10;

    private readonly ISalesRepository _salesRepository;
    private readonly IInventoryRepository _inventoryRepository;

    public SalesService(ISalesRepository salesRepository, IInventoryRepository inventoryRepository)
    {
        _salesRepository = salesRepository;
        _inventoryRepository = inventoryRepository;
    }

    public async Task<IReadOnlyCollection<SaleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sales = await _salesRepository.GetAllAsync(cancellationToken);
        return sales.Select(MapSale).ToArray();
    }

    public async Task<SalesResult<SaleDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var sale = await _salesRepository.GetByIdAsync(id, cancellationToken);
        return sale is null
            ? SalesResult<SaleDto>.NotFound()
            : SalesResult<SaleDto>.Success(MapSale(sale));
    }

    public async Task<SalesResult<SaleDto>> CreateAsync(
        CreateSaleCommand command,
        CancellationToken cancellationToken = default)
    {
        var sanitized = SanitizeCommand(command);
        var inventoryItemIds = sanitized.Lines.Select(line => line.InventoryItemId).Distinct().ToArray();
        var inventoryItems = await _inventoryRepository.GetItemsByIdsAsync(inventoryItemIds, cancellationToken);
        var inventoryItemsById = inventoryItems.ToDictionary(item => item.Id);

        var errors = ValidateCommand(sanitized, inventoryItemsById);
        if (errors.Count > 0)
        {
            return SalesResult<SaleDto>.Failed(errors);
        }

        var now = DateTimeOffset.UtcNow;
        var subtotalAmount = RoundMoney(sanitized.Lines.Sum(line => line.UnitPrice * line.Quantity));
        var discountPercentage = subtotalAmount > LoyaltyThreshold ? LoyaltyDiscountPercentage : 0;
        var discountAmount = discountPercentage == 0
            ? 0m
            : RoundMoney(subtotalAmount * discountPercentage / 100m);
        var totalAmount = RoundMoney(subtotalAmount - discountAmount);
        var invoiceNumber = await GenerateInvoiceNumberAsync(now, cancellationToken);

        var sale = new Sale
        {
            CustomerName = sanitized.CustomerName,
            VehicleNo = sanitized.VehicleNo,
            SoldBy = sanitized.SoldBy,
            Notes = sanitized.Notes,
            SaleDate = sanitized.SaleDate,
            SubtotalAmount = subtotalAmount,
            DiscountPercentage = discountPercentage,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            CreatedAt = now
        };

        foreach (var line in sanitized.Lines)
        {
            var inventoryItem = inventoryItemsById[line.InventoryItemId];
            var lineTotal = RoundMoney(line.UnitPrice * line.Quantity);

            sale.Items.Add(new SaleItem
            {
                InventoryItemId = inventoryItem.Id,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                LineTotal = lineTotal
            });

            inventoryItem.QuantityInStock -= line.Quantity;
            inventoryItem.UpdatedAt = now;

            await _inventoryRepository.AddStockChangeAsync(new InventoryStockChange
            {
                InventoryItemId = inventoryItem.Id,
                ChangeType = "Sale",
                QuantityChanged = -line.Quantity,
                QuantityAfterChange = inventoryItem.QuantityInStock,
                ReferenceCode = invoiceNumber,
                ChangedBy = sanitized.SoldBy,
                Notes = $"Sold to {sanitized.CustomerName} ({sanitized.VehicleNo}) via invoice {invoiceNumber}.",
                ChangedAt = now
            }, cancellationToken);
        }

        sale.Invoice = new SalesInvoice
        {
            InvoiceNumber = invoiceNumber,
            InvoiceDate = sanitized.SaleDate,
            GeneratedAt = now,
            CustomerName = sanitized.CustomerName,
            VehicleNo = sanitized.VehicleNo,
            IssuedBy = sanitized.SoldBy,
            SubtotalAmount = subtotalAmount,
            DiscountPercentage = discountPercentage,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount
        };

        await _salesRepository.AddAsync(sale, cancellationToken);
        await UpsertDailyReportAsync(
            sanitized.SaleDate,
            sanitized.Lines.Sum(line => line.Quantity),
            subtotalAmount,
            discountAmount,
            totalAmount,
            now,
            cancellationToken);
        await _salesRepository.SaveChangesAsync(cancellationToken);

        var saved = await _salesRepository.GetByIdAsync(sale.Id, cancellationToken);
        return SalesResult<SaleDto>.Success(MapSale(saved!));
    }

    public async Task<SalesReportSummaryDto> GetReportSummaryAsync(
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        var reports = await _salesRepository.GetReportsAsync(from, to, cancellationToken);
        var dailyReports = reports.Select(MapReport).ToArray();

        return new SalesReportSummaryDto(
            dailyReports.Sum(report => report.TotalSalesCount),
            dailyReports.Sum(report => report.TotalItemsSold),
            RoundMoney(dailyReports.Sum(report => report.GrossRevenue)),
            RoundMoney(dailyReports.Sum(report => report.TotalDiscount)),
            RoundMoney(dailyReports.Sum(report => report.NetRevenue)),
            dailyReports);
    }

    private async Task UpsertDailyReportAsync(
        DateOnly saleDate,
        int totalItemsSold,
        decimal grossRevenue,
        decimal totalDiscount,
        decimal netRevenue,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken)
    {
        var report = await _salesRepository.GetReportByDateAsync(saleDate, cancellationToken);
        if (report is null)
        {
            report = new SalesReport
            {
                ReportDate = saleDate,
                TotalSalesCount = 0,
                TotalItemsSold = 0,
                GrossRevenue = 0m,
                TotalDiscount = 0m,
                NetRevenue = 0m,
                UpdatedAt = updatedAt
            };

            await _salesRepository.AddReportAsync(report, cancellationToken);
        }

        report.TotalSalesCount += 1;
        report.TotalItemsSold += totalItemsSold;
        report.GrossRevenue = RoundMoney(report.GrossRevenue + grossRevenue);
        report.TotalDiscount = RoundMoney(report.TotalDiscount + totalDiscount);
        report.NetRevenue = RoundMoney(report.NetRevenue + netRevenue);
        report.UpdatedAt = updatedAt;
    }

    private static IReadOnlyCollection<SalesError> ValidateCommand(
        CreateSaleCommand command,
        IReadOnlyDictionary<int, InventoryItem> inventoryItemsById)
    {
        var errors = new List<SalesError>();

        if (string.IsNullOrWhiteSpace(command.CustomerName))
        {
            errors.Add(new SalesError(nameof(command.CustomerName), "Customer name is required."));
        }

        if (string.IsNullOrWhiteSpace(command.VehicleNo))
        {
            errors.Add(new SalesError(nameof(command.VehicleNo), "Vehicle number is required."));
        }

        if (string.IsNullOrWhiteSpace(command.SoldBy))
        {
            errors.Add(new SalesError(nameof(command.SoldBy), "Sold by is required."));
        }

        if (command.SaleDate == default)
        {
            errors.Add(new SalesError(nameof(command.SaleDate), "Sale date is required."));
        }

        if (command.Lines is null || command.Lines.Count == 0)
        {
            errors.Add(new SalesError("Lines", "At least one sales item is required."));
            return errors;
        }

        if (command.Lines.Any(line => line.Quantity <= 0))
        {
            errors.Add(new SalesError("Lines", "All sales items must have a quantity greater than zero."));
        }

        if (command.Lines.Any(line => line.UnitPrice <= 0))
        {
            errors.Add(new SalesError("Lines", "All sales items must have a unit price greater than zero."));
        }

        var duplicateItemIds = command.Lines
            .GroupBy(line => line.InventoryItemId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateItemIds.Length > 0)
        {
            errors.Add(new SalesError("Lines", "Each inventory item can only be added once per sale."));
        }

        foreach (var line in command.Lines)
        {
            if (!inventoryItemsById.TryGetValue(line.InventoryItemId, out var inventoryItem))
            {
                errors.Add(new SalesError(
                    "Lines",
                    $"Inventory item {line.InventoryItemId} could not be found."));
                continue;
            }

            if (line.Quantity > inventoryItem.QuantityInStock)
            {
                errors.Add(new SalesError(
                    "Lines",
                    $"Only {inventoryItem.QuantityInStock} units of {inventoryItem.Name} are available in stock."));
            }
        }

        return errors;
    }

    private async Task<string> GenerateInvoiceNumberAsync(
        DateTimeOffset timestamp,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var invoiceNumber = $"INV-{timestamp:yyyyMMddHHmmss}-{RandomNumberGenerator.GetInt32(1000, 9999)}";
            if (!await _salesRepository.InvoiceNumberExistsAsync(invoiceNumber, cancellationToken))
            {
                return invoiceNumber;
            }
        }

        return $"INV-{timestamp:yyyyMMddHHmmssfff}-{RandomNumberGenerator.GetInt32(10000, 99999)}";
    }

    private static SaleDto MapSale(Sale sale)
    {
        var items = sale.Items
            .Select(item => new SaleItemDto(
                item.Id,
                item.InventoryItemId,
                item.InventoryItem?.PartNumber ?? string.Empty,
                item.InventoryItem?.Name ?? string.Empty,
                item.Quantity,
                item.UnitPrice,
                item.LineTotal))
            .ToArray();

        var invoice = sale.Invoice is null
            ? new SalesInvoiceDto(
                0,
                sale.Id,
                string.Empty,
                sale.SaleDate,
                sale.CreatedAt,
                sale.CustomerName,
                sale.VehicleNo,
                sale.SoldBy,
                sale.SubtotalAmount,
                sale.DiscountPercentage,
                sale.DiscountAmount,
                sale.TotalAmount)
            : new SalesInvoiceDto(
                sale.Invoice.Id,
                sale.Invoice.SaleId,
                sale.Invoice.InvoiceNumber,
                sale.Invoice.InvoiceDate,
                sale.Invoice.GeneratedAt,
                sale.Invoice.CustomerName,
                sale.Invoice.VehicleNo,
                sale.Invoice.IssuedBy,
                sale.Invoice.SubtotalAmount,
                sale.Invoice.DiscountPercentage,
                sale.Invoice.DiscountAmount,
                sale.Invoice.TotalAmount);

        return new SaleDto(
            sale.Id,
            sale.CustomerName,
            sale.VehicleNo,
            sale.SoldBy,
            sale.SaleDate,
            sale.Notes,
            sale.CreatedAt,
            sale.SubtotalAmount,
            sale.DiscountPercentage,
            sale.DiscountAmount,
            sale.TotalAmount,
            sale.DiscountPercentage > 0,
            invoice,
            items);
    }

    private static DailySalesReportDto MapReport(SalesReport report)
    {
        return new DailySalesReportDto(
            report.ReportDate,
            report.TotalSalesCount,
            report.TotalItemsSold,
            report.GrossRevenue,
            report.TotalDiscount,
            report.NetRevenue,
            report.UpdatedAt);
    }

    private static CreateSaleCommand SanitizeCommand(CreateSaleCommand command)
    {
        return new CreateSaleCommand(
            command.CustomerName.Trim(),
            command.VehicleNo.Trim().ToUpperInvariant(),
            command.SoldBy.Trim(),
            command.SaleDate,
            command.Notes.Trim(),
            command.Lines
                .Select(line => new CreateSaleLineCommand(
                    line.InventoryItemId,
                    line.Quantity,
                    RoundMoney(line.UnitPrice)))
                .ToArray());
    }

    private static decimal RoundMoney(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
