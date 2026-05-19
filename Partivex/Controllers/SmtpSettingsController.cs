using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/smtp-settings")]
[Authorize(Roles = ApplicationRoles.Admin)]
public sealed class SmtpSettingsController : ControllerBase
{
    private readonly ISmtpSettingService _smtpSettingService;

    public SmtpSettingsController(ISmtpSettingService smtpSettingService)
    {
        _smtpSettingService = smtpSettingService;
    }

    [HttpGet]
    public async Task<ActionResult<SmtpSettingDto>> Get(CancellationToken cancellationToken)
    {
        return Ok(await _smtpSettingService.GetAsync(cancellationToken));
    }

    [HttpPut]
    public async Task<ActionResult<SmtpSettingDto>> Update(UpdateSmtpSettingDto dto, CancellationToken cancellationToken)
    {
        return Ok(await _smtpSettingService.UpdateAsync(dto, cancellationToken));
    }
}
