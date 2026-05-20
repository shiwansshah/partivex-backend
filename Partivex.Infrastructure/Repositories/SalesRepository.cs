using Microsoft.EntityFrameworkCore;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Infrastructure.Data;

namespace Partivex.Infrastructure.Repositories;

public sealed class SalesRepository : ISalesRepository
{
    private readonly AppDbContext _dbContext;

    public SalesRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Sale>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Sales
            .AsNoTracking()
            .Include(sale => sale.Invoice)
            .Include(sale => sale.Items)
                .ThenInclude(item => item.InventoryItem)
            .OrderByDescending(sale => sale.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Sale?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Sales
            .Include(sale => sale.Invoice)
            .Include(sale => sale.Items)
                .ThenInclude(item => item.InventoryItem)
            .FirstOrDefaultAsync(sale => sale.Id == id, cancellationToken);
    }

    public Task<bool> InvoiceNumberExistsAsync(string invoiceNumber, CancellationToken cancellationToken = default)
    {
        return _dbContext.SalesInvoices
            .AnyAsync(invoice => invoice.InvoiceNumber == invoiceNumber, cancellationToken);
    }

    public async Task AddAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _dbContext.Sales.AddAsync(sale, cancellationToken);
    }

    public Task<SalesReport?> GetReportByDateAsync(DateOnly reportDate, CancellationToken cancellationToken = default)
    {
        return _dbContext.SalesReports
            .FirstOrDefaultAsync(report => report.ReportDate == reportDate, cancellationToken);
    }

    public async Task AddReportAsync(SalesReport report, CancellationToken cancellationToken = default)
    {
        await _dbContext.SalesReports.AddAsync(report, cancellationToken);
    }

    public async Task<IReadOnlyCollection<SalesReport>> GetReportsAsync(
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SalesReports.AsNoTracking().AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(report => report.ReportDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(report => report.ReportDate <= to.Value);
        }

        return await query
            .OrderByDescending(report => report.ReportDate)
            .ToArrayAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
