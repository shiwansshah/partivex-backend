using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Domain.Enums;

namespace Partivex.Application.Services;

public sealed class CustomerAppointmentService : ICustomerAppointmentService
{
    private static readonly IReadOnlyList<ServiceOptionDto> ServiceOptions =
    [
        new("General Inspection", "Routine vehicle health and safety inspection."),
        new("Oil Change", "Engine oil and filter replacement."),
        new("Brake Service", "Brake pads, discs, fluid, and safety checks."),
        new("Battery and Electrical", "Battery, alternator, starter, and wiring diagnosis."),
        new("Engine Diagnostics", "Computerized diagnostics and problem investigation."),
        new("Tire and Wheel Service", "Tire replacement, rotation, balancing, and alignment."),
        new("AC and Heating", "Air conditioning and cabin heating service."),
        new("Other Repair", "Describe the issue and the service team will review it.")
    ];

    private readonly ICustomerAppointmentRepository _appointmentRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public CustomerAppointmentService(
        ICustomerAppointmentRepository appointmentRepository,
        IVehicleRepository vehicleRepository)
    {
        _appointmentRepository = appointmentRepository;
        _vehicleRepository = vehicleRepository;
    }

    public IReadOnlyList<ServiceOptionDto> GetServiceOptions()
    {
        return ServiceOptions;
    }

    public async Task<IReadOnlyList<AppointmentListDto>> GetAppointmentsAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var appointments = await _appointmentRepository.GetByCustomerIdAsync(customerId, cancellationToken);

        return appointments.Select(MapList).ToArray();
    }

    public async Task<CustomerPortalResult<AppointmentDetailDto>> GetAppointmentAsync(
        Guid id,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);

        if (appointment is null || appointment.CustomerId != customerId)
        {
            return CustomerPortalResult<AppointmentDetailDto>.NotFound("Appointment not found.");
        }

        return CustomerPortalResult<AppointmentDetailDto>.Success(MapDetail(appointment));
    }

    public async Task<CustomerPortalResult<AppointmentDetailDto>> CreateAppointmentAsync(
        string customerId,
        CreateAppointmentDto dto,
        CancellationToken cancellationToken = default)
    {
        var errors = ValidateCreate(dto);

        Vehicle? vehicle = null;
        if (dto.VehicleId.HasValue && dto.VehicleId.Value != Guid.Empty)
        {
            vehicle = await _vehicleRepository.GetByIdAsync(dto.VehicleId.Value);
            if (vehicle is null || vehicle.CustomerId != customerId)
            {
                errors.Add(new CustomerPortalError(nameof(dto.VehicleId), "Select one of your registered vehicles."));
            }
        }

        DateTimeOffset preferredAt = default;
        if (dto.PreferredDate.HasValue && dto.PreferredTime.HasValue)
        {
            preferredAt = BuildPreferredAt(dto.PreferredDate.Value, dto.PreferredTime.Value);
            if (preferredAt <= DateTimeOffset.Now)
            {
                errors.Add(new CustomerPortalError(nameof(dto.PreferredDate), "Appointment date and time must be in the future."));
            }
        }

        if (errors.Count > 0)
        {
            return CustomerPortalResult<AppointmentDetailDto>.Failed(errors, "Appointment could not be created.");
        }

        var now = DateTimeOffset.UtcNow;
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            VehicleId = dto.VehicleId!.Value,
            ServiceType = NormalizeRequired(dto.ServiceType),
            PreferredAt = preferredAt,
            Notes = NormalizeOptional(dto.Notes),
            Status = AppointmentStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _appointmentRepository.AddAsync(appointment, cancellationToken);
        await _appointmentRepository.SaveChangesAsync(cancellationToken);

        var saved = await _appointmentRepository.GetByIdAsync(appointment.Id, cancellationToken);
        return CustomerPortalResult<AppointmentDetailDto>.Success(MapDetail(saved!));
    }

    public async Task<CustomerPortalResult<AppointmentDetailDto>> CancelAppointmentAsync(
        Guid id,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);

        if (appointment is null || appointment.CustomerId != customerId)
        {
            return CustomerPortalResult<AppointmentDetailDto>.NotFound("Appointment not found.");
        }

        if (appointment.Status is not (AppointmentStatus.Pending or AppointmentStatus.Confirmed))
        {
            return CustomerPortalResult<AppointmentDetailDto>.Failed(
            [
                new CustomerPortalError(nameof(appointment.Status), "Only pending or confirmed appointments can be cancelled.")
            ],
            "Appointment could not be cancelled.");
        }

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.UpdatedAt = DateTimeOffset.UtcNow;

        await _appointmentRepository.SaveChangesAsync(cancellationToken);

        return CustomerPortalResult<AppointmentDetailDto>.Success(MapDetail(appointment));
    }

    private static List<CustomerPortalError> ValidateCreate(CreateAppointmentDto dto)
    {
        var errors = new List<CustomerPortalError>();

        if (!dto.VehicleId.HasValue || dto.VehicleId.Value == Guid.Empty)
        {
            errors.Add(new CustomerPortalError(nameof(dto.VehicleId), "Vehicle is required."));
        }

        if (string.IsNullOrWhiteSpace(dto.ServiceType))
        {
            errors.Add(new CustomerPortalError(nameof(dto.ServiceType), "Service type is required."));
        }

        if (!dto.PreferredDate.HasValue)
        {
            errors.Add(new CustomerPortalError(nameof(dto.PreferredDate), "Preferred date is required."));
        }

        if (!dto.PreferredTime.HasValue)
        {
            errors.Add(new CustomerPortalError(nameof(dto.PreferredTime), "Preferred time is required."));
        }

        return errors;
    }

    private static DateTimeOffset BuildPreferredAt(DateOnly date, TimeOnly time)
    {
        var localDateTime = DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Unspecified);
        return new DateTimeOffset(localDateTime, TimeZoneInfo.Local.GetUtcOffset(localDateTime));
    }

    private static AppointmentListDto MapList(Appointment appointment)
    {
        return new AppointmentListDto(
            appointment.Id,
            appointment.VehicleId,
            appointment.Vehicle?.Name ?? string.Empty,
            appointment.Vehicle?.Number ?? string.Empty,
            appointment.ServiceType,
            DateOnly.FromDateTime(appointment.PreferredAt.DateTime),
            TimeOnly.FromDateTime(appointment.PreferredAt.DateTime),
            appointment.Status.ToString(),
            appointment.CreatedAt);
    }

    private static AppointmentDetailDto MapDetail(Appointment appointment)
    {
        return new AppointmentDetailDto(
            appointment.Id,
            appointment.VehicleId,
            appointment.Vehicle?.Name ?? string.Empty,
            appointment.Vehicle?.Number ?? string.Empty,
            appointment.ServiceType,
            DateOnly.FromDateTime(appointment.PreferredAt.DateTime),
            TimeOnly.FromDateTime(appointment.PreferredAt.DateTime),
            appointment.Notes,
            appointment.Status.ToString(),
            appointment.CreatedAt,
            appointment.UpdatedAt);
    }

    private static string NormalizeRequired(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
