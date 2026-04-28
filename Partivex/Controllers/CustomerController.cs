using Microsoft.AspNetCore.Authorization; // Imports authorization attributes.
using Microsoft.AspNetCore.Mvc; // Imports MVC controller types.
using Partivex.Application.Constants; // Imports role constants.
using Partivex.Application.DTOs; // Imports customer DTOs.
using Partivex.Application.Interfaces; // Imports customer service contract.

namespace Partivex.Controllers; // Defines API namespace.

[ApiController] // Enables API conventions.
[Route("api/customers")] // Sets customer route.
[Authorize(Roles = ApplicationRoles.AdminAndStaff)] // Restricts customer reads.
public sealed class CustomerController : ControllerBase // Defines customer controller.
{
    private readonly ICustomerService _customerService; // Stores customer service.

    public CustomerController(ICustomerService customerService) // Defines constructor.
    {
        _customerService = customerService; // Assigns customer service.
    }

    [HttpGet] // Handles customer listing.
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)] // Documents success response.
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documents missing auth.
    [ProducesResponseType(StatusCodes.Status403Forbidden)] // Documents denied role.
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomers() // Lists customers.
    {
        var customers = await _customerService.GetAllCustomersAsync(); // Gets customers.

        return Ok(customers); // Returns customer list.
    }

    [HttpGet("{id}")] // Handles customer detail.
    [ProducesResponseType(typeof(CustomerDetailDto), StatusCodes.Status200OK)] // Documents success response.
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)] // Documents missing customer.
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documents missing auth.
    [ProducesResponseType(StatusCodes.Status403Forbidden)] // Documents denied role.
    public async Task<ActionResult<CustomerDetailDto>> GetCustomerById([FromRoute] string id) // Gets customer detail.
    {
        try // Handles service outcomes.
        {
            var customer = await _customerService.GetCustomerByIdAsync(id); // Gets customer DTO.

            return Ok(customer); // Returns customer detail.
        }
        catch (KeyNotFoundException exception) // Handles missing customer.
        {
            return NotFound(new ApiErrorResponse(exception.Message)); // Returns not found.
        }
    }

    [HttpGet("{id}/history")] // Handles customer history.
    [ProducesResponseType(typeof(CustomerHistoryDto), StatusCodes.Status200OK)] // Documents success response.
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)] // Documents missing customer.
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documents missing auth.
    [ProducesResponseType(StatusCodes.Status403Forbidden)] // Documents denied role.
    public async Task<ActionResult<CustomerHistoryDto>> GetCustomerHistory([FromRoute] string id) // Gets customer history.
    {
        try // Handles service outcomes.
        {
            var history = await _customerService.GetCustomerHistoryAsync(id); // Gets history DTO.

            return Ok(history); // Returns history data.
        }
        catch (KeyNotFoundException exception) // Handles missing customer.
        {
            return NotFound(new ApiErrorResponse(exception.Message)); // Returns not found.
        }
    }
}
