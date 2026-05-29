using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface IEsewaPaymentService
{
    Task<CustomerPartPurchaseResult<EsewaPaymentInitiationDto>> CreatePartCheckoutPaymentAsync(
        string customerId,
        CustomerPartCheckoutRequest request,
        string source,
        CancellationToken cancellationToken = default);

    Task<CustomerPartPurchaseResult<EsewaPaymentInitiationDto>> CreatePartInvoicePaymentAsync(
        int invoiceId,
        string customerId,
        CancellationToken cancellationToken = default);

    Task<CustomerPortalResult<EsewaPaymentInitiationDto>> CreateAppointmentInvoicePaymentAsync(
        int invoiceId,
        string customerId,
        CancellationToken cancellationToken = default);

    Task<EsewaPaymentCallbackResult> CompletePaymentAsync(string encodedData, CancellationToken cancellationToken = default);

    Task<EsewaPaymentCallbackResult> FailPaymentAsync(string? encodedData, CancellationToken cancellationToken = default);
}
