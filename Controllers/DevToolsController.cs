using AcademicAIAssistant.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AcademicAIAssistant.Controllers;

[Authorize]
public class DevToolsController : Controller
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DevToolsController> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DevToolsController(
        IWebHostEnvironment environment,
        IConfiguration configuration,
        ILogger<DevToolsController> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
        _localizer = localizer;
    }

    [HttpGet]
    public IActionResult Index()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        SetViewData();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetDemoData()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        bool seedEnabled = _configuration.GetValue("SeedData:Enabled", true);
        bool resetAllowed = _configuration.GetValue("SeedData:AllowReset", false);

        if (!seedEnabled || !resetAllowed)
        {
            TempData["WarningMessage"] = _localizer["DevTools_ResetDisabled"].Value;
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await DbInitializer.ResetDemoDataAsync(HttpContext.RequestServices);
            TempData["SuccessMessage"] = _localizer["DevTools_ResetSuccess"].Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset demo data.");
            TempData["ErrorMessage"] = _localizer["DevTools_ResetError"].Value;
        }

        return RedirectToAction(nameof(Index));
    }

    private void SetViewData()
    {
        ViewData["EnvironmentName"] = _environment.EnvironmentName;
        ViewData["SeedEnabled"] = _configuration.GetValue("SeedData:Enabled", true);
        ViewData["AllowReset"] = _configuration.GetValue("SeedData:AllowReset", false);
        ViewData["DemoEmail"] = DbInitializer.DemoEmail;
        ViewData["DemoPassword"] = DbInitializer.DemoPassword;
    }
}
