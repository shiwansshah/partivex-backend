using Partivex.Application.DTOs;
using Partivex.Domain.Entities;

namespace Partivex.Application.Interfaces;

public interface IActivityLogRepository
{
    Task AddAsync(ActivityLog log, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ActivityLog>> GetAsync(
        ActivityLogFilterDto filter,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
