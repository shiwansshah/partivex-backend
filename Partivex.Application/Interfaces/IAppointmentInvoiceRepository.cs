using Partivex.Domain.Entities;

namespace Partivex.Application.Interfaces;

public interface IAppointmentInvoiceRepository
{
    Task<IReadOnlyList<AppointmentInvoice>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppointmentInvoice>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppointmentInvoice>> GetOverduePendingAsync(DateTimeOffset cutoff, CancellationToken cancellationToken = default);

    Task<AppointmentInvoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> InvoiceNumberExistsAsync(string invoiceNumber, CancellationToken cancellationToken = default);

    Task<bool> AppointmentHasInvoiceAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    Task AddAsync(AppointmentInvoice invoice, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
