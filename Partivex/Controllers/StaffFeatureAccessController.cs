using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/staff/me/feature-access")]
[Authorize(Roles = ApplicationRoles.Staff)]
public sealed class StaffFeatureAccessController : ControllerBase
{
    private readonly IStaffFeatureAccessService _staffFeatureAccessService;

    public StaffFeatureAccessController(IStaffFeatureAccessService staffFeatureAccessService)
    {
        _staffFeatureAccessService = staffFeatureAccessService;
    }

    [HttpGet]
    public async Task<ActionResult<StaffFeatureAccessSummaryDto>> GetMyFeatureAccess(
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var featureAccess = await _staffFeatureAccessService.GetFeatureAccessAsync(userId, cancellationToken);

        return Ok(featureAccess);
    }
}
