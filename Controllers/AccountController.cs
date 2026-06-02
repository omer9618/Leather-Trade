using LTMS.Models;
using LTMS.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LTMS.Data;
using Microsoft.EntityFrameworkCore;

namespace LTMS.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _context; // Added DbContext

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger,
            ApplicationDbContext context) // Added DbContext parameter
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _context = context; // Initialize DbContext
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    var user = await _userManager.FindByEmailAsync(model.Email);

                    if (await _userManager.IsInRoleAsync(user, "Seller"))
                    {
                        return RedirectToAction("Dashboard", "Seller");
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Buyer"))
                    {
                        return RedirectToAction("Dashboard", "Buyer");
                    }
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterViewModel { Role = "Buyer" }); // Set default role
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                _logger.LogWarning("Registration failed: Model state is invalid. Errors: {Errors}", errors);
                return View(model);
            }

            try
            {
                _logger.LogInformation("Starting registration for {Email}", model.Email);

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration failed: User with email {Email} already exists", model.Email);
                    ModelState.AddModelError("Email", "A user with this email already exists.");
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.Name,
                    EmailConfirmed = true
                };

                _logger.LogInformation("Creating user with email: {Email}", model.Email);
                var createResult = await _userManager.CreateAsync(user, model.Password);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    _logger.LogError("User creation failed: {Errors}", errors);
                    foreach (var error in createResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }

                _logger.LogInformation("User created successfully. Adding to role: {Role}", model.Role);

                // Ensure role exists
                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    _logger.LogInformation("Creating role: {Role}", model.Role);
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole(model.Role));
                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                        _logger.LogError("Role creation failed: {Errors}", errors);
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(model);
                    }
                }

                var addToRoleResult = await _userManager.AddToRoleAsync(user, model.Role);
                if (!addToRoleResult.Succeeded)
                {
                    var errors = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
                    _logger.LogError("Role assignment failed: {Errors}", errors);
                    foreach (var error in addToRoleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }

                _logger.LogInformation("Successfully registered user {Email} with role {Role}", user.Email, model.Role);

                // Sign in the user
                await _signInManager.SignInAsync(user, isPersistent: false);

                if (model.Role == "Seller")
                {
                    return RedirectToAction("Dashboard", "Seller");
                }
                else if (model.Role == "Buyer")
                {
                    return RedirectToAction("Dashboard", "Buyer");
                }
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for {Email}. Exception: {Message}, Stack Trace: {StackTrace}", 
                    model.Email, ex.Message, ex.StackTrace);
                ModelState.AddModelError("", $"An error occurred during registration: {ex.Message}");
                return View(model);
            }
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: /Account/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Calculate analytics
            var isBuyer = await _userManager.IsInRoleAsync(user, "Buyer");
            var isSeller = await _userManager.IsInRoleAsync(user, "Seller");
            decimal totalSpent = 0;
            decimal totalEarned = 0;
            int totalOrders = 0;

            if (isBuyer)
            {
                totalOrders = _context.Orders.Count(o => o.BuyerId == user.Id);
                totalSpent = _context.Orders.Where(o => o.BuyerId == user.Id).Sum(o => (decimal?)o.Amount) ?? 0;
            }
            if (isSeller)
            {
                totalOrders = _context.Orders.Count(o => o.SellerId == user.Id);
                totalEarned = _context.Orders.Where(o => o.SellerId == user.Id).Sum(o => (decimal?)o.Amount) ?? 0;
            }

            var model = new ProfileViewModel
            {
                Name = user.FullName,
                Email = user.Email,
                Role = isBuyer ? "Buyer" : isSeller ? "Seller" : "",
                TotalOrders = totalOrders,
                TotalSpent = totalSpent,
                TotalEarned = totalEarned
            };
            return View(model);
        }

        #region Helpers
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        #endregion
    }


}
