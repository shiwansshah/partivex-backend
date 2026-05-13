using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/admin/activity-logs")]
[Authorize(Roles = ApplicationRoles.Admin)]
public sealed class AdminActivityLogsController : ControllerBase
{
    private readonly IActivityLogService _activityLogService;

    public AdminActivityLogsController(IActivityLogService activityLogService)
    {
        _activityLogService = activityLogService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ActivityLogDto>>> GetLogs(
        [FromQuery] string? user,
        [FromQuery] string? action,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        var logs = await _activityLogService.GetLogsAsync(
            new ActivityLogFilterDto(user, action, from, to),
            cancellationToken);

        return Ok(logs);
    }
}
