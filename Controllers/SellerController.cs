using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LTMS.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LTMS.Data;
using System.Security.Claims;

[Authorize(Roles = "Seller")]
[Route("Seller/[action]")]
public class SellerController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public SellerController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var userId = _userManager.GetUserId(User);
        var openDemands = await _context.Demands
            .Where(d => d.Status == "Open")
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();

        var myBids = await _context.Bids
            .Include(b => b.Demand)
            .Where(b => b.SellerId == userId)
            .OrderByDescending(b => b.SubmittedDate)
            .Take(5)
            .ToListAsync();

        ViewBag.OpenDemands = openDemands;
        ViewBag.MyBids = myBids;

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> MyBids()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var bids = await _context.Bids
            .Include(b => b.Demand)
            .Where(b => b.SellerId == userId)
            .OrderByDescending(b => b.SubmittedDate)
            .ToListAsync();

        return View(bids);
    }

    [HttpGet]
    public async Task<IActionResult> Demands()
    {
        var demands = await _context.Demands
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();

        return View(demands);
    }

    [HttpGet]
    public async Task<IActionResult> DemandDetails(int id)
    {
        var demand = await _context.Demands
            .Include(d => d.LeatherType)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (demand == null)
        {
            return NotFound();
        }

        // Check if seller has matching inventory
        var userId = _userManager.GetUserId(User);
        var hasMatchingInventory = await _context.Inventories
            .AnyAsync(i => i.SellerId == userId && 
                          i.LeatherTypeId == demand.LeatherTypeId && 
                          i.Status == "Available");

        ViewBag.HasMatchingInventory = hasMatchingInventory;

        return View(demand);
    }

    [HttpGet]
    public async Task<IActionResult> OrderDetails(int id)
    {
        var userId = _userManager.GetUserId(User);
        var order = await _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.Bid)
            .FirstOrDefaultAsync(o => o.Id == id && o.SellerId == userId);
        if (order == null)
            return NotFound();
        return View(order);
    }

    [HttpGet]
    public async Task<IActionResult> Orders()
    {
        var userId = _userManager.GetUserId(User);
        var orders = await _context.Orders
            .Include(o => o.Buyer)
            .OrderByDescending(o => o.OrderDate)
            .Where(o => o.SellerId == userId)
            .ToListAsync();
        return View(orders);
    }
}


