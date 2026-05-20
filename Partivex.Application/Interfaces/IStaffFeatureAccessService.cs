using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface IStaffFeatureAccessService
{
    Task<StaffFeatureAccessSummaryDto> GetFeatureAccessAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<StaffFeatureAccessSummaryDto> UpdateFeatureAccessAsync(
        string userId,
        UpdateStaffFeatureAccessDto dto,
        CancellationToken cancellationToken = default);
}
