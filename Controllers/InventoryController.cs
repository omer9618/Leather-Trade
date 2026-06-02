using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTMS.Data;
using LTMS.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace LTMS.Controllers
{
    [Authorize(Roles = "Seller")]
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InventoryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Inventory
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var inventory = await _context.Inventories
                .Include(i => i.LeatherType)
                .Where(i => i.SellerId == userId)
                .OrderByDescending(i => i.LastUpdated)
                .ToListAsync();

            var leatherTypes = await _context.LeatherTypes.ToListAsync();
            ViewBag.LeatherTypes = leatherTypes;

            return View(inventory);
        }

        // GET: Inventory/GetInventory/5
        [HttpGet]
        public async Task<IActionResult> GetInventory(int id)
        {
            var userId = _userManager.GetUserId(User);
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == id && i.SellerId == userId);

            if (inventory == null)
            {
                return NotFound();
            }

            return Json(new
            {
                id = inventory.Id,
                productName = inventory.ProductName,
                leatherTypeId = inventory.LeatherTypeId,
                description = inventory.Description,
                price = inventory.Price,
                quantity = inventory.Quantity,
                unitOfMeasurement = inventory.UnitOfMeasurement,
                status = inventory.Status
            });
        }

        // GET: Inventory/Create
        public async Task<IActionResult> Create()
        {
            var leatherTypes = await _context.LeatherTypes.ToListAsync();
            ViewBag.LeatherTypes = leatherTypes;
            return View();
        }

        // POST: Inventory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Inventory inventory, IFormFile? Image)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    inventory.SellerId = _userManager.GetUserId(User);
                    inventory.Status = "Available";
                    inventory.LastUpdated = DateTime.UtcNow;

                    // Handle image upload
                    if (Image != null && Image.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/inventory");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);
                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await Image.CopyToAsync(stream);
                        }
                        inventory.ImagePath = "/images/inventory/" + uniqueFileName;
                    }

                    _context.Add(inventory);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }

            // Collect model state errors for debugging
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList() })
                .ToList();
            return Json(new { success = false, message = "Invalid model state", errors });
        }

        // GET: Inventory/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == id && i.SellerId == userId);

            if (inventory == null)
            {
                return NotFound();
            }

            var leatherTypes = await _context.LeatherTypes.ToListAsync();
            ViewBag.LeatherTypes = leatherTypes;
            return View(inventory);
        }

        // POST: Inventory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] Inventory inventory, IFormFile? Image)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = _userManager.GetUserId(User);
                    var existingInventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.Id == inventory.Id && i.SellerId == userId);

                    if (existingInventory == null)
                    {
                        return Json(new { success = false, message = "Inventory item not found" });
                    }

                    existingInventory.ProductName = inventory.ProductName;
                    existingInventory.Description = inventory.Description;
                    existingInventory.Price = inventory.Price;
                    existingInventory.Quantity = inventory.Quantity;
                    existingInventory.LeatherTypeId = inventory.LeatherTypeId;
                    existingInventory.UnitOfMeasurement = inventory.UnitOfMeasurement;
                    existingInventory.Status = inventory.Status;
                    existingInventory.LastUpdated = DateTime.UtcNow;

                    // Handle image upload
                    if (Image != null && Image.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/inventory");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);
                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await Image.CopyToAsync(stream);
                        }
                        existingInventory.ImagePath = "/images/inventory/" + uniqueFileName;
                    }

                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }

            return Json(new { success = false, message = "Invalid model state" });
        }

        // POST: Inventory/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.Id == id && i.SellerId == userId);

                if (inventory != null)
                {
                    _context.Inventories.Remove(inventory);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Inventory item not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private bool InventoryExists(int id)
        {
            return _context.Inventories.Any(e => e.Id == id);
        }
    }
} 