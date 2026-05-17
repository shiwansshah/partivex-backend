using System.ComponentModel.DataAnnotations;

namespace Partivex.Application.DTOs;

public sealed record CustomerPortalError(string Code, string Description);

public sealed class CustomerPortalResult<T>
{
    private CustomerPortalResult(T? value, IReadOnlyCollection<CustomerPortalError> errors, bool isNotFound, string? message)
    {
        Value = value;
        Errors = errors;
        IsNotFound = isNotFound;
        Message = message;
    }

    public T? Value { get; }

    public IReadOnlyCollection<CustomerPortalError> Errors { get; }

    public bool IsNotFound { get; }

    public string? Message { get; }

    public bool Succeeded => !IsNotFound && Errors.Count == 0;

    public static CustomerPortalResult<T> Success(T value) => new(value, [], false, null);

    public static CustomerPortalResult<T> Failed(IReadOnlyCollection<CustomerPortalError> errors, string? message = null) =>
        new(default, errors, false, message);

    public static CustomerPortalResult<T> NotFound(string message) => new(default, [], true, message);
}

public sealed record ServiceOptionDto(string Name, string Description);

public sealed class CreateAppointmentDto
{
    [Required]
    public Guid? VehicleId { get; init; }

    [Required]
    [MaxLength(100)]
    public string? ServiceType { get; init; }

    [Required]
    public DateOnly? PreferredDate { get; init; }

    [Required]
    public TimeOnly? PreferredTime { get; init; }

    [MaxLength(1000)]
    public string? Notes { get; init; }
}

public sealed record AppointmentListDto(
    Guid Id,
    Guid VehicleId,
    string VehicleName,
    string VehicleNumber,
    string ServiceType,
    DateOnly PreferredDate,
    TimeOnly PreferredTime,
    string Status,
    DateTimeOffset CreatedAt);

public sealed record AppointmentDetailDto(
    Guid Id,
    Guid VehicleId,
    string VehicleName,
    string VehicleNumber,
    string ServiceType,
    DateOnly PreferredDate,
    TimeOnly PreferredTime,
    string? Notes,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AppointmentResponseDto(string Message, AppointmentDetailDto Appointment);

public sealed class CreatePartRequestDto
{
    [Required]
    [MaxLength(120)]
    public string? PartName { get; init; }

    public Guid? VehicleId { get; init; }

    [MaxLength(200)]
    public string? BrandModelSpecification { get; init; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }

    [MaxLength(1000)]
    public string? Reason { get; init; }
}

public sealed record PartRequestListDto(
    Guid Id,
    Guid? VehicleId,
    string? VehicleName,
    string? VehicleNumber,
    string PartName,
    string? BrandModelSpecification,
    int Quantity,
    string Status,
    DateTimeOffset CreatedAt);

public sealed record PartRequestDetailDto(
    Guid Id,
    Guid? VehicleId,
    string? VehicleName,
    string? VehicleNumber,
    string PartName,
    string? BrandModelSpecification,
    int Quantity,
    string? Reason,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record PartRequestResponseDto(string Message, PartRequestDetailDto PartRequest);

public sealed class CreateReviewDto
{
    public Guid? AppointmentId { get; init; }

    [Range(1, 5)]
    public int Rating { get; init; }

    [Required]
    [MaxLength(2000)]
    public string? Comment { get; init; }
}

public sealed class UpdateReviewDto
{
    [Range(1, 5)]
    public int Rating { get; init; }

    [Required]
    [MaxLength(2000)]
    public string? Comment { get; init; }
}

public sealed record ReviewListDto(
    Guid Id,
    Guid? AppointmentId,
    string Category,
    string? ServiceType,
    DateOnly? AppointmentDate,
    string? AppointmentStatus,
    int Rating,
    string Comment,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ReviewDetailDto(
    Guid Id,
    Guid? AppointmentId,
    string Category,
    string? ServiceType,
    DateOnly? AppointmentDate,
    string? AppointmentStatus,
    int Rating,
    string Comment,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ReviewResponseDto(string Message, ReviewDetailDto Review);
