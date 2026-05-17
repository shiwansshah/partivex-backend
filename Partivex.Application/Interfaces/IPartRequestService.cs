using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface IPartRequestService
{
    Task<IReadOnlyList<PartRequestListDto>> GetPartRequestsAsync(string customerId, CancellationToken cancellationToken = default);

    Task<CustomerPortalResult<PartRequestDetailDto>> GetPartRequestAsync(
        Guid id,
        string customerId,
        CancellationToken cancellationToken = default);

    Task<CustomerPortalResult<PartRequestDetailDto>> CreatePartRequestAsync(
        string customerId,
        CreatePartRequestDto dto,
        CancellationToken cancellationToken = default);

    Task<CustomerPortalResult<PartRequestDetailDto>> CancelPartRequestAsync(
        Guid id,
        string customerId,
        CancellationToken cancellationToken = default);
}
