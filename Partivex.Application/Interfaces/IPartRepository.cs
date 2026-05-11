using Partivex.Domain.Entities;

namespace Partivex.Application.Interfaces;

public interface IPartRepository
{
    Task<Part?> GetByIdAsync(int id);

    Task<bool> PartCodeExistsAsync(string partCode, int? excludedPartId = null);

    Task<Part> AddAsync(Part part);

    Task UpdateAsync(Part part);
}
