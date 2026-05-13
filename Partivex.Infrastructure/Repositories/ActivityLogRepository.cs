using Microsoft.EntityFrameworkCore;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Infrastructure.Data;

namespace Partivex.Infrastructure.Repositories;

public sealed class ActivityLogRepository : IActivityLogRepository
{
    private readonly AppDbContext _dbContext;

    public ActivityLogRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ActivityLog log, CancellationToken cancellationToken = default)
    {
        await _dbContext.ActivityLogs.AddAsync(log, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ActivityLog>> GetAsync(
        ActivityLogFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ActivityLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.User))
        {
            var user = filter.User.Trim().ToLower();
            query = query.Where(log =>
                log.UserName.ToLower().Contains(user) ||
                log.UserId.ToLower().Contains(user));
        }

        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            var action = filter.Action.Trim().ToLower();
            query = query.Where(log => log.Action.ToLower().Contains(action));
        }

        if (filter.From.HasValue)
        {
            query = query.Where(log => log.CreatedAt >= filter.From.Value);
        }

        if (filter.To.HasValue)
        {
            query = query.Where(log => log.CreatedAt <= filter.To.Value);
        }

        return await query
            .OrderByDescending(log => log.CreatedAt)
            .Take(100)
            .ToArrayAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
