using Partivex.Domain.Entities;

namespace Partivex.Application.Interfaces;

public interface INotificationRepository
{
    Task<IReadOnlyCollection<Notification>> GetForUserAsync(string role, string? userId, CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(string role, string? userId, CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
