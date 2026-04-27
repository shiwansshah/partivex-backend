using Microsoft.EntityFrameworkCore;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Infrastructure.Data;

namespace Partivex.Infrastructure.Repositories;

public sealed class VendorRepository : IVendorRepository
{
    private readonly AppDbContext _dbContext;

    public VendorRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Vendor>> GetAllAsync()
    {
        return await _dbContext.Vendors
            .AsNoTracking()
            .OrderBy(vendor => vendor.Name)
            .ToListAsync();
    }

    public Task<Vendor?> GetByIdAsync(int id)
    {
        return _dbContext.Vendors.FirstOrDefaultAsync(vendor => vendor.Id == id);
    }

    public async Task<Vendor> AddAsync(Vendor vendor)
    {
        _dbContext.Vendors.Add(vendor);
        await _dbContext.SaveChangesAsync();
        return vendor;
    }

    public async Task UpdateAsync(Vendor vendor)
    {
        _dbContext.Vendors.Update(vendor);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Vendor vendor)
    {
        _dbContext.Vendors.Remove(vendor);
        await _dbContext.SaveChangesAsync();
    }
}
