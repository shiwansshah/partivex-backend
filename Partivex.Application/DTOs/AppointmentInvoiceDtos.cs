using System.ComponentModel.DataAnnotations;

namespace Partivex.Application.DTOs;

public sealed record StaffAppointmentListDto(
    Guid Id,
    string CustomerId,
    string CustomerName,
    string CustomerEmail,
    Guid VehicleId,
    string VehicleName,
    string VehicleNumber,
    string ServiceType,
    DateTimeOffset PreferredAt,
    string Status,
    DateTimeOffset CreatedAt);

public sealed record AppointmentInvoiceDto(
    int Id,
    string InvoiceNumber,
    Guid AppointmentId,
    string CustomerId,
    string CustomerName,
    string CustomerEmail,
    string ServiceType,
    string VehicleName,
    string VehicleNumber,
    DateTimeOffset InvoiceDate,
    decimal Amount,
    string PaymentStatus,
    string? Notes,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PaidAt,
    DateTimeOffset? LastReminderEmailSentAt);

public sealed class CreateAppointmentInvoiceDto
{
    [Required]
    public Guid AppointmentId { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; init; }

    [Required]
    [MaxLength(24)]
    public string PaymentStatus { get; init; } = "Pending";

    [MaxLength(500)]
    public string? Notes { get; init; }
}

public sealed class UpdateAppointmentInvoicePaymentDto
{
    [Required]
    [MaxLength(24)]
    public string PaymentStatus { get; init; } = "Pending";
}

public sealed record AppointmentInvoiceEmailResult(string Message, bool EmailSent);

public sealed record OverdueAppointmentInvoiceEmailResult(int SentCount, int SkippedCount);

public sealed record SmtpSettingDto(string SenderEmail);

public sealed class UpdateSmtpSettingDto
{
    [Required]
    [EmailAddress]
    [MaxLength(160)]
    public string SenderEmail { get; init; } = string.Empty;
}
