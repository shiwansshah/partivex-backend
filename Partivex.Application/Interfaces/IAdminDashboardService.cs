using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface IAdminDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
