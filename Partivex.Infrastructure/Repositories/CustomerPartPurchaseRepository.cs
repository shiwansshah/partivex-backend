using Microsoft.EntityFrameworkCore;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Infrastructure.Data;

namespace Partivex.Infrastructure.Repositories;

public sealed class CustomerPartPurchaseRepository : ICustomerPartPurchaseRepository
{
    private readonly AppDbContext _dbContext;

    public CustomerPartPurchaseRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CustomerPartPurchaseInvoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await BaseQuery()
            .AsNoTracking()
            .OrderByDescending(invoice => invoice.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CustomerPartPurchaseInvoice>> GetByCustomerIdAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        return await BaseQuery()
            .AsNoTracking()
            .Where(invoice => invoice.CustomerId == customerId)
            .OrderByDescending(invoice => invoice.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public Task<CustomerPartPurchaseInvoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return BaseQuery().FirstOrDefaultAsync(invoice => invoice.Id == id, cancellationToken);
    }

    public Task<bool> InvoiceNumberExistsAsync(string invoiceNumber, CancellationToken cancellationToken = default)
    {
        return _dbContext.CustomerPartPurchaseInvoices
            .AnyAsync(invoice => invoice.InvoiceNumber == invoiceNumber, cancellationToken);
    }

    public async Task AddAsync(CustomerPartPurchaseInvoice invoice, CancellationToken cancellationToken = default)
    {
        await _dbContext.CustomerPartPurchaseInvoices.AddAsync(invoice, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<CustomerPartPurchaseInvoice> BaseQuery()
    {
        return _dbContext.CustomerPartPurchaseInvoices
            .Include(invoice => invoice.Customer)
            .Include(invoice => invoice.PartRequest)
            .Include(invoice => invoice.Items)
                .ThenInclude(item => item.Part);
    }
}
