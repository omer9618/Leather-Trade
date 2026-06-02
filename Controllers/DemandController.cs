using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LTMS.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LTMS.Data;
using Microsoft.AspNetCore.SignalR;
using LTMS;

public class DemandController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<LTMS.NotificationHub> _notificationHub;

    public DemandController(ApplicationDbContext context, IHubContext<LTMS.NotificationHub> notificationHub)
    {
        _context = context;
        _notificationHub = notificationHub;
    }

    // GET: Demand
    public async Task<IActionResult> Index()
    {
        var demands = await _context.Demands
            .Where(d => d.Status == "Open")
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();
        return View(demands);
    }

    // GET: Demand/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var demand = await _context.Demands
            .Include(d => d.Bids)
            .ThenInclude(b => b.Seller)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (demand == null)
        {
            return NotFound();
        }

        return View(demand);
    }

    [Authorize]
    // GET: Demand/Create
    public IActionResult Create()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Description,Deadline,LeatherTypeId")] Demand demand)
    {
        if (ModelState.IsValid)
        {
            demand.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            demand.CreatedDate = DateTime.UtcNow;
            demand.Status = "Open";

            _context.Add(demand);
            await _context.SaveChangesAsync();

            // Notify sellers with matching inventory
            var matchingSellerIds = await _context.Inventories
                .Where(i => i.LeatherTypeId == demand.LeatherTypeId)
                .Select(i => i.SellerId)
                .Distinct()
                .ToListAsync();

            var leatherType = await _context.LeatherTypes.FindAsync(demand.LeatherTypeId);
            string leatherTypeName = leatherType?.Name ?? "a leather type";
            // Debugging: Log matching seller IDs and leather type name
            System.Diagnostics.Debug.WriteLine($"Matching seller IDs for leather type '{leatherTypeName}': {string.Join(", ", matchingSellerIds)}");
            foreach (var sellerId in matchingSellerIds)
            {
                if (!string.IsNullOrEmpty(sellerId))
                {
                    var notification = new Notification
                    {
                        UserId = sellerId,
                        Title = "New Demand Posted",
                        Message = $"A new demand for {leatherTypeName} has been posted.",
                        Type = "Demand",
                        IsRead = false,
                        ReferenceId = demand.Id.ToString(),
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Add(notification);
                    // Debugging: Log notification details
                    System.Diagnostics.Debug.WriteLine($"Created notification for seller {sellerId}: Title={notification.Title}, Message={notification.Message}, ReferenceId={notification.ReferenceId}");
                    // Send real-time notification via SignalR (only if the seller is online)
                    await _notificationHub.Clients.User(sellerId).SendAsync(
                        "ReceiveNotification",
                        notification.Title,
                        notification.Message,
                        notification.ReferenceId
                    );
                    // Debugging: Log SignalR notification send attempt
                    System.Diagnostics.Debug.WriteLine($"Sent SignalR notification to seller {sellerId}: Title={notification.Title}, Message={notification.Message}, ReferenceId={notification.ReferenceId}");
                }
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        return View(demand);
    }
}