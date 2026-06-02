using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LTMS.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LTMS.Data;
using Microsoft.AspNetCore.Http;
using System.IO;

[Authorize(Roles = "Seller")]
public class BidController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BidController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("DemandId,Amount,Description,DeliveryTime")] Bid bid, IFormFile BidImage)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var demand = await _context.Demands.FindAsync(bid.DemandId);
                if (demand == null || demand.Status != "Open")
                {
                    TempData["ErrorMessage"] = "This demand is no longer open for bidding.";
                    return RedirectToAction("DemandDetails", "Seller", new { id = bid.DemandId });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User not found. Please try logging in again.";
                    return RedirectToAction("DemandDetails", "Seller", new { id = bid.DemandId });
                }

                string imagePath = null;
                if (BidImage != null && BidImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/bids");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(BidImage.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await BidImage.CopyToAsync(stream);
                    }
                    imagePath = "/images/bids/" + uniqueFileName;
                }

                // Create a new bid instance
                var newBid = new Bid
                {
                    DemandId = bid.DemandId,
                    Amount = bid.Amount,
                    Description = bid.Description,
                    DeliveryTime = bid.DeliveryTime,
                    SellerId = userId,
                    Status = "Pending",
                    SubmittedDate = DateTime.UtcNow,
                    ImagePath = imagePath
                };

                _context.Add(newBid);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Bid submitted successfully!";
                return RedirectToAction("DemandDetails", "Seller", new { id = bid.DemandId });
            }
            catch (Exception ex)
            {
                // Log the error (ex)
                TempData["ErrorMessage"] = "An error occurred while submitting your bid. Please try again.";
                return RedirectToAction("DemandDetails", "Seller", new { id = bid.DemandId });
            }
        }

        // Collect validation errors
        var errors = ModelState.Values.SelectMany(v => v.Errors)
                                     .Select(e => e.ErrorMessage)
                                     .ToList();
        TempData["ErrorMessage"] = "Please correct the following errors: " + string.Join(", ", errors);
        return RedirectToAction("DemandDetails", "Seller", new { id = bid.DemandId });
    }

    public async Task<IActionResult> MyBids()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "User not found. Please try logging in again.";
                return RedirectToAction("Index", "Home");
            }

            var bids = await _context.Bids
                .Include(b => b.Demand)
                .Where(b => b.SellerId == userId)
                .OrderByDescending(b => b.SubmittedDate)
                .ToListAsync();

            return View(bids);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred while loading your bids.";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var bid = await _context.Bids.FindAsync(id);
            if (bid == null)
            {
                return NotFound();
            }

            // Verify that the current user owns this bid
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (bid.SellerId != userId)
            {
                return Forbid();
            }

            _context.Bids.Remove(bid);
            await _context.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            // Log the error
            return StatusCode(500);
        }
    }

    
}