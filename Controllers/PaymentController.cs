using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Stripe;
using LTMS.Models;
using LTMS.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace LTMS.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string _stripeSecretKey;
        private readonly string _stripePublishableKey;
        private readonly ILogger<PaymentController> _logger;
        private readonly IConfiguration _configuration;

        public PaymentController(ApplicationDbContext context, IConfiguration configuration, ILogger<PaymentController> logger)
        {
            _context = context;
            _stripeSecretKey = configuration["Stripe:SecretKey"];
            _stripePublishableKey = configuration["Stripe:PublishableKey"];
            _logger = logger;
            StripeConfiguration.ApiKey = _stripeSecretKey;
        }

        [HttpGet]
        public async Task<IActionResult> PaymentForm(int bidId)
        {
            try
            {
                var bid = await _context.Bids
                    .Include(b => b.Demand)
                    .FirstOrDefaultAsync(b => b.Id == bidId);

                if (bid == null)
                {
                    return NotFound();
                }

                // Check if payment already exists
                var existingPayment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.BidId == bidId && p.Status == "Completed");

                if (existingPayment != null)
                {
                    return RedirectToAction("Success");
                }

                // Create a new Payment model with the bid information
                var payment = new Payment
                {
                    Amount = bid.Amount,
                    BidId = bid.Id,
                    Status = "Pending"
                };

                return View(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment form for bid {BidId}", bidId);
                TempData["ErrorMessage"] = "An error occurred while loading the payment form.";
                return RedirectToAction("Details", "Bid", new { id = bidId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
        {
            try
            {
                _logger.LogInformation("Creating payment intent for bid {BidId}", request?.BidId);

                if (request == null)
                {
                    _logger.LogWarning("Request body is null");
                    return BadRequest(new { success = false, message = "Request body is required" });
                }

                if (request.BidId <= 0)
                {
                    _logger.LogWarning("Invalid bid ID: {BidId}", request.BidId);
                    return BadRequest(new { success = false, message = "Invalid bid ID" });
                }

                // Verify bid exists
                var bid = await _context.Bids
                    .Include(b => b.Demand)
                    .FirstOrDefaultAsync(b => b.Id == request.BidId);

                if (bid == null)
                {
                    _logger.LogWarning("Bid not found: {BidId}", request.BidId);
                    return NotFound(new { success = false, message = "Bid not found" });
                }

                // Check if payment already exists
                var existingPayment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.BidId == request.BidId && p.Status == "Completed");

                if (existingPayment != null)
                {
                    _logger.LogWarning("Payment already exists for bid: {BidId}", request.BidId);
                    return BadRequest(new { success = false, message = "Payment already exists for this bid" });
                }

                // Always return a fake successful payment intent for demo purposes
                return Ok(new {
                    success = true,
                    clientSecret = "demo_client_secret",
                    paymentId = 1,
                    amount = bid.Amount,
                    currency = "usd"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment intent for bid {BidId}: {Message}", request?.BidId, ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while creating the payment intent." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Confirming payment for bid {BidId}", request?.BidId);

                if (request == null)
                {
                    _logger.LogWarning("Request body is null");
                    return BadRequest(new { success = false, message = "Request body is required" });
                }

                if (request.BidId <= 0)
                {
                    _logger.LogWarning("Invalid bid ID: {BidId}", request.BidId);
                    return BadRequest(new { success = false, message = "Invalid bid ID" });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthenticated user attempted payment confirmation");
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                // Get the bid
                var bid = await _context.Bids
                    .Include(b => b.Demand)
                    .FirstOrDefaultAsync(b => b.Id == request.BidId);

                if (bid == null)
                {
                    _logger.LogWarning("Bid not found: {BidId}", request.BidId);
                    return NotFound(new { success = false, message = "Bid not found" });
                }

                if (bid.Demand == null)
                {
                    _logger.LogWarning("Demand not found for bid: {BidId}", request.BidId);
                    return BadRequest(new { success = false, message = "Associated demand not found" });
                }

                // Check if payment already exists
                var existingPayment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.BidId == request.BidId && p.Status == "Completed");

                if (existingPayment != null)
                {
                    _logger.LogWarning("Payment already exists for bid: {BidId}", request.BidId);
                    return BadRequest(new { success = false, message = "Payment already exists for this bid" });
                }

                try
                {
                    // Create new order record
                    var order = new Order
                    {
                        BuyerId = userId,
                        SellerId = bid.SellerId,
                        BidId = request.BidId,
                        Amount = bid.Amount,
                        OrderDate = DateTime.UtcNow,
                        Status = "Pending"
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync(); // Save to get the OrderId

                    // Create new payment record
                    var payment = new Payment
                    {
                        BidId = request.BidId,
                        OrderId = order.Id,
                        Amount = bid.Amount,
                        Status = "Completed",
                        PaymentDate = DateTime.UtcNow,
                        TransactionId = Guid.NewGuid().ToString(),
                        BuyerId = userId,
                        SellerId = bid.SellerId,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Validate payment data
                    var validationContext = new ValidationContext(payment, serviceProvider: null, items: null);
                    var validationResults = new List<ValidationResult>();
                    if (!Validator.TryValidateObject(payment, validationContext, validationResults, validateAllProperties: true))
                    {
                        var errorMessages = validationResults.Select(r => r.ErrorMessage);
                        _logger.LogWarning("Payment validation failed: {Errors}", string.Join(", ", errorMessages));
                        return BadRequest(new { success = false, message = "Invalid payment data: " + string.Join(", ", errorMessages) });
                    }

                    _context.Payments.Add(payment);

                    // Update bid status
                    bid.Status = "Paid";

                    // Create notification for seller
                    var notification = new Notification
                    {
                        UserId = bid.SellerId,
                        Title = "Payment Received",
                        Message = $"You have received a payment of ${payment.Amount:N2} for Bid #{bid.Id}.",
                        Type = "Payment",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    };

                    _context.Notifications.Add(notification);

                    try
                    {
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Payment confirmed successfully for bid {BidId}", request.BidId);
                        return Ok(new { success = true, message = "Payment confirmed successfully" });
                    }
                    catch (DbUpdateException dbEx)
                    {
                        _logger.LogError(dbEx, "Database error while saving payment for bid {BidId}: {Message}", request.BidId, dbEx.Message);
                        
                        // Check for specific database errors
                        if (dbEx.InnerException != null)
                        {
                            var innerException = dbEx.InnerException;
                            if (innerException.Message.Contains("duplicate key"))
                            {
                                return BadRequest(new { success = false, message = "A payment for this bid already exists" });
                            }
                            else if (innerException.Message.Contains("foreign key constraint"))
                            {
                                return BadRequest(new { success = false, message = "Invalid reference data (bid, user, or demand)" });
                            }
                        }
                        
                        return StatusCode(500, new { success = false, message = "Error saving payment to database. Please try again." });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing payment for bid {BidId}: {Message}", request.BidId, ex.Message);
                    return StatusCode(500, new { success = false, message = "An error occurred while processing your payment." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment for bid {BidId}: {Message}", request?.BidId, ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while processing your payment." });
            }
        }

        public IActionResult Success()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(string stripeToken, decimal amount, int orderId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Get the bid
                var bid = await _context.Bids
                    .Include(b => b.Demand)
                    .FirstOrDefaultAsync(b => b.Id == orderId);

                if (bid == null)
                {
                    return Json(new { success = false, message = "Bid not found" });
                }

                // Verify the amount matches
                if (bid.Amount != amount)
                {
                    return Json(new { success = false, message = "Amount mismatch" });
                }

                var options = new ChargeCreateOptions
                {
                    Amount = (long)(amount * 100), // Convert to cents
                    Currency = "usd",
                    Description = $"Payment for Bid #{orderId}",
                    Source = stripeToken,
                    Metadata = new Dictionary<string, string>
                    {
                        { "BidId", orderId.ToString() },
                        { "BuyerId", userId },
                        { "SellerId", bid.Demand.UserId }
                    }
                };

                var service = new ChargeService();
                var charge = await service.CreateAsync(options);

                if (charge.Status == "succeeded")
                {
                    // Create new order record
                    var order = new Order
                    {
                        BuyerId = userId,
                        SellerId = bid.Demand.UserId,
                        BidId = orderId,
                        Amount = amount,
                        OrderDate = DateTime.UtcNow,
                        Status = "Pending"
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync(); // Save to get the OrderId

                    // Create payment record
                    var payment = new Payment
                    {
                        BidId = orderId,
                        OrderId = order.Id,
                        Amount = amount,
                        PaymentDate = DateTime.UtcNow,
                        Status = "Completed",
                        TransactionId = charge.Id,
                        StripePaymentIntentId = charge.PaymentIntentId,
                        BuyerId = userId,
                        SellerId = bid.Demand.UserId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Payments.Add(payment);

                    // Update bid status
                    bid.Status = "Paid";
                    await _context.SaveChangesAsync();

                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Payment failed" });
                }
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error processing payment");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                return Json(new { success = false, message = "An unexpected error occurred" });
            }
        }

        [HttpGet]
        public IActionResult GetPaymentModal(decimal amount, int orderId)
        {
            ViewBag.StripePublishableKey = _stripePublishableKey;
            ViewBag.Amount = amount;
            ViewBag.OrderId = orderId;
            return PartialView("_PaymentModal");
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var payments = await _context.Payments
                .Include(p => p.Bid)
                .Include(p => p.Buyer)
                .Include(p => p.Seller)
                .Where(p => p.BuyerId == userId || p.SellerId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(payments);
        }
    }

    public class CreatePaymentIntentRequest
    {
        public int BidId { get; set; }
    }

    public class ConfirmPaymentRequest
    {
        public string PaymentIntentId { get; set; }
        public int BidId { get; set; }
    }
} 