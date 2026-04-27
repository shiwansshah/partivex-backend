using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface IVendorService
{
    Task<IReadOnlyList<VendorResponseDto>> GetAllAsync();

    Task<VendorResponseDto?> GetByIdAsync(int id);

    Task<VendorResponseDto> CreateAsync(CreateVendorDto vendorDto);

    Task<VendorResponseDto?> UpdateAsync(int id, UpdateVendorDto vendorDto);

    Task<bool> DeleteAsync(int id);
}
