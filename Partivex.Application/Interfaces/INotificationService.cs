using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface INotificationService
{
    Task CreateAsync(string targetRole, string? targetUserId, string title, string message, string type, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<NotificationDto>> GetForUserAsync(string role, string? userId, CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(string role, string? userId, CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(int id, CancellationToken cancellationToken = default);
}
