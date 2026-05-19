using System.Text;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Domain.Enums;

namespace Partivex.Application.Services;

public sealed class AppointmentInvoiceService : IAppointmentInvoiceService
{
    private readonly IAppointmentInvoiceRepository _invoiceRepository;
    private readonly ICustomerAppointmentRepository _appointmentRepository;
    private readonly ICustomerInvoiceEmailService _emailService;

    public AppointmentInvoiceService(
        IAppointmentInvoiceRepository invoiceRepository,
        ICustomerAppointmentRepository appointmentRepository,
        ICustomerInvoiceEmailService emailService)
    {
        _invoiceRepository = invoiceRepository;
        _appointmentRepository = appointmentRepository;
        _emailService = emailService;
    }

    public async Task<IReadOnlyList<StaffAppointmentListDto>> GetAppointmentsAsync(CancellationToken cancellationToken = default)
    {
        var appointments = await _appointmentRepository.GetAllAsync(cancellationToken);
        return appointments.Select(MapAppointment).ToArray();
    }

    public async Task<IReadOnlyList<AppointmentInvoiceDto>> GetAllInvoicesAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await _invoiceRepository.GetAllAsync(cancellationToken);
        return invoices.Select(MapInvoice).ToArray();
    }

    public async Task<IReadOnlyList<AppointmentInvoiceDto>> GetCustomerInvoicesAsync(string customerId, CancellationToken cancellationToken = default)
    {
        var invoices = await _invoiceRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        return invoices.Select(MapInvoice).ToArray();
    }

    public async Task<CustomerPortalResult<AppointmentInvoiceDto>> CreateInvoiceAsync(
        CreateAppointmentInvoiceDto dto,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<CustomerPortalError>();
        if (dto.AppointmentId == Guid.Empty) errors.Add(new CustomerPortalError(nameof(dto.AppointmentId), "Select an appointment."));
        if (dto.Amount <= 0) errors.Add(new CustomerPortalError(nameof(dto.Amount), "Invoice amount must be greater than zero."));

        var paymentStatus = NormalizePaymentStatus(dto.PaymentStatus, errors);
        if (errors.Count > 0) return CustomerPortalResult<AppointmentInvoiceDto>.Failed(errors, "Appointment invoice could not be created.");

        var appointment = await _appointmentRepository.GetByIdAsync(dto.AppointmentId, cancellationToken);
        if (appointment is null) return CustomerPortalResult<AppointmentInvoiceDto>.NotFound("Appointment not found.");

        if (await _invoiceRepository.AppointmentHasInvoiceAsync(dto.AppointmentId, cancellationToken))
        {
            return CustomerPortalResult<AppointmentInvoiceDto>.Failed(
            [
                new CustomerPortalError(nameof(dto.AppointmentId), "This appointment already has an invoice.")
            ],
            "Appointment invoice could not be created.");
        }

        var now = DateTimeOffset.UtcNow;
        var invoice = new AppointmentInvoice
        {
            InvoiceNumber = await CreateInvoiceNumberAsync(cancellationToken),
            AppointmentId = appointment.Id,
            CustomerId = appointment.CustomerId,
            CustomerName = string.IsNullOrWhiteSpace(appointment.Customer.FullName) ? appointment.Customer.Email ?? "Customer" : appointment.Customer.FullName,
            CustomerEmail = appointment.Customer.Email ?? string.Empty,
            ServiceType = appointment.ServiceType,
            VehicleName = appointment.Vehicle?.Name ?? string.Empty,
            VehicleNumber = appointment.Vehicle?.Number ?? string.Empty,
            InvoiceDate = now,
            Amount = dto.Amount,
            PaymentStatus = paymentStatus,
            Notes = NormalizeOptional(dto.Notes),
            CreatedBy = createdBy,
            CreatedAt = now,
            PaidAt = paymentStatus == "Paid" ? now : null
        };

        if (appointment.Status == AppointmentStatus.Pending)
        {
            appointment.Status = AppointmentStatus.Confirmed;
            appointment.UpdatedAt = now;
        }

        await _invoiceRepository.AddAsync(invoice, cancellationToken);
        await _invoiceRepository.SaveChangesAsync(cancellationToken);

        var saved = await _invoiceRepository.GetByIdAsync(invoice.Id, cancellationToken);
        return CustomerPortalResult<AppointmentInvoiceDto>.Success(MapInvoice(saved!));
    }

    public async Task<CustomerPortalResult<AppointmentInvoiceDto>> UpdatePaymentStatusAsync(
        int id,
        UpdateAppointmentInvoicePaymentDto dto,
        string? customerId,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<CustomerPortalError>();
        var paymentStatus = NormalizePaymentStatus(dto.PaymentStatus, errors);
        if (errors.Count > 0) return CustomerPortalResult<AppointmentInvoiceDto>.Failed(errors, "Payment status could not be updated.");

        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice is null || (!string.IsNullOrWhiteSpace(customerId) && invoice.CustomerId != customerId))
        {
            return CustomerPortalResult<AppointmentInvoiceDto>.NotFound("Appointment invoice not found.");
        }

        invoice.PaymentStatus = paymentStatus;
        invoice.PaidAt = paymentStatus == "Paid" ? DateTimeOffset.UtcNow : null;
        await _invoiceRepository.SaveChangesAsync(cancellationToken);

        return CustomerPortalResult<AppointmentInvoiceDto>.Success(MapInvoice(invoice));
    }

    public async Task<CustomerPortalResult<byte[]>> GenerateInvoicePdfAsync(int id, string? customerId = null, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice is null || (!string.IsNullOrWhiteSpace(customerId) && invoice.CustomerId != customerId))
        {
            return CustomerPortalResult<byte[]>.NotFound("Appointment invoice not found.");
        }

        return CustomerPortalResult<byte[]>.Success(BuildInvoicePdf(MapInvoice(invoice)));
    }

    public async Task<CustomerPortalResult<AppointmentInvoiceEmailResult>> SendInvoiceEmailAsync(int id, string? email = null, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice is null) return CustomerPortalResult<AppointmentInvoiceEmailResult>.NotFound("Appointment invoice not found.");

        var dto = MapInvoice(invoice);
        var pdf = BuildInvoicePdf(dto);
        var result = await _emailService.SendAppointmentInvoiceAsync(dto, string.IsNullOrWhiteSpace(email) ? dto.CustomerEmail : email.Trim(), pdf, cancellationToken);
        return CustomerPortalResult<AppointmentInvoiceEmailResult>.Success(result);
    }

    public async Task<OverdueAppointmentInvoiceEmailResult> SendOverdueReminderEmailsAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddMonths(-1);
        var invoices = await _invoiceRepository.GetOverduePendingAsync(cutoff, cancellationToken);
        var sent = 0;
        var skipped = 0;

        foreach (var invoice in invoices)
        {
            if (string.IsNullOrWhiteSpace(invoice.CustomerEmail) || invoice.LastReminderEmailSentAt >= cutoff)
            {
                skipped++;
                continue;
            }

            var dto = MapInvoice(invoice);
            var result = await _emailService.SendAppointmentInvoiceAsync(dto, invoice.CustomerEmail, BuildInvoicePdf(dto), cancellationToken);
            if (result.EmailSent)
            {
                invoice.LastReminderEmailSentAt = DateTimeOffset.UtcNow;
                sent++;
            }
            else
            {
                skipped++;
            }
        }

        await _invoiceRepository.SaveChangesAsync(cancellationToken);
        return new OverdueAppointmentInvoiceEmailResult(sent, skipped);
    }

    private async Task<string> CreateInvoiceNumberAsync(CancellationToken cancellationToken)
    {
        string invoiceNumber;
        do
        {
            invoiceNumber = $"AI-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";
        }
        while (await _invoiceRepository.InvoiceNumberExistsAsync(invoiceNumber, cancellationToken));

        return invoiceNumber;
    }

    private static string NormalizePaymentStatus(string? status, List<CustomerPortalError> errors)
    {
        var normalized = string.Equals(status, "Paid", StringComparison.OrdinalIgnoreCase) ? "Paid" : "Pending";
        if (!string.Equals(normalized, status?.Trim(), StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(status))
        {
            errors.Add(new CustomerPortalError(nameof(status), "Payment status must be Paid or Pending."));
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static StaffAppointmentListDto MapAppointment(Appointment appointment)
    {
        return new StaffAppointmentListDto(
            appointment.Id,
            appointment.CustomerId,
            string.IsNullOrWhiteSpace(appointment.Customer.FullName) ? appointment.Customer.Email ?? "Customer" : appointment.Customer.FullName,
            appointment.Customer.Email ?? string.Empty,
            appointment.VehicleId,
            appointment.Vehicle?.Name ?? string.Empty,
            appointment.Vehicle?.Number ?? string.Empty,
            appointment.ServiceType,
            appointment.PreferredAt,
            appointment.Status.ToString(),
            appointment.CreatedAt);
    }

    private static AppointmentInvoiceDto MapInvoice(AppointmentInvoice invoice)
    {
        return new AppointmentInvoiceDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.AppointmentId,
            invoice.CustomerId,
            invoice.CustomerName,
            invoice.CustomerEmail,
            invoice.ServiceType,
            invoice.VehicleName,
            invoice.VehicleNumber,
            invoice.InvoiceDate,
            invoice.Amount,
            invoice.PaymentStatus,
            invoice.Notes,
            invoice.CreatedBy,
            invoice.CreatedAt,
            invoice.PaidAt,
            invoice.LastReminderEmailSentAt);
    }

    private static byte[] BuildInvoicePdf(AppointmentInvoiceDto invoice)
    {
        var lines = new[]
        {
            "Partivex",
            $"Appointment Invoice {invoice.InvoiceNumber}",
            $"Date: {invoice.InvoiceDate:yyyy-MM-dd HH:mm}",
            $"Customer: {invoice.CustomerName}",
            $"Email: {invoice.CustomerEmail}",
            $"Service: {invoice.ServiceType}",
            $"Vehicle: {invoice.VehicleName} - {invoice.VehicleNumber}",
            $"Payment Status: {invoice.PaymentStatus}",
            "",
            $"Amount Due: NPR {invoice.Amount:0.00}",
            $"Notes: {invoice.Notes ?? "No notes"}"
        };

        return SimplePdf(lines);
    }

    private static byte[] SimplePdf(IReadOnlyList<string> lines)
    {
        var objects = new List<string>();
        var content = new StringBuilder("BT\n/F1 11 Tf\n50 790 Td\n14 TL\n");
        foreach (var line in lines)
        {
            content.Append('(').Append(line.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)")).Append(") Tj\nT*\n");
        }
        content.Append("ET");
        objects.Add("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");
        objects.Add("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");
        objects.Add("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>\nendobj\n");
        objects.Add("4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n");
        objects.Add($"5 0 obj\n<< /Length {Encoding.ASCII.GetByteCount(content.ToString())} >>\nstream\n{content}\nendstream\nendobj\n");

        var pdf = new StringBuilder("%PDF-1.4\n");
        var offsets = new List<int> { 0 };
        foreach (var obj in objects)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(pdf.ToString()));
            pdf.Append(obj);
        }
        var xrefOffset = Encoding.ASCII.GetByteCount(pdf.ToString());
        pdf.Append("xref\n0 6\n0000000000 65535 f \n");
        for (var i = 1; i < offsets.Count; i++) pdf.Append(offsets[i].ToString("0000000000")).Append(" 00000 n \n");
        pdf.Append("trailer\n<< /Root 1 0 R /Size 6 >>\nstartxref\n").Append(xrefOffset).Append("\n%%EOF");
        return Encoding.ASCII.GetBytes(pdf.ToString());
    }
}
