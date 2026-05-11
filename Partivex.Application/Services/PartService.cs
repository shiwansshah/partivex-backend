using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Application.Services;

public sealed class PartService : IPartService
{
    public Task<IReadOnlyList<PartResponseDto>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<PartResponseDto>>(Array.Empty<PartResponseDto>());
    }

    public Task<PartResponseDto?> GetByIdAsync(int id)
    {
        return Task.FromResult<PartResponseDto?>(null);
    }

    public Task<PartResponseDto> CreateAsync(CreatePartDto partDto)
    {
        throw new NotImplementedException("Part creation will be implemented in the next step.");
    }

    public Task<PartResponseDto?> UpdateAsync(int id, UpdatePartDto partDto)
    {
        throw new NotImplementedException("Part update will be implemented in the next step.");
    }

    public Task<bool> DeleteAsync(int id)
    {
        throw new NotImplementedException("Part deletion will be implemented in the next step.");
    }
}
