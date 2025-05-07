using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Seller")]
[Route("Seller/[action]")] // Add this line
public class SellerController : Controller
{
    [HttpGet] // Explicit HTTP method
    public IActionResult Dashboard()
    {
        ViewBag.WelcomeMessage = $"Welcome, {User.Identity?.Name}!";
        return View();
    }

    [HttpGet] // Explicit HTTP method
    public IActionResult Inventory()
    {
        return View();
    }

    [HttpGet] // Explicit HTTP method
    public IActionResult Orders()
    {
        return View();
    }
}