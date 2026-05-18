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

    public async Task<IReadOnlyList<Part>> GetAllAsync(
        string? searchTerm,
        string? sortBy,
        string? sortDirection,
        int pageNumber,
        int pageSize)
    {
        var query = _dbContext.Parts
            .AsNoTracking()
            .Where(part => part.IsActive);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedSearchTerm = searchTerm.Trim().ToLower();
            query = query.Where(part =>
                part.Name.ToLower().Contains(normalizedSearchTerm) ||
                part.PartCode.ToLower().Contains(normalizedSearchTerm) ||
                part.Category.ToLower().Contains(normalizedSearchTerm) ||
                part.CompatibleVehicle.ToLower().Contains(normalizedSearchTerm));
        }

        var normalizedSortBy = sortBy?.Trim().ToLowerInvariant();
        var isDescending = string.Equals(sortDirection?.Trim(), "desc", StringComparison.OrdinalIgnoreCase);

        query = normalizedSortBy switch
        {
            "partcode" => isDescending ? query.OrderByDescending(part => part.PartCode) : query.OrderBy(part => part.PartCode),
            "price" or "unitprice" => isDescending ? query.OrderByDescending(part => part.UnitPrice) : query.OrderBy(part => part.UnitPrice),
            "stock" or "currentstock" => isDescending ? query.OrderByDescending(part => part.CurrentStock) : query.OrderBy(part => part.CurrentStock),
            "createdat" => isDescending ? query.OrderByDescending(part => part.CreatedAt) : query.OrderBy(part => part.CreatedAt),
            _ => isDescending ? query.OrderByDescending(part => part.Name) : query.OrderBy(part => part.Name)
        };

        var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
        var safePageSize = pageSize < 1 ? 20 : pageSize > 100 ? 100 : pageSize;

        return await query
            .Skip((safePageNumber - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync();
    }

    public Task<Part?> GetByIdAsync(int id)
    {
        return _dbContext.Parts.FirstOrDefaultAsync(part => part.Id == id);
    }

    public Task<Part?> GetActiveByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Parts.FirstOrDefaultAsync(part => part.Id == id && part.IsActive, cancellationToken);
    }

    public Task<bool> PartCodeExistsAsync(string partCode, int? excludedPartId = null)
    {
        var normalizedCode = partCode.Trim().ToUpper();

        return _dbContext.Parts.AnyAsync(part =>
            part.IsActive &&
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
