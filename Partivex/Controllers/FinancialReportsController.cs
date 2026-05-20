using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/financial-reports")]
[Authorize(Roles = ApplicationRoles.Admin)]
public sealed class FinancialReportsController : ControllerBase
{
    private readonly IFinancialReportService _financialReportService;

    public FinancialReportsController(IFinancialReportService financialReportService)
    {
        _financialReportService = financialReportService;
    }

    [HttpGet]
    public async Task<ActionResult<FinancialReportDto>> GetReport(
        [FromQuery] string period = "monthly",
        [FromQuery] string? referenceDate = null,
        CancellationToken cancellationToken = default)
    {
        var reportDate = ParseReferenceDate(referenceDate);

        try
        {
            var report = await _financialReportService.GetReportAsync(period, reportDate, cancellationToken);

            return Ok(report);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return StatusCode(499);
        }
    }

    private static DateOnly ParseReferenceDate(string? referenceDate)
    {
        if (DateOnly.TryParseExact(
                referenceDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate))
        {
            return parsedDate;
        }

        return DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
