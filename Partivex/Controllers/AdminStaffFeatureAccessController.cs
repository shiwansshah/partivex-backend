using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = ApplicationRoles.Admin)]
public sealed class AdminStaffFeatureAccessController : ControllerBase
{
    private readonly IStaffFeatureAccessService _staffFeatureAccessService;

    public AdminStaffFeatureAccessController(IStaffFeatureAccessService staffFeatureAccessService)
    {
        _staffFeatureAccessService = staffFeatureAccessService;
    }

    [HttpGet("{userId}/feature-access")]
    public async Task<ActionResult<StaffFeatureAccessSummaryDto>> GetFeatureAccess(
        string userId,
        CancellationToken cancellationToken)
    {
        var featureAccess = await _staffFeatureAccessService.GetFeatureAccessAsync(userId, cancellationToken);

        return Ok(featureAccess);
    }

    [HttpPut("{userId}/feature-access")]
    public async Task<ActionResult<StaffFeatureAccessSummaryDto>> UpdateFeatureAccess(
        string userId,
        [FromBody] UpdateStaffFeatureAccessDto dto,
        CancellationToken cancellationToken)
    {
        var featureAccess = await _staffFeatureAccessService.UpdateFeatureAccessAsync(
            userId,
            dto,
            cancellationToken);

        return Ok(featureAccess);
    }
}
