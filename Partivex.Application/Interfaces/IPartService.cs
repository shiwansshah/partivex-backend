using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface IPartService
{
    Task<IReadOnlyList<PartResponseDto>> GetAllAsync();

    Task<PartResponseDto?> GetByIdAsync(int id);

    Task<PartResponseDto> CreateAsync(CreatePartDto partDto);

    Task<PartResponseDto?> UpdateAsync(int id, UpdatePartDto partDto);

    Task<bool> DeleteAsync(int id);
}
