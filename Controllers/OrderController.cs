using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LTMS.Models;
using LTMS.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

[Authorize(Roles = "Buyer")]
[Route("Order/[action]")]
public class OrderController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public OrderController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var orders = await _context.Orders
            .Include(o => o.Seller)
            .Where(o => o.BuyerId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        // Get reviews for these orders
        var orderIds = orders.Select(o => o.Id).ToList();
        var reviews = await _context.Reviews
            .Where(r => orderIds.Contains(r.OrderId))
            .ToListAsync();

        ViewBag.Reviews = reviews;

        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var userId = _userManager.GetUserId(User);
        var order = await _context.Orders
            .Include(o => o.Seller)
            .Include(o => o.Bid)
            .ThenInclude(b => b.Demand)
            .FirstOrDefaultAsync(o => o.Id == id && o.BuyerId == userId);

        if (order == null)
            return NotFound();

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsReceived(int id)
    {
        var userId = _userManager.GetUserId(User);
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id && o.BuyerId == userId);
        if (order == null)
            return NotFound();
        if (order.Status != "Pending")
        {
            TempData["ErrorMessage"] = "Order cannot be marked as received.";
            return RedirectToAction("Details", new { id });
        }
        order.Status = "Completed";
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Order marked as received.";
        return RedirectToAction("Details", new { id });
    }
} 