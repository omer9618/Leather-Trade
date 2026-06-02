using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LTMS.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LTMS.Data;

namespace LTMS.Controllers
{
    [Authorize(Roles = "Buyer")]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int orderId, int rating, string comment)
        {
            if (rating < 1 || rating > 5)
            {
                return BadRequest("Rating must be between 1 and 5");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _context.Orders
                .Include(o => o.Seller)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerId == userId);

            if (order == null)
            {
                return NotFound("Order not found");
            }

            if (order.Status != "Completed")
            {
                return BadRequest("You can only review completed orders");
            }

            // Check if review already exists
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.OrderId == orderId);

            if (existingReview != null)
            {
                return BadRequest("You have already reviewed this order");
            }

            var review = new Review
            {
                BuyerId = userId,
                SellerId = order.SellerId,
                OrderId = orderId,
                Rating = rating,
                Comment = comment,
                IsVerified = true
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Review submitted successfully";
            return RedirectToAction("Details", "Order", new { id = orderId });
        }

        [HttpGet]
        public async Task<IActionResult> SellerReviews(string sellerId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.Buyer)
                .Include(r => r.Order)
                .Where(r => r.SellerId == sellerId && r.IsVerified)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }
    }
} 