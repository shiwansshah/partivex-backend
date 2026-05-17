using Partivex.Domain.Entities;

namespace Partivex.Application.Interfaces;

public interface IPartRequestRepository
{
    Task<IReadOnlyList<PartRequest>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);

    Task<PartRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(PartRequest partRequest, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
