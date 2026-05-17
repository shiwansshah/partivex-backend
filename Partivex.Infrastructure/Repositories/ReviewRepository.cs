using Microsoft.EntityFrameworkCore;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Infrastructure.Data;

namespace Partivex.Infrastructure.Repositories;

public sealed class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _dbContext;

    public ReviewRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Review>> GetByCustomerIdAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reviews
            .AsNoTracking()
            .Include(review => review.Appointment)
            .Where(review => review.CustomerId == customerId)
            .OrderByDescending(review => review.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Reviews
            .Include(review => review.Appointment)
            .FirstOrDefaultAsync(review => review.Id == id, cancellationToken);
    }

    public Task<bool> ExistsForAppointmentAsync(
        string customerId,
        Guid appointmentId,
        Guid? excludingReviewId = null,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Reviews.AnyAsync(
            review =>
                review.CustomerId == customerId &&
                review.AppointmentId == appointmentId &&
                (!excludingReviewId.HasValue || review.Id != excludingReviewId.Value),
            cancellationToken);
    }

    public async Task AddAsync(Review review, CancellationToken cancellationToken = default)
    {
        await _dbContext.Reviews.AddAsync(review, cancellationToken);
    }

    public void Remove(Review review)
    {
        _dbContext.Reviews.Remove(review);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
