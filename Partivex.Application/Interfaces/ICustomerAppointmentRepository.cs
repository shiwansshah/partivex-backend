using Partivex.Domain.Entities;

namespace Partivex.Application.Interfaces;

public interface ICustomerAppointmentRepository
{
    Task<IReadOnlyList<Appointment>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);

    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
