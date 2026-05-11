using Microsoft.EntityFrameworkCore;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Infrastructure.Data;

namespace Partivex.Infrastructure.Repositories;

public sealed class PartRepository : IPartRepository
{
    private readonly AppDbContext _dbContext;

    public PartRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Part?> GetByIdAsync(int id)
    {
        return _dbContext.Parts.FirstOrDefaultAsync(part => part.Id == id);
    }

    public Task<bool> PartCodeExistsAsync(string partCode, int? excludedPartId = null)
    {
        var normalizedCode = partCode.Trim().ToUpper();

        return _dbContext.Parts.AnyAsync(part =>
            part.PartCode.ToUpper() == normalizedCode &&
            (!excludedPartId.HasValue || part.Id != excludedPartId.Value));
    }

    public async Task<Part> AddAsync(Part part)
    {
        _dbContext.Parts.Add(part);
        await _dbContext.SaveChangesAsync();
        return part;
    }

    public async Task UpdateAsync(Part part)
    {
        _dbContext.Parts.Update(part);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Part part)
    {
        part.IsActive = false;
        await _dbContext.SaveChangesAsync();
    }
}
