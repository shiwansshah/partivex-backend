using Partivex.Domain.Entities;

namespace Partivex.Application.Interfaces;

public interface IReviewRepository
{
    Task<IReadOnlyList<Review>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);

    Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsForAppointmentAsync(
        string customerId,
        Guid appointmentId,
        Guid? excludingReviewId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(Review review, CancellationToken cancellationToken = default);

    void Remove(Review review);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
