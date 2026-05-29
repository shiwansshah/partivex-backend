using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/payments/esewa")]
public sealed class EsewaPaymentsController : ControllerBase
{
    private readonly IEsewaPaymentService _esewaPaymentService;

    public EsewaPaymentsController(IEsewaPaymentService esewaPaymentService)
    {
        _esewaPaymentService = esewaPaymentService;
    }

    [HttpGet("success")]
    public async Task<IActionResult> Success([FromQuery] string? data, CancellationToken cancellationToken)
    {
        var result = string.IsNullOrWhiteSpace(data)
            ? await _esewaPaymentService.FailPaymentAsync(null, cancellationToken)
            : await _esewaPaymentService.CompletePaymentAsync(data, cancellationToken);
        return Redirect(result.RedirectUrl);
    }

    [HttpGet("failure")]
    public async Task<IActionResult> Failure([FromQuery] string? data, CancellationToken cancellationToken)
    {
        var result = await _esewaPaymentService.FailPaymentAsync(data, cancellationToken);
        return Redirect(result.RedirectUrl);
    }
}
