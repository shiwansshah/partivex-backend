using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Infrastructure.Data;

namespace Partivex.Infrastructure.Services;

public sealed class AdminDashboardService : IAdminDashboardService
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminDashboardService> _logger;

    public AdminDashboardService(
        AppDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        ILogger<AdminDashboardService> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<AdminDashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var staffUsers = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Staff);
            var customerUsers = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Customer);
            var salesPeriodStart = new DateTimeOffset(
                DateTimeOffset.UtcNow.Year,
                DateTimeOffset.UtcNow.Month,
                1,
                0,
                0,
                0,
                TimeSpan.Zero);
            var salesPeriodEnd = salesPeriodStart.AddMonths(1);

            var vehicleCount = await _dbContext.Vehicles.AsNoTracking().CountAsync(cancellationToken);
            var partSales = await GetCustomerPartSalesTotalAsync(salesPeriodStart, salesPeriodEnd, cancellationToken);
            var appointmentSales = await GetAppointmentSalesTotalAsync(salesPeriodStart, salesPeriodEnd, cancellationToken);
            var totalStock = await _dbContext.Parts
                .AsNoTracking()
                .Where(part => part.IsActive)
                .Select(part => (int?)part.CurrentStock)
                .SumAsync(cancellationToken);
            var lowStockParts = await _dbContext.Parts
                .AsNoTracking()
                .CountAsync(part => part.IsActive && part.CurrentStock <= part.MinimumStockLevel, cancellationToken);
            var lineGraphData = await GetSalesTrendAsync(cancellationToken);
            var histogramData = await GetStockHistogramAsync(cancellationToken);

            return new AdminDashboardSummaryDto(
                staffUsers.Count,
                customerUsers.Count,
                vehicleCount,
                partSales + appointmentSales,
                totalStock ?? 0,
                lowStockParts,
                lineGraphData,
                histogramData);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to load admin dashboard summary.");
            throw;
        }
    }

    private async Task<decimal> GetCustomerPartSalesTotalAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken)
    {
        var total = await _dbContext.CustomerPartPurchaseInvoices
            .AsNoTracking()
            .Where(invoice =>
                invoice.InvoiceDate >= start &&
                invoice.InvoiceDate < end &&
                SalesRecognitionRules.RecognizedStatusValues.Contains(invoice.Status.ToLower()))
            .Select(invoice => (decimal?)invoice.TotalAmount)
            .SumAsync(cancellationToken);

        return total ?? 0m;
    }

    private async Task<decimal> GetAppointmentSalesTotalAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken)
    {
        var total = await _dbContext.AppointmentInvoices
            .AsNoTracking()
            .Where(invoice =>
                invoice.InvoiceDate >= start &&
                invoice.InvoiceDate < end &&
                SalesRecognitionRules.RecognizedStatusValues.Contains(invoice.PaymentStatus.ToLower()))
            .Select(invoice => (decimal?)invoice.Amount)
            .SumAsync(cancellationToken);

        return total ?? 0m;
    }

    private async Task<IReadOnlyCollection<DashboardChartPointDto>> GetSalesTrendAsync(CancellationToken cancellationToken)
    {
        var today = DateTimeOffset.UtcNow.Date;
        var start = new DateTimeOffset(today.AddDays(-6), TimeSpan.Zero);

        var partSales = await _dbContext.CustomerPartPurchaseInvoices
            .AsNoTracking()
            .Where(invoice =>
                invoice.InvoiceDate >= start &&
                SalesRecognitionRules.RecognizedStatusValues.Contains(invoice.Status.ToLower()))
            .Select(invoice => new SalesPoint(invoice.InvoiceDate, invoice.TotalAmount))
            .ToArrayAsync(cancellationToken);

        var appointmentSales = await _dbContext.AppointmentInvoices
            .AsNoTracking()
            .Where(invoice =>
                invoice.InvoiceDate >= start &&
                SalesRecognitionRules.RecognizedStatusValues.Contains(invoice.PaymentStatus.ToLower()))
            .Select(invoice => new SalesPoint(invoice.InvoiceDate, invoice.Amount))
            .ToArrayAsync(cancellationToken);

        var salesByDate = partSales
            .Concat(appointmentSales)
            .GroupBy(sale => sale.InvoiceDate.Date)
            .ToDictionary(group => group.Key, group => group.Sum(sale => sale.Amount));

        return Enumerable.Range(0, 7)
            .Select(offset => today.AddDays(offset - 6))
            .Select(date => new DashboardChartPointDto(
                date.ToString("MMM d"),
                salesByDate.TryGetValue(date, out var total) ? total : 0m))
            .ToArray();
    }

    private async Task<IReadOnlyCollection<DashboardChartPointDto>> GetStockHistogramAsync(CancellationToken cancellationToken)
    {
        var stockByCategory = await _dbContext.Parts
            .AsNoTracking()
            .Where(part => part.IsActive)
            .GroupBy(part => part.Category)
            .Select(group => new
            {
                Category = group.Key,
                Quantity = group.Sum(part => part.CurrentStock)
            })
            .OrderByDescending(item => item.Quantity)
            .Take(8)
            .ToArrayAsync(cancellationToken);

        return stockByCategory
            .Select(item => new DashboardChartPointDto(
                string.IsNullOrWhiteSpace(item.Category) ? "Uncategorized" : item.Category,
                item.Quantity))
            .ToArray();
    }

    private sealed record SalesPoint(DateTimeOffset InvoiceDate, decimal Amount);
}
