using Microsoft.EntityFrameworkCore;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Infrastructure.Data;

namespace Partivex.Infrastructure.Repositories;

public sealed class FinancialReportRepository : IFinancialReportRepository
{
    private readonly AppDbContext _dbContext;

    public FinancialReportRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PurchaseInvoice>> GetPurchaseInvoicesAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PurchaseInvoices
            .AsNoTracking()
            .Include(invoice => invoice.Items)
            .Where(invoice => invoice.InvoiceDate >= start && invoice.InvoiceDate < end)
            .OrderByDescending(invoice => invoice.InvoiceDate)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CustomerPartPurchaseInvoice>> GetCustomerPartPurchaseInvoicesAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CustomerPartPurchaseInvoices
            .AsNoTracking()
            .Where(invoice => invoice.InvoiceDate >= start && invoice.InvoiceDate < end)
            .OrderByDescending(invoice => invoice.InvoiceDate)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AppointmentInvoice>> GetAppointmentInvoicesAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AppointmentInvoices
            .AsNoTracking()
            .Where(invoice => invoice.InvoiceDate >= start && invoice.InvoiceDate < end)
            .OrderByDescending(invoice => invoice.InvoiceDate)
            .ToArrayAsync(cancellationToken);
    }
}
