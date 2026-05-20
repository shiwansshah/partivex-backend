using Microsoft.EntityFrameworkCore;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Infrastructure.Data;

namespace Partivex.Infrastructure.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _dbContext;

    public NotificationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Notification>> GetForUserAsync(string role, string? userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .AsNoTracking()
            .Where(n =>
                (n.TargetUserId == null && n.TargetRole.Contains(role)) ||
                (userId != null && n.TargetUserId == userId))
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(string role, string? userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .Where(n =>
                !n.IsRead &&
                ((n.TargetUserId == null && n.TargetRole.Contains(role)) ||
                 (userId != null && n.TargetUserId == userId)))
            .CountAsync(cancellationToken);
    }

    public async Task MarkAsReadAsync(int id, CancellationToken cancellationToken = default)
    {
        await _dbContext.Notifications
            .Where(n => n.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), cancellationToken);
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _dbContext.Notifications.AddAsync(notification, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
