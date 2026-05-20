using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;

namespace Partivex.Infrastructure.Services;

public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;

    public NotificationService(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task CreateAsync(string targetRole, string? targetUserId, string title, string message, string type, CancellationToken cancellationToken = default)
    {
        await _repository.AddAsync(new Notification
        {
            TargetRole = targetRole,
            TargetUserId = targetUserId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<NotificationDto>> GetForUserAsync(string role, string? userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _repository.GetForUserAsync(role, userId, cancellationToken);
        return notifications.Select(n => new NotificationDto(n.Id, n.Title, n.Message, n.Type, n.IsRead, n.CreatedAt)).ToArray();
    }

    public Task<int> GetUnreadCountAsync(string role, string? userId, CancellationToken cancellationToken = default)
    {
        return _repository.GetUnreadCountAsync(role, userId, cancellationToken);
    }

    public Task MarkAsReadAsync(int id, CancellationToken cancellationToken = default)
    {
        return _repository.MarkAsReadAsync(id, cancellationToken);
    }
}
