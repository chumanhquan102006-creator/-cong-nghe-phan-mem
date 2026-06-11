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
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ReplaceLocalizedValidationMessages();
            return View(model);
        }

        int userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return RedirectToAction(nameof(Login));
        }

        var user = await _context.Users.FirstOrDefaultAsync(item => item.Id == userId);
        if (user == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.CurrentPassword);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(nameof(model.CurrentPassword), _localizer["ChangePassword_CurrentPasswordIncorrect"]);
            return View(model);
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, model.NewPassword);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = _localizer["ChangePassword_Success"].Value;
        return RedirectToAction(nameof(ChangePassword));
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

    private int GetCurrentUserId()
    {
        string? claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(claimValue, out int userId))
        {
            return userId;
        }

        return HttpContext.Session.GetInt32("UserId") ?? 0;
    }

    private void ReplaceLocalizedValidationMessages()
    {
        ReplaceValidationMessage(nameof(ChangePasswordViewModel.CurrentPassword), "CurrentPasswordRequired");
        ReplaceValidationMessage(nameof(ChangePasswordViewModel.NewPassword), "NewPasswordRequired");
        ReplaceValidationMessage(nameof(ChangePasswordViewModel.ConfirmPassword), "ConfirmPasswordRequired");
        ReplaceValidationMessage(nameof(ChangePasswordViewModel.NewPassword), "NewPasswordMinLength");
        ReplaceValidationMessage(nameof(ChangePasswordViewModel.ConfirmPassword), "ChangePassword_PasswordsDoNotMatch");
    }

    private void ReplaceValidationMessage(string key, string localizedKey)
    {
        if (!ModelState.TryGetValue(key, out var entry))
        {
            return;
        }

        for (int i = 0; i < entry.Errors.Count; i++)
        {
            if (entry.Errors[i].ErrorMessage == localizedKey)
            {
                entry.Errors[i] = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelError(_localizer[localizedKey]);
            }
        }
    }

    private static string NormalizeEmail(string email)
    {
        return string.IsNullOrWhiteSpace(email) ? string.Empty : email.Trim().ToLowerInvariant();
    }
}
