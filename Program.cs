using LTMS.Data;
using LTMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cookie settings - Updated to point to the new Auth page
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Auth";  // Changed from "/Account/Login"
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Debug middleware (optional)
if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        Console.WriteLine($"Request: {context.Request.Path}");
        await next();
    });
}

// Enable Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Updated endpoint routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Add explicit routes for our auth endpoints
app.MapControllerRoute(
    name: "auth",
    pattern: "Account/Auth",
    defaults: new { controller = "Account", action = "Auth" });

app.MapControllerRoute(
    name: "login",
    pattern: "Account/Login",
    defaults: new { controller = "Account", action = "Login" });

app.MapControllerRoute(
    name: "register",
    pattern: "Account/Register",
    defaults: new { controller = "Account", action = "Register" });

app.Run();