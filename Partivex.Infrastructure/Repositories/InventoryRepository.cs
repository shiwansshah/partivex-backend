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

    public async Task<IReadOnlyCollection<InventoryItem>> GetItemsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.InventoryItems
            .AsNoTracking()
            .OrderBy(item => item.Name)
            .ToArrayAsync(cancellationToken);
    }

    public Task<InventoryItem?> GetItemByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.InventoryItems.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public Task<bool> PartNumberExistsAsync(
        string partNumber,
        int? excludingId = null,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.InventoryItems.AnyAsync(
            item => item.PartNumber == partNumber && (!excludingId.HasValue || item.Id != excludingId.Value),
            cancellationToken);
    }

    public async Task AddItemAsync(InventoryItem item, CancellationToken cancellationToken = default)
    {
        await _dbContext.InventoryItems.AddAsync(item, cancellationToken);
    }

    public async Task AddStockChangeAsync(InventoryStockChange stockChange, CancellationToken cancellationToken = default)
    {
        await _dbContext.InventoryStockChanges.AddAsync(stockChange, cancellationToken);
    }

    public void RemoveItem(InventoryItem item)
    {
        _dbContext.InventoryItems.Remove(item);
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
            .Include(change => change.InventoryItem)
            .OrderByDescending(change => change.ChangedAt)
            .Take(take)
            .ToArrayAsync(cancellationToken);
    }
}
