using Microsoft.AspNetCore.Mvc;

namespace Partivex.Controllers
{
    public class VendorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
