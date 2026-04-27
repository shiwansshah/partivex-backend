using Microsoft.AspNetCore.Mvc;

namespace Partivex.Controllers;

[ApiController]
[Route("api/vendor")]
public class VendorController : ControllerBase
{
    [HttpGet]
    public IActionResult GetVendorModuleStatus()
    {
        return Ok("Vendor Management API Working");
    }
}
