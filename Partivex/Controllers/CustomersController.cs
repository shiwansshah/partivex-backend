using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Roles = ApplicationRoles.AdminAndStaff)]
public sealed class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomers()
    {
        var customers = await _customerService.GetAllCustomersAsync();

        return Ok(customers);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CustomerDetailDto>> GetCustomerById([FromRoute] string id)
    {
        try
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);

            return Ok(customer);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new ApiErrorResponse(exception.Message));
        }
    }

    [HttpGet("{id}/history")]
    [ProducesResponseType(typeof(CustomerHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CustomerHistoryDto>> GetCustomerHistory([FromRoute] string id)
    {
        try
        {
            var history = await _customerService.GetCustomerHistoryAsync(id);

            return Ok(history);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new ApiErrorResponse(exception.Message));
        }
    }
}
