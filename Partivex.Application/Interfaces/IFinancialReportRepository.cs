using Partivex.Domain.Entities;

namespace Partivex.Application.Interfaces;

public interface IFinancialReportRepository
{
    Task<IReadOnlyList<PurchaseInvoice>> GetPurchaseInvoicesAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerPartPurchaseInvoice>> GetCustomerPartPurchaseInvoicesAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppointmentInvoice>> GetAppointmentInvoicesAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default);
}
