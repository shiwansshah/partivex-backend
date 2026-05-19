using Microsoft.EntityFrameworkCore;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Infrastructure.Data;

namespace Partivex.Infrastructure.Repositories;

public sealed class AppointmentInvoiceRepository : IAppointmentInvoiceRepository
{
    private readonly AppDbContext _dbContext;

    public AppointmentInvoiceRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AppointmentInvoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await BaseQuery()
            .AsNoTracking()
            .OrderByDescending(invoice => invoice.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AppointmentInvoice>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
    {
        return await BaseQuery()
            .AsNoTracking()
            .Where(invoice => invoice.CustomerId == customerId)
            .OrderByDescending(invoice => invoice.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AppointmentInvoice>> GetOverduePendingAsync(DateTimeOffset cutoff, CancellationToken cancellationToken = default)
    {
        return await BaseQuery()
            .Where(invoice => invoice.PaymentStatus == "Pending" && invoice.InvoiceDate <= cutoff)
            .OrderBy(invoice => invoice.InvoiceDate)
            .ToArrayAsync(cancellationToken);
    }

    public Task<AppointmentInvoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return BaseQuery().FirstOrDefaultAsync(invoice => invoice.Id == id, cancellationToken);
    }

    public Task<bool> InvoiceNumberExistsAsync(string invoiceNumber, CancellationToken cancellationToken = default)
    {
        return _dbContext.AppointmentInvoices.AnyAsync(invoice => invoice.InvoiceNumber == invoiceNumber, cancellationToken);
    }

    public Task<bool> AppointmentHasInvoiceAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        return _dbContext.AppointmentInvoices.AnyAsync(invoice => invoice.AppointmentId == appointmentId, cancellationToken);
    }

    public async Task AddAsync(AppointmentInvoice invoice, CancellationToken cancellationToken = default)
    {
        await _dbContext.AppointmentInvoices.AddAsync(invoice, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<AppointmentInvoice> BaseQuery()
    {
        return _dbContext.AppointmentInvoices
            .Include(invoice => invoice.Customer)
            .Include(invoice => invoice.Appointment)
                .ThenInclude(appointment => appointment.Vehicle);
    }
}
