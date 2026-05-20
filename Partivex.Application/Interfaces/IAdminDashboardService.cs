using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminDashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
