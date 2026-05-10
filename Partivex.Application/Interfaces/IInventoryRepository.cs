using Partivex.Domain.Entities;

namespace Partivex.Application.Interfaces;

public interface IInventoryRepository
{
    Task<IReadOnlyCollection<InventoryItem>> GetItemsAsync(CancellationToken cancellationToken = default);

    Task<InventoryItem?> GetItemByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> PartNumberExistsAsync(
        string partNumber,
        int? excludingId = null,
        CancellationToken cancellationToken = default);

    Task AddItemAsync(InventoryItem item, CancellationToken cancellationToken = default);

    Task AddStockChangeAsync(InventoryStockChange stockChange, CancellationToken cancellationToken = default);

    void RemoveItem(InventoryItem item);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<InventoryStockChange>> GetRecentStockChangesAsync(
        int take = 12,
        CancellationToken cancellationToken = default);
}
