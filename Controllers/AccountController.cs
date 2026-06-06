using System.Security.Claims;
using AcademicAIAssistant.Data;
using AcademicAIAssistant.Models;
using AcademicAIAssistant.Models.ViewModels;
using AcademicAIAssistant.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace AcademicAIAssistant.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly ILoginRateLimiter _loginRateLimiter;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public AccountController(AppDbContext context, ILoginRateLimiter loginRateLimiter, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _loginRateLimiter = loginRateLimiter;
        _localizer = localizer;
        _passwordHasher = new PasswordHasher<User>();
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        bool emailExists = await _context.Users.AnyAsync(user => user.Email == model.Email);
        if (emailExists)
        {
            ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
            return View(model);
        }

        var user = new User
        {
            FullName = model.FullName,
            Email = model.Email,
            Role = "Student"
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await SignInUserAsync(user, isPersistent: false);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        string normalizedEmail = NormalizeEmail(model.Email);
        string ipAddress = GetClientIpAddress();

        LoginRateLimitResult lockoutStatus = _loginRateLimiter.GetLockoutStatus(normalizedEmail, ipAddress);
        if (lockoutStatus.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, _localizer["Login_TooManyAttempts", lockoutStatus.RemainingMinutes]);
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _context.Users.FirstOrDefaultAsync(item => item.Email.ToLower() == normalizedEmail);
        if (user == null)
        {
            _loginRateLimiter.RecordFailedAttempt(normalizedEmail, ipAddress);
            ModelState.AddModelError(string.Empty, _localizer["Login_InvalidCredentials"]);
            return View(model);
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            _loginRateLimiter.RecordFailedAttempt(normalizedEmail, ipAddress);
            ModelState.AddModelError(string.Empty, _localizer["Login_InvalidCredentials"]);
            return View(model);
        }

        _loginRateLimiter.ResetAttempts(normalizedEmail, ipAddress);
        await SignInUserAsync(user, model.RememberMe);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Dashboard");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Index", "Home");
    }

    private async Task SignInUserAsync(User user, bool isPersistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var properties = new AuthenticationProperties
        {
            IsPersistent = isPersistent,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserFullName", user.FullName);
        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("UserRole", user.Role);
    }

    private string GetClientIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static string NormalizeEmail(string email)
    {
        return string.IsNullOrWhiteSpace(email) ? string.Empty : email.Trim().ToLowerInvariant();
    }
}
