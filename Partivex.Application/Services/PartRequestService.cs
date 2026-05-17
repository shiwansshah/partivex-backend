using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Domain.Enums;

namespace Partivex.Application.Services;

public sealed class PartRequestService : IPartRequestService
{
    private readonly IPartRequestRepository _partRequestRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public PartRequestService(IPartRequestRepository partRequestRepository, IVehicleRepository vehicleRepository)
    {
        _partRequestRepository = partRequestRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<IReadOnlyList<PartRequestListDto>> GetPartRequestsAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var requests = await _partRequestRepository.GetByCustomerIdAsync(customerId, cancellationToken);

        return requests.Select(MapList).ToArray();
    }

    public async Task<CustomerPortalResult<PartRequestDetailDto>> GetPartRequestAsync(
        Guid id,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var request = await _partRequestRepository.GetByIdAsync(id, cancellationToken);

        if (request is null || request.CustomerId != customerId)
        {
            return CustomerPortalResult<PartRequestDetailDto>.NotFound("Part request not found.");
        }

        return CustomerPortalResult<PartRequestDetailDto>.Success(MapDetail(request));
    }

    public async Task<CustomerPortalResult<PartRequestDetailDto>> CreatePartRequestAsync(
        string customerId,
        CreatePartRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var errors = ValidateCreate(dto);

        if (dto.VehicleId.HasValue && dto.VehicleId.Value != Guid.Empty)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(dto.VehicleId.Value);
            if (vehicle is null || vehicle.CustomerId != customerId)
            {
                errors.Add(new CustomerPortalError(nameof(dto.VehicleId), "Select one of your registered vehicles."));
            }
        }

        if (errors.Count > 0)
        {
            return CustomerPortalResult<PartRequestDetailDto>.Failed(errors, "Part request could not be created.");
        }

        var now = DateTimeOffset.UtcNow;
        var request = new PartRequest
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            VehicleId = dto.VehicleId == Guid.Empty ? null : dto.VehicleId,
            PartName = NormalizeRequired(dto.PartName),
            BrandModelSpecification = NormalizeOptional(dto.BrandModelSpecification),
            Quantity = dto.Quantity,
            Reason = NormalizeOptional(dto.Reason),
            Status = PartRequestStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _partRequestRepository.AddAsync(request, cancellationToken);
        await _partRequestRepository.SaveChangesAsync(cancellationToken);

        var saved = await _partRequestRepository.GetByIdAsync(request.Id, cancellationToken);
        return CustomerPortalResult<PartRequestDetailDto>.Success(MapDetail(saved!));
    }

    public async Task<CustomerPortalResult<PartRequestDetailDto>> CancelPartRequestAsync(
        Guid id,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var request = await _partRequestRepository.GetByIdAsync(id, cancellationToken);

        if (request is null || request.CustomerId != customerId)
        {
            return CustomerPortalResult<PartRequestDetailDto>.NotFound("Part request not found.");
        }

        if (request.Status != PartRequestStatus.Pending)
        {
            return CustomerPortalResult<PartRequestDetailDto>.Failed(
            [
                new CustomerPortalError(nameof(request.Status), "Only pending part requests can be cancelled.")
            ],
            "Part request could not be cancelled.");
        }

        request.Status = PartRequestStatus.Cancelled;
        request.UpdatedAt = DateTimeOffset.UtcNow;

        await _partRequestRepository.SaveChangesAsync(cancellationToken);

        return CustomerPortalResult<PartRequestDetailDto>.Success(MapDetail(request));
    }

    private static List<CustomerPortalError> ValidateCreate(CreatePartRequestDto dto)
    {
        var errors = new List<CustomerPortalError>();

        if (string.IsNullOrWhiteSpace(dto.PartName))
        {
            errors.Add(new CustomerPortalError(nameof(dto.PartName), "Part name is required."));
        }

        if (dto.Quantity <= 0)
        {
            errors.Add(new CustomerPortalError(nameof(dto.Quantity), "Quantity must be greater than zero."));
        }

        return errors;
    }

    private static PartRequestListDto MapList(PartRequest request)
    {
        return new PartRequestListDto(
            request.Id,
            request.VehicleId,
            request.Vehicle?.Name,
            request.Vehicle?.Number,
            request.PartName,
            request.BrandModelSpecification,
            request.Quantity,
            request.Status.ToString(),
            request.CreatedAt);
    }

    private static PartRequestDetailDto MapDetail(PartRequest request)
    {
        return new PartRequestDetailDto(
            request.Id,
            request.VehicleId,
            request.Vehicle?.Name,
            request.Vehicle?.Number,
            request.PartName,
            request.BrandModelSpecification,
            request.Quantity,
            request.Reason,
            request.Status.ToString(),
            request.CreatedAt,
            request.UpdatedAt);
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
