using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;

namespace Partivex.Application.Services;

public sealed class ActivityLogService : IActivityLogService
{
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public ActivityLogService(
        IActivityLogRepository activityLogRepository,
        ICurrentUserContext currentUserContext)
    {
        _activityLogRepository = activityLogRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task LogAsync(CreateActivityLogCommand command, CancellationToken cancellationToken = default)
    {
        var log = new ActivityLog
        {
            UserId = _currentUserContext.UserId,
            UserName = _currentUserContext.UserName,
            Role = _currentUserContext.Role,
            Action = command.Action.Trim(),
            EntityName = command.EntityName.Trim(),
            EntityId = command.EntityId.Trim(),
            Description = command.Description.Trim(),
            IpAddress = _currentUserContext.IpAddress,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _activityLogRepository.AddAsync(log, cancellationToken);
        await _activityLogRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ActivityLogDto>> GetLogsAsync(
        ActivityLogFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(
            new CreateActivityLogCommand(
                "ViewActivityLogs",
                "ActivityLog",
                string.Empty,
                "Admin viewed the activity logs page."),
            cancellationToken);

        var logs = await _activityLogRepository.GetAsync(filter, cancellationToken);

        return logs.Select(MapToDto).ToArray();
    }

    private static ActivityLogDto MapToDto(ActivityLog log)
    {
        return new ActivityLogDto(
            log.Id,
            log.UserId,
            log.UserName,
            log.Role,
            log.Action,
            log.EntityName,
            log.EntityId,
            log.Description,
            log.IpAddress,
            log.CreatedAt);
    }
}
