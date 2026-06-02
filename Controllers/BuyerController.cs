using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LTMS.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LTMS.Data;

[Authorize(Roles = "Buyer")]
[Route("Buyer/[action]")]
public class BuyerController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public BuyerController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    // Dashboard: List all demands posted by this buyer
    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var userId = _userManager.GetUserId(User);
        var myDemands = await _context.Demands
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();

        return View(myDemands);
    }

    // View all bids for a specific demand
    [HttpGet]
    [Route("Buyer/DemandBids/{id:int}")]
    public async Task<IActionResult> DemandBids(int id)
    {
        var demand = await _context.Demands
            .Include(d => d.Bids)
            .ThenInclude(b => b.Seller)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (demand == null || demand.UserId != _userManager.GetUserId(User))
            return NotFound();

        // Get all unique sellerIds from the bids
        var sellerIds = demand.Bids
            .Where(b => b.SellerId != null)
            .Select(b => b.SellerId)
            .Distinct()
            .ToList();

        // Fetch reviews for these sellers with buyer information
        var reviews = await _context.Reviews
            .Include(r => r.Buyer)
            .Where(r => sellerIds.Contains(r.SellerId) && r.IsVerified)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        // Build a dictionary: sellerId -> list of reviews
        var sellerReviews = sellerIds.ToDictionary(
            sellerId => sellerId,
            sellerId => reviews.Where(r => r.SellerId == sellerId).ToList()
        );

        // Build a dictionary: sellerId -> (average, count) for ratings display
        var sellerRatings = sellerIds.ToDictionary(
            sellerId => sellerId,
            sellerId => {
                var sellerReviews = reviews.Where(r => r.SellerId == sellerId).ToList();
                double avg = sellerReviews.Any() ? sellerReviews.Average(r => r.Rating) : 0;
                int count = sellerReviews.Count;
                return (avg, count);
            });

        ViewBag.SellerReviews = sellerReviews;
        ViewBag.SellerRatings = sellerRatings;
        return View(demand);
    }

    // Accept a bid
    [HttpPost]
    public async Task<IActionResult> AcceptBid(int bidId)
    {
        var bid = await _context.Bids.Include(b => b.Demand).FirstOrDefaultAsync(b => b.Id == bidId);
        if (bid == null || bid.Demand.UserId != _userManager.GetUserId(User))
            return NotFound();

        bid.Status = "Accepted";
        bid.Demand.Status = "Accepted";
        // Optionally, reject all other bids for this demand
        var otherBids = _context.Bids.Where(b => b.DemandId == bid.DemandId && b.Id != bidId);
        foreach (var other in otherBids)
            other.Status = "Rejected";

        await _context.SaveChangesAsync();
        return RedirectToAction("DemandBids", new { id = bid.DemandId });
    }

    // Reject a bid
    [HttpPost]
    public async Task<IActionResult> RejectBid(int bidId)
    {
        var bid = await _context.Bids.Include(b => b.Demand).FirstOrDefaultAsync(b => b.Id == bidId);
        if (bid == null || bid.Demand.UserId != _userManager.GetUserId(User))
            return NotFound();

        bid.Status = "Rejected";
        await _context.SaveChangesAsync();
        return RedirectToAction("DemandBids", new { id = bid.DemandId });
    }

    // Create new demand (GET/POST)
    [HttpGet]
    public async Task<IActionResult> CreateDemand()
    {
        var leatherTypes = await _context.LeatherTypes.ToListAsync();
        ViewBag.CommonLeatherTypes = leatherTypes.Where(lt => lt.Category == "Common").ToList();
        ViewBag.SpecialtyLeatherTypes = leatherTypes.Where(lt => lt.Category == "Specialty").ToList();
        ViewBag.ExoticLeatherTypes = leatherTypes.Where(lt => lt.Category == "Exotic").ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDemand(Demand model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError("", "User is not authenticated.");
                    return View(model);
                }

                // Verify leather type exists
                var leatherType = await _context.LeatherTypes.FindAsync(model.LeatherTypeId);
                if (leatherType == null)
                {
                    ModelState.AddModelError("LeatherTypeId", "Selected leather type is invalid.");
                    return View(model);
                }

                model.UserId = userId;
                model.Status = "Open";
                model.CreatedDate = DateTime.UtcNow;
                model.Bids = new List<Bid>();

                _context.Demands.Add(model);
                await _context.SaveChangesAsync();

                // Notify sellers who have matching inventory
                var matchingSellers = await _context.Inventories
                    .Where(i => i.LeatherTypeId == model.LeatherTypeId && i.Status == "Available")
                    .Select(i => i.SellerId)
                    .Distinct()
                    .ToListAsync();

                foreach (var sellerId in matchingSellers)
                {
                    var notification = new Notification
                    {
                        UserId = sellerId,
                        Title = "New Matching Demand",
                        Message = $"A new demand for {leatherType.Name} has been posted that matches your inventory.",
                        Type = "Demand",
                        ReferenceId = model.Id.ToString(),
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    };
                    _context.Notifications.Add(notification);
                }
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Demand created successfully!";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Exception", $"Exception: {ex.Message}");
                ModelState.AddModelError("StackTrace", ex.StackTrace ?? "No stack trace available.");
            }
        }
        else
        {
            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key].Errors;
                foreach (var error in errors)
                {
                    ModelState.AddModelError(key, $"Field: {key} - Error: {error.ErrorMessage}");
                }
            }
        }

        // Reload leather types for the view
        var leatherTypes = await _context.LeatherTypes.ToListAsync();
        ViewBag.CommonLeatherTypes = leatherTypes.Where(lt => lt.Category == "Common").ToList();
        ViewBag.SpecialtyLeatherTypes = leatherTypes.Where(lt => lt.Category == "Specialty").ToList();
        ViewBag.ExoticLeatherTypes = leatherTypes.Where(lt => lt.Category == "Exotic").ToList();
        return View(model);
    }

    // Delete demand and notify sellers
    [HttpPost]
    public async Task<IActionResult> DeleteDemand(int id)
    {
        var demand = await _context.Demands.Include(d => d.Bids).FirstOrDefaultAsync(d => d.Id == id);
        if (demand == null || demand.UserId != _userManager.GetUserId(User))
            return NotFound();

        // Notify sellers (pseudo-code, implement your notification system)
        foreach (var bid in demand.Bids)
        {
            // NotifySeller(bid.SellerId, "A demand you bid on was deleted.");
            _context.Bids.Remove(bid);
        }
        _context.Demands.Remove(demand);
        await _context.SaveChangesAsync();
        return RedirectToAction("Dashboard");
    }
}