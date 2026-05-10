using Microsoft.EntityFrameworkCore;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Infrastructure.Data;

namespace Partivex.Infrastructure.Repositories;

public sealed class PurchaseRepository : IPurchaseRepository
{
    private readonly AppDbContext _dbContext;

    public PurchaseRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<PurchaseInvoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.PurchaseInvoices
            .AsNoTracking()
            .Include(invoice => invoice.Items)
                .ThenInclude(item => item.InventoryItem)
            .OrderByDescending(invoice => invoice.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public Task<PurchaseInvoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.PurchaseInvoices
            .Include(invoice => invoice.Items)
                .ThenInclude(item => item.InventoryItem)
            .FirstOrDefaultAsync(invoice => invoice.Id == id, cancellationToken);
    }

    public Task<bool> InvoiceNumberExistsAsync(string invoiceNumber, CancellationToken cancellationToken = default)
    {
        return _dbContext.PurchaseInvoices
            .AnyAsync(invoice => invoice.InvoiceNumber == invoiceNumber, cancellationToken);
    }

    public async Task AddAsync(PurchaseInvoice invoice, CancellationToken cancellationToken = default)
    {
        await _dbContext.PurchaseInvoices.AddAsync(invoice, cancellationToken);
    }

    public void Remove(PurchaseInvoice invoice)
    {
        _dbContext.PurchaseInvoices.Remove(invoice);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
