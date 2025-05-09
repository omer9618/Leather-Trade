using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LTMS.Models;
using LTMS.ViewModels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LTMS.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Auth(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                _logger.LogInformation("Authenticated user attempted to access Auth page");
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            _logger.LogInformation("Loading combined auth page");
            return View();
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login attempt with invalid model");
                return View("Auth", model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation($"User logged in: {model.Email}");
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (await _userManager.IsInRoleAsync(user, "Seller"))
                {
                    return RedirectToAction("Dashboard", "Seller");
                }
                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            _logger.LogWarning($"Failed login attempt for: {model.Email}");
            return View("Auth", model);
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Registration attempt with invalid model");
                return View("Auth", model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                _logger.LogWarning($"Registration attempt with existing email: {model.Email}");
                ModelState.AddModelError("Email", "Email is already registered.");
                return View("Auth", model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.Name
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation($"New user registered: {model.Email}");

                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    _logger.LogInformation($"Creating new role: {model.Role}");
                    await _roleManager.CreateAsync(new IdentityRole(model.Role));
                }

                await _userManager.AddToRoleAsync(user, model.Role);
                _logger.LogInformation($"Added user {model.Email} to role {model.Role}");

                await _signInManager.SignInAsync(user, isPersistent: false);

                if (model.Role == "Seller")
                {
                    return RedirectToAction("Dashboard", "Seller");
                }
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
                _logger.LogWarning($"User creation error: {error.Description}");
            }

            return View("Auth", model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            _logger.LogWarning("Access denied page loaded");
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}