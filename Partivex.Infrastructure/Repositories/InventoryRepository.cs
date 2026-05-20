using Microsoft.EntityFrameworkCore;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Infrastructure.Data;

namespace Partivex.Infrastructure.Repositories;

public sealed class InventoryRepository : IInventoryRepository
{
    private readonly AppDbContext _dbContext;

    public InventoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Part>> GetPartsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Parts
            .AsNoTracking()
            .Where(part => part.IsActive)
            .OrderBy(part => part.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<InventoryItem>> GetItemsByIdsAsync(int[] ids, CancellationToken cancellationToken = default)
    {
        return await _dbContext.InventoryItems
            .Where(item => ids.Contains(item.Id))
            .ToArrayAsync(cancellationToken);
    }

    public Task<InventoryItem?> GetByPartNumberAsync(string partNumber, CancellationToken cancellationToken = default)
    {
        return _dbContext.InventoryItems
            .FirstOrDefaultAsync(item => item.PartNumber == partNumber, cancellationToken);
    }

    public async Task AddInventoryItemAsync(InventoryItem item, CancellationToken cancellationToken = default)
    {
        await _dbContext.InventoryItems.AddAsync(item, cancellationToken);
    }

    public async Task AddStockChangeAsync(InventoryStockChange stockChange, CancellationToken cancellationToken = default)
    {
        await _dbContext.InventoryStockChanges.AddAsync(stockChange, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<InventoryStockChange>> GetRecentStockChangesAsync(
        int take = 12,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.InventoryStockChanges
            .AsNoTracking()
            .Include(change => change.Part)
            .Include(change => change.Vendor)
            .OrderByDescending(change => change.ChangedAt)
            .Take(take)
            .ToArrayAsync(cancellationToken);
    }
}
