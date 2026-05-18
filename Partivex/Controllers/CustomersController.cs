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
    private readonly ICustomerHistoryService _customerHistoryService;

    public CustomersController(ICustomerService customerService, ICustomerHistoryService customerHistoryService)
    {
        _customerService = customerService;
        _customerHistoryService = customerHistoryService;
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

    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> SearchCustomers([FromQuery] string term)
    {
        try
        {
            var customers = await _customerService.SearchAsync(term);

            return Ok(customers);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ApiErrorResponse(exception.Message));
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CustomerDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CustomerDetailDto>> UpdateCustomer([FromRoute] string id, [FromBody] UpdateCustomerDto dto)
    {
        try
        {
            var customer = await _customerService.UpdateAsync(id, dto);

            return Ok(customer);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ApiErrorResponse(exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ApiErrorResponse(exception.Message));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new ApiErrorResponse(exception.Message));
        }
    }

    [HttpGet("{id}/history")]
    [ProducesResponseType(typeof(IEnumerable<CustomerHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<CustomerHistoryDto>>> GetCustomerHistory([FromRoute] string id)
    {
        try
        {
            var history = await _customerHistoryService.GetHistoryAsync(id);

            return Ok(history);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new ApiErrorResponse(exception.Message));
        }
    }

    [HttpPost("{id}/history")]
    [ProducesResponseType(typeof(CustomerHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CustomerHistoryDto>> CreateCustomerHistory([FromRoute] string id, [FromBody] CreateCustomerHistoryDto dto)
    {
        try
        {
            var history = await _customerHistoryService.CreateHistoryAsync(id, dto);

            return Ok(history);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ApiErrorResponse(exception.Message));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new ApiErrorResponse(exception.Message));
        }
    }
}
