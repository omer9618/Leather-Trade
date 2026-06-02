using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTMS.Data;
using LTMS.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text.Json.Serialization;

namespace LTMS.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetBidChat")]
        public async Task<IActionResult> GetBidChat(int bidId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var bid = await _context.Bids
                    .Include(b => b.Seller)
                    .Include(b => b.Demand)
                    .FirstOrDefaultAsync(b => b.Id == bidId);

                if (bid == null)
                    return NotFound("Bid not found");

                // Check if user is either the buyer or seller of this bid
                if (bid.SellerId != userId && bid.Demand.UserId != userId)
                    return Forbid();

                var messages = await _context.ChatMessages
                    .Where(m => m.BidId == bidId)
                    .OrderBy(m => m.Timestamp)
                    .Select(m => new
                    {
                        m.SenderId,
                        m.ReceiverId,
                        m.Message,
                        m.Timestamp
                    })
                    .ToListAsync();

                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while fetching chat messages");
            }
        }

        [HttpPost("SendBidMessage")]
        public async Task<IActionResult> SendBidMessage([FromBody] ChatMessageModel model)
        {
            if (model == null)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine("[Chat] Model is null");
                return BadRequest("Model is null");
            }
            if (model.BidId == 0 || string.IsNullOrEmpty(model.SenderId) || string.IsNullOrEmpty(model.ReceiverId) || string.IsNullOrEmpty(model.Message))
            {
                System.Diagnostics.Debug.WriteLine($"[Chat] Missing fields: BidId={model.BidId}, SenderId={model.SenderId}, ReceiverId={model.ReceiverId}, Message={model.Message}");
                return BadRequest("Missing required fields");
            }
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var bid = await _context.Bids
                    .Include(b => b.Seller)
                    .Include(b => b.Demand)
                    .FirstOrDefaultAsync(b => b.Id == model.BidId);

                if (bid == null)
                    return NotFound("Bid not found");

                // Check if user is either the buyer or seller of this bid
                if (bid.SellerId != userId && bid.Demand.UserId != userId)
                    return Forbid();

                var message = new ChatMessage
                {
                    BidId = model.BidId,
                    SenderId = model.SenderId,
                    ReceiverId = model.ReceiverId,
                    Message = model.Message,
                    Timestamp = DateTime.UtcNow
                };

                _context.ChatMessages.Add(message);
                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Chat] Exception: {ex.Message}");
                return StatusCode(500, "An error occurred while sending the message");
            }
        }

        [HttpPost("SendBidImage")]
        public async Task<IActionResult> SendBidImage([FromForm] int bidId, [FromForm] string senderId, [FromForm] string receiverId, [FromForm] IFormFile image)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var bid = await _context.Bids
                    .Include(b => b.Seller)
                    .Include(b => b.Demand)
                    .FirstOrDefaultAsync(b => b.Id == bidId);

                if (bid == null)
                    return NotFound("Bid not found");

                // Check if user is either the buyer or seller of this bid
                if (bid.SellerId != userId && bid.Demand.UserId != userId)
                    return Forbid();

                string imagePath = null;
                if (image != null && image.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/chat");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    imagePath = "/images/chat/" + uniqueFileName;
                }

                var message = new ChatMessage
                {
                    BidId = bidId,
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Message = "[Image]",
                    ImagePath = imagePath,
                    Timestamp = DateTime.UtcNow
                };

                _context.ChatMessages.Add(message);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, imagePath = imagePath, senderId, receiverId, timestamp = message.Timestamp });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while sending the image message");
            }
        }
    }

    public class ChatMessageModel
    {
        [JsonPropertyName("bidId")]
        public int BidId { get; set; }
        [JsonPropertyName("senderId")]
        public string SenderId { get; set; }
        [JsonPropertyName("receiverId")]
        public string ReceiverId { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
} 