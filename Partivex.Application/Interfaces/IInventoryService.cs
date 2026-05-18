using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface IInventoryService
{
    Task<InventoryMonitoringDto> GetMonitoringAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<InventoryItemDto>> GetItemsAsync(CancellationToken cancellationToken = default);

    Task<InventoryResult<PurchaseInvoiceDto>> AddStockAsync(
        AddStockCommand command,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<InventoryStockChangeDto>> GetRecentStockChangesAsync(
        CancellationToken cancellationToken = default);
}
