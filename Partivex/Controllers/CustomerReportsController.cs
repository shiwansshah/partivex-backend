using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/customer-reports")]
[Authorize(Roles = ApplicationRoles.AdminAndStaff)]
public sealed class CustomerReportsController : ControllerBase
{
    private readonly ICustomerReportService _customerReportService;

    public CustomerReportsController(ICustomerReportService customerReportService)
    {
        _customerReportService = customerReportService;
    }

    [HttpGet("regular")]
    [ProducesResponseType(typeof(IEnumerable<CustomerReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<CustomerReportDto>>> GetRegularCustomers()
    {
        var reports = await _customerReportService.GetRegularCustomersAsync();
        return Ok(reports);
    }

    [HttpGet("high-spenders")]
    [ProducesResponseType(typeof(IEnumerable<CustomerReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<CustomerReportDto>>> GetHighSpenders()
    {
        var reports = await _customerReportService.GetHighSpendersAsync();
        return Ok(reports);
    }

    [HttpGet("credit")]
    [ProducesResponseType(typeof(IEnumerable<CustomerReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<CustomerReportDto>>> GetCreditCustomers()
    {
        var reports = await _customerReportService.GetCreditCustomersAsync();
        return Ok(reports);
    }
}