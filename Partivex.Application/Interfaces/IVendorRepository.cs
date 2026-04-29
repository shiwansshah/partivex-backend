using Partivex.Domain.Entities;

namespace Partivex.Application.Interfaces;

public interface IVendorRepository
{
    Task<IReadOnlyList<Vendor>> GetAllAsync();

    Task<Vendor?> GetByIdAsync(int id);

    Task<bool> EmailExistsAsync(string email, int? excludedVendorId = null);

    Task<Vendor> AddAsync(Vendor vendor);

    Task UpdateAsync(Vendor vendor);

    Task DeleteAsync(Vendor vendor);
}
