using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface IPurchaseService
{
    Task<IReadOnlyCollection<PurchaseInvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PurchaseResult<PurchaseInvoiceDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PurchaseResult<PurchaseInvoiceDto>> CreateAsync(
        CreatePurchaseInvoiceCommand command,
        CancellationToken cancellationToken = default);

    Task<PurchaseResult<PurchaseInvoiceDto>> ConfirmAsync(int id, CancellationToken cancellationToken = default);

    Task<PurchaseResult<int>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
