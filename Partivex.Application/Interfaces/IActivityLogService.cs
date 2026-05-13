using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface IActivityLogService
{
    Task LogAsync(CreateActivityLogCommand command, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ActivityLogDto>> GetLogsAsync(
        ActivityLogFilterDto filter,
        CancellationToken cancellationToken = default);
}
