using System.Security.Claims;
using AcademicAIAssistant.Data;
using AcademicAIAssistant.Models;
using AcademicAIAssistant.Models.ViewModels;
using AcademicAIAssistant.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicAIAssistant.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly AppDbContext _context;
    private readonly IAIService _aiService;

    public SettingsController(AppDbContext context, IAIService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        int userId = GetCurrentUserId();
        UserAISetting? setting = await _context.UserAISettings
            .FirstOrDefaultAsync(item => item.UserId == userId);

        var model = new AISettingsViewModel
        {
            Provider = setting?.Provider ?? "Gemini",
            ModelName = setting?.ModelName ?? "gemini-1.5-flash",
            IsEnabled = setting?.IsEnabled ?? false,
            MaskedApiKey = MaskApiKey(setting?.ApiKey),
            HasApiKey = !string.IsNullOrWhiteSpace(setting?.ApiKey)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAISettings(AISettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        int userId = GetCurrentUserId();
        UserAISetting? setting = await _context.UserAISettings
            .FirstOrDefaultAsync(item => item.UserId == userId);

        if (setting == null)
        {
            setting = new UserAISetting
            {
                UserId = userId,
                CreatedAt = DateTime.Now
            };
            _context.UserAISettings.Add(setting);
        }

        setting.Provider = model.Provider;
        setting.ModelName = model.ModelName;
        setting.IsEnabled = model.IsEnabled;
        setting.UpdatedAt = DateTime.Now;

        if (!string.IsNullOrWhiteSpace(model.ApiKey))
        {
            setting.ApiKey = model.ApiKey.Trim();
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "AI settings saved successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestAIConnection()
    {
        int userId = GetCurrentUserId();
        UserAISetting? setting = await _context.UserAISettings
            .FirstOrDefaultAsync(item => item.UserId == userId);

        if (setting == null)
        {
            TempData["ErrorMessage"] = "AI settings not found. Please save your settings first.";
            return RedirectToAction(nameof(Index));
        }

        if (!setting.IsEnabled)
        {
            TempData["WarningMessage"] = "AI mode is disabled. Enable AI mode before testing.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(setting.ApiKey))
        {
            TempData["ErrorMessage"] = "API key is missing.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            bool isConnected = await _aiService.TestConnectionAsync(setting);
            TempData["SuccessMessage"] = isConnected
                ? "AI connection test succeeded."
                : "AI connection test failed. Empty response returned.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private int GetCurrentUserId()
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdValue!);
    }

    private static string MaskApiKey(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return "Not set";
        }

        string suffix = apiKey.Length <= 4 ? apiKey : apiKey[^4..];
        return $"{apiKey[..Math.Min(3, apiKey.Length)]}-****{suffix}";
    }
}
