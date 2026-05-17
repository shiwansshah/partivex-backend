using Microsoft.EntityFrameworkCore;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Infrastructure.Data;

namespace Partivex.Infrastructure.Repositories;

public sealed class PartRequestRepository : IPartRequestRepository
{
    private readonly AppDbContext _dbContext;

    public PartRequestRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PartRequest>> GetByCustomerIdAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PartRequests
            .AsNoTracking()
            .Include(request => request.Vehicle)
            .Where(request => request.CustomerId == customerId)
            .OrderByDescending(request => request.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public Task<PartRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.PartRequests
            .Include(request => request.Vehicle)
            .FirstOrDefaultAsync(request => request.Id == id, cancellationToken);
    }

    public async Task AddAsync(PartRequest partRequest, CancellationToken cancellationToken = default)
    {
        await _dbContext.PartRequests.AddAsync(partRequest, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
