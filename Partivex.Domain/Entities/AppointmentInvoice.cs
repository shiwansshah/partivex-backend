namespace Partivex.Domain.Entities;

public class AppointmentInvoice
{
    public int Id { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public Guid AppointmentId { get; set; }

    public Appointment Appointment { get; set; } = null!;

    public string CustomerId { get; set; } = string.Empty;

    public ApplicationUser Customer { get; set; } = null!;

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public string ServiceType { get; set; } = string.Empty;

    public string VehicleName { get; set; } = string.Empty;

    public string VehicleNumber { get; set; } = string.Empty;

    public DateTimeOffset InvoiceDate { get; set; }

    public decimal Amount { get; set; }

    public string PaymentStatus { get; set; } = "Pending";

    public string? Notes { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? PaidAt { get; set; }

    public DateTimeOffset? LastReminderEmailSentAt { get; set; }
}
