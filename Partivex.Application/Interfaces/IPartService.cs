using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface IPartService
{
    Task<IReadOnlyList<PartResponseDto>> GetAllAsync(
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = "name",
        string? sortDirection = "asc");

    Task<PartResponseDto?> GetByIdAsync(int id);

    Task<PartResponseDto> CreateAsync(CreatePartDto partDto);

    Task<PartResponseDto?> UpdateAsync(int id, UpdatePartDto partDto);

    Task<bool> DeleteAsync(int id);
}
