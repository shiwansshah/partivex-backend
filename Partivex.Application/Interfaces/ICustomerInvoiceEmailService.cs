using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface ICustomerInvoiceEmailService
{
    Task<CustomerPartInvoiceEmailResult> SendInvoiceAsync(
        CustomerPartInvoiceDto invoice,
        string email,
        byte[] pdfBytes,
        CancellationToken cancellationToken = default);

    Task<AppointmentInvoiceEmailResult> SendAppointmentInvoiceAsync(
        AppointmentInvoiceDto invoice,
        string email,
        byte[] pdfBytes,
        CancellationToken cancellationToken = default);
}
