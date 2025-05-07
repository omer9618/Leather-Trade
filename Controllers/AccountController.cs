using Microsoft.AspNetCore.Mvc;

namespace LTMS.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
    }
}
