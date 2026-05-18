using System.ComponentModel.DataAnnotations; // Imports validation attributes.

namespace Partivex.Application.DTOs; // Defines DTO namespace.

public sealed record CustomerDto( // Defines customer list DTO.
    [Required] // Requires customer id.
    [StringLength(450)] // Limits customer id length.
    string Id, // Carries customer id.
    [Required] // Requires full name.
    [StringLength(256)] // Limits full name length.
    string FullName, // Carries customer full name.
    [Required] // Requires email.
    [EmailAddress] // Validates email format.
    [StringLength(256)] // Limits email length.
    string Email, // Carries customer email.
    [StringLength(32)] // Limits phone length.
    string? PhoneNumber, // Carries customer phone number.
    [StringLength(500)] // Limits address length.
    string? Address); // Carries customer address.

public sealed record CustomerDetailDto( // Defines customer detail DTO.
    [Required] // Requires customer id.
    [StringLength(450)] // Limits customer id length.
    string Id, // Carries customer id.
    [Required] // Requires full name.
    [StringLength(256)] // Limits full name length.
    string FullName, // Carries customer full name.
    [Required] // Requires email.
    [EmailAddress] // Validates email format.
    [StringLength(256)] // Limits email length.
    string Email, // Carries customer email.
    [StringLength(32)] // Limits phone length.
    string? PhoneNumber, // Carries customer phone number.
    [StringLength(500)] // Limits address length.
    string? Address, // Carries customer address.
    [Required] // Requires vehicle list.
    List<VehicleDto> Vehicles); // Carries customer vehicles.

public sealed class UpdateCustomerDto // Defines customer update DTO.
{
    [Required]
    [StringLength(256)]
    public string? FullName { get; init; }

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string? Email { get; init; }

    [StringLength(32)]
    public string? PhoneNumber { get; init; }

    [Required]
    [StringLength(500)]
    public string? Address { get; init; }
}

public sealed class CreateCustomerHistoryDto // Defines customer history create DTO.
{
    [StringLength(450)]
    public string? CustomerId { get; init; }

    public Guid? VehicleId { get; init; }

    [Required]
    [StringLength(20)]
    public string? HistoryType { get; init; }

    [Required]
    [StringLength(1000)]
    public string? Description { get; init; }

    public decimal Amount { get; init; }

    [Required]
    [StringLength(20)]
    public string? PaymentStatus { get; init; }

    public DateTime? HistoryDate { get; init; }
}

public sealed record CustomerHistoryDto( // Defines customer history DTO.
    Guid Id, // Carries history id.
    string CustomerId, // Carries customer id.
    Guid? VehicleId, // Carries vehicle id.
    string HistoryType, // Carries history type.
    string Description, // Carries description.
    decimal Amount, // Carries amount.
    string PaymentStatus, // Carries payment status.
    DateTime HistoryDate); // Carries history date.

public sealed record CustomerReportDto( // Defines customer report DTO.
    string CustomerId, // Carries customer id.
    string FullName, // Carries full name.
    string Email, // Carries email.
    string? PhoneNumber, // Carries phone number.
    int TotalHistoryCount, // Carries history count.
    decimal TotalAmount, // Carries total amount.
    decimal PendingCreditAmount, // Carries pending credit.
    decimal OverdueCreditAmount, // Carries overdue credit.
    DateTime? LatestActivityDate); // Carries latest activity date.
