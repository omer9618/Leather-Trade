using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LTMS.Models;
using System.Threading.Tasks;

[Authorize(Roles = "Seller")]
[Route("Seller/[action]")]
public class SellerController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public SellerController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        var fullName = user?.FullName ?? "Seller";

        ViewBag.WelcomeMessage = $"Welcome, {fullName}!";
        return View();
    }

    [HttpGet]
    public IActionResult Inventory()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Orders()
    {
        return View();
    }
}
