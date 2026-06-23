using System.Security.Claims;
using AcademicAIAssistant.Data;
using AcademicAIAssistant.Helpers;
using AcademicAIAssistant.Models;
using AcademicAIAssistant.Models.ViewModels;
using AcademicAIAssistant.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace AcademicAIAssistant.Controllers;

[Authorize]
public class WritingCoachController : Controller
{
    private readonly AppDbContext _context;
    private readonly IAIService _aiService;
    private readonly WritingCoachFallbackService _fallbackService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public WritingCoachController(
        AppDbContext context,
        IAIService aiService,
        WritingCoachFallbackService fallbackService,
        IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _aiService = aiService;
        _fallbackService = fallbackService;
        _localizer = localizer;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new WritingCoachViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(WritingCoachViewModel model)
    {
        ValidateCoachInput(model);
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        int userId = GetCurrentUserId();
        UserAISetting? setting = await _context.UserAISettings
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.UserId == userId);

        string response;
        bool usedAi = false;

        if (setting != null && setting.IsEnabled && !string.IsNullOrWhiteSpace(setting.ApiKey))
        {
            try
            {
                response = await _aiService.GenerateWritingCoachResponseAsync(
                    model.Mode,
                    model.Topic,
                    model.EssayType,
                    model.ThesisStatement,
                    model.UserInput,
                    model.Language,
                    setting);
                usedAi = true;
            }
            catch
            {
                response = _fallbackService.GenerateResponse(
                    model.Mode,
                    model.Topic,
                    model.EssayType,
                    model.ThesisStatement,
                    model.UserInput,
                    model.Language);
            }
        }
        else
        {
            response = _fallbackService.GenerateResponse(
                model.Mode,
                model.Topic,
                model.EssayType,
                model.ThesisStatement,
                model.UserInput,
                model.Language);
        }

        var session = new WritingCoachSession
        {
            UserId = userId,
            Mode = model.Mode,
            Topic = model.Topic,
            EssayType = model.EssayType,
            ThesisStatement = model.ThesisStatement,
            UserInput = model.UserInput,
            AIResponse = response,
            Language = model.Language,
            CreatedAt = DateTime.Now
        };

        _context.WritingCoachSessions.Add(session);
        await _context.SaveChangesAsync();

        TempData[usedAi ? "SuccessMessage" : "WarningMessage"] = usedAi
            ? _localizer["WritingCoach_AiResponseSuccess"].Value
            : _localizer["WritingCoach_AiResponseFallback"].Value;

        return RedirectToAction(nameof(Details), new { id = session.Id });
    }

    [HttpGet]
    public async Task<IActionResult> History()
    {
        int userId = GetCurrentUserId();

        var sessions = await _context.WritingCoachSessions
            .Where(session => session.UserId == userId)
            .OrderByDescending(session => session.CreatedAt)
            .ToListAsync();

        return View(sessions);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        WritingCoachSession? session = await FindOwnedSessionAsync(id);
        if (session == null)
        {
            return Forbid();
        }

        return View(session);
    }

    [HttpGet]
    public async Task<IActionResult> ExportResponse(int id)
    {
        WritingCoachSession? session = await FindOwnedSessionAsync(id);
        if (session == null)
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(session.AIResponse))
        {
            return NotFound();
        }

        return this.TxtFile("writing-coach-response", session.AIResponse, session.CreatedAt);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendToWriting(int id)
    {
        WritingCoachSession? session = await FindOwnedSessionAsync(id);
        if (session == null)
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(session.AIResponse))
        {
            TempData["WarningMessage"] = _localizer["Writing_NoCoachResponseAvailable"];
            return RedirectToAction(nameof(Details), new { id });
        }

        return RedirectToAction("Index", "Writing", new { prefillFromCoachId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        WritingCoachSession? session = await FindOwnedSessionAsync(id);
        if (session == null)
        {
            return Forbid();
        }

        _context.WritingCoachSessions.Remove(session);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = _localizer["WritingCoach_DeleteSuccess"];

        return RedirectToAction(nameof(History));
    }

    private void ValidateCoachInput(WritingCoachViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Topic) && string.IsNullOrWhiteSpace(model.UserInput))
        {
            ModelState.AddModelError(nameof(model.Topic), _localizer["Validation_WritingCoachTopicOrInstructionsRequired"]);
        }

        if (model.Mode == "ThesisImprover" && string.IsNullOrWhiteSpace(model.ThesisStatement))
        {
            ModelState.AddModelError(nameof(model.ThesisStatement), _localizer["Validation_WritingCoachThesisRequired"]);
        }

        if (model.Mode == "CounterArgument"
            && string.IsNullOrWhiteSpace(model.ThesisStatement)
            && string.IsNullOrWhiteSpace(model.UserInput))
        {
            ModelState.AddModelError(nameof(model.ThesisStatement), _localizer["Validation_WritingCoachThesisOrArgumentRequired"]);
        }
    }

    private async Task<WritingCoachSession?> FindOwnedSessionAsync(int id)
    {
        int userId = GetCurrentUserId();

        return await _context.WritingCoachSessions
            .FirstOrDefaultAsync(session => session.Id == id && session.UserId == userId);
    }

    private int GetCurrentUserId()
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdValue!);
    }
}
