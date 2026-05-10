using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface IInventoryService
{
    Task<InventoryMonitoringDto> GetMonitoringAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<InventoryItemDto>> GetItemsAsync(CancellationToken cancellationToken = default);

    Task<InventoryResult<InventoryItemDto>> CreateItemAsync(
        UpsertInventoryItemCommand command,
        CancellationToken cancellationToken = default);

    Task<InventoryResult<InventoryItemDto>> UpdateItemAsync(
        int id,
        UpsertInventoryItemCommand command,
        CancellationToken cancellationToken = default);

    Task<InventoryResult<InventoryDeletedResponse>> DeleteItemAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<InventoryStockChangeDto>> GetRecentStockChangesAsync(
        CancellationToken cancellationToken = default);
}
