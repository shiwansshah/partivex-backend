using Partivex.Domain.Entities;

namespace Partivex.Application.Interfaces;

public interface IInventoryRepository
{
    Task<IReadOnlyCollection<Part>> GetPartsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<InventoryItem>> GetItemsByIdsAsync(int[] ids, CancellationToken cancellationToken = default);

    Task<InventoryItem?> GetByPartNumberAsync(string partNumber, CancellationToken cancellationToken = default);

    Task AddInventoryItemAsync(InventoryItem item, CancellationToken cancellationToken = default);

    Task AddStockChangeAsync(InventoryStockChange stockChange, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<InventoryStockChange>> GetRecentStockChangesAsync(
        int take = 12,
        CancellationToken cancellationToken = default);
}
