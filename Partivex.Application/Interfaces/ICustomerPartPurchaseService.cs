using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface ICustomerPartPurchaseService
{
    Task<IReadOnlyList<CustomerPartCatalogDto>> GetCatalogAsync(CancellationToken cancellationToken = default);

    Task<CustomerPartPurchaseResult<CustomerPartInvoiceDto>> CheckoutAsync(
        string customerId,
        CustomerPartCheckoutRequest request,
        string source,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerPartInvoiceDto>> GetAllInvoicesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerPartInvoiceDto>> GetCustomerInvoicesAsync(string customerId, CancellationToken cancellationToken = default);

    Task<CustomerPartPurchaseResult<CustomerPartInvoiceDto>> GetInvoiceAsync(
        int id,
        string? customerId = null,
        CancellationToken cancellationToken = default);

    Task<CustomerPartPurchaseResult<StaffPartRequestApprovalResultDto>> ApprovePartRequestAsync(
        Guid partRequestId,
        ApprovePartRequestDto request,
        string staffIdentifier,
        CancellationToken cancellationToken = default);

    Task<CustomerPartPurchaseResult<CustomerPartInvoiceEmailResult>> SendInvoiceEmailAsync(
        int id,
        string email,
        CancellationToken cancellationToken = default);

    Task<CustomerPartPurchaseResult<byte[]>> GenerateInvoicePdfAsync(
        int id,
        string? customerId = null,
        CancellationToken cancellationToken = default);
}
