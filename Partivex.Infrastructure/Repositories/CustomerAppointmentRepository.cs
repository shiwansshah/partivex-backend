using Microsoft.EntityFrameworkCore;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Infrastructure.Data;

namespace Partivex.Infrastructure.Repositories;

public sealed class CustomerAppointmentRepository : ICustomerAppointmentRepository
{
    private readonly AppDbContext _dbContext;

    public CustomerAppointmentRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Appointment>> GetByCustomerIdAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Appointments
            .AsNoTracking()
            .Include(appointment => appointment.Vehicle)
            .Where(appointment => appointment.CustomerId == customerId)
            .OrderByDescending(appointment => appointment.PreferredAt)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Appointments
            .Include(appointment => appointment.Vehicle)
            .FirstOrDefaultAsync(appointment => appointment.Id == id, cancellationToken);
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        await _dbContext.Appointments.AddAsync(appointment, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
