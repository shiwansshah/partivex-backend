using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface IAppointmentInvoiceService
{
    Task<IReadOnlyList<StaffAppointmentListDto>> GetAppointmentsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppointmentInvoiceDto>> GetAllInvoicesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppointmentInvoiceDto>> GetCustomerInvoicesAsync(string customerId, CancellationToken cancellationToken = default);

    Task<CustomerPortalResult<AppointmentInvoiceDto>> CreateInvoiceAsync(CreateAppointmentInvoiceDto dto, string createdBy, CancellationToken cancellationToken = default);

    Task<CustomerPortalResult<AppointmentInvoiceDto>> UpdatePaymentStatusAsync(int id, UpdateAppointmentInvoicePaymentDto dto, string? customerId, CancellationToken cancellationToken = default);

    Task<CustomerPortalResult<byte[]>> GenerateInvoicePdfAsync(int id, string? customerId = null, CancellationToken cancellationToken = default);

    Task<CustomerPortalResult<AppointmentInvoiceEmailResult>> SendInvoiceEmailAsync(int id, string? email = null, CancellationToken cancellationToken = default);

    Task<OverdueAppointmentInvoiceEmailResult> SendOverdueReminderEmailsAsync(CancellationToken cancellationToken = default);
}
