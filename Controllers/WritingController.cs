using System.Security.Claims;
using System.Text;
using System.Text.Json;
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
public class WritingController : Controller
{
    private readonly AppDbContext _context;
    private readonly WritingFeedbackService _writingFeedbackService;
    private readonly IAIService _aiService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public WritingController(
        AppDbContext context,
        WritingFeedbackService writingFeedbackService,
        IAIService aiService,
        IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _writingFeedbackService = writingFeedbackService;
        _aiService = aiService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? prefillFromOcrId, int? prefillFromCoachId, int? insertReferenceId)
    {
        if (insertReferenceId.HasValue)
        {
            int userId = GetCurrentUserId();
            ReferenceItem? reference = await _context.ReferenceItems
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == insertReferenceId.Value && item.UserId == userId);

            if (reference == null)
            {
                TempData["ErrorMessage"] = _localizer["Writing_ReferenceNotFoundOrForbidden"].Value;
                return View(new EssayAnalyzeViewModel());
            }

            return View(new EssayAnalyzeViewModel
            {
                Title = $"Essay with Reference - {reference.Title}",
                EssayType = "Essay",
                Content = $"""
                    In-text citation: {reference.ApaInTextCitation}

                    References
                    {reference.ApaReference}
                    """
            });
        }

        if (prefillFromCoachId.HasValue)
        {
            int userId = GetCurrentUserId();
            WritingCoachSession? session = await _context.WritingCoachSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == prefillFromCoachId.Value && item.UserId == userId);

            if (session == null)
            {
                TempData["ErrorMessage"] = _localizer["Writing_CoachSessionNotFoundOrForbidden"].Value;
                return View(new EssayAnalyzeViewModel());
            }

            if (string.IsNullOrWhiteSpace(session.AIResponse))
            {
                TempData["WarningMessage"] = _localizer["Writing_NoCoachResponseAvailable"].Value;
                return View(new EssayAnalyzeViewModel());
            }

            return View(new EssayAnalyzeViewModel
            {
                Title = $"Writing Coach - {session.Topic}",
                EssayType = MapEssayType(session.EssayType),
                Content = session.AIResponse
            });
        }

        if (prefillFromOcrId.HasValue)
        {
            int userId = GetCurrentUserId();
            OCRScan? scan = await _context.OCRScans
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == prefillFromOcrId.Value && item.UserId == userId);

            if (scan == null)
            {
                TempData["ErrorMessage"] = _localizer["Writing_OcrNotFoundOrForbidden"].Value;
                return View(new EssayAnalyzeViewModel());
            }

            if (string.IsNullOrWhiteSpace(scan.ExtractedText))
            {
                TempData["WarningMessage"] = _localizer["Writing_NoExtractedTextAvailable"].Value;
                return View(new EssayAnalyzeViewModel());
            }

            return View(new EssayAnalyzeViewModel
            {
                Title = $"OCR - {scan.Title}",
                EssayType = "Essay",
                Content = scan.ExtractedText
            });
        }

        return View(new EssayAnalyzeViewModel());
    }

    private static string MapEssayType(string essayType)
    {
        return EssayAnalyzeViewModel.EssayTypeOptions.Contains(essayType)
            ? essayType
            : "Essay";
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Analyze(EssayAnalyzeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = _localizer["Writing_CompleteRequiredFields"].Value;
            return View("Index", model);
        }

        var essay = new Essay
        {
            UserId = GetCurrentUserId(),
            Title = model.Title,
            EssayType = model.EssayType,
            Content = model.Content,
            WordCount = _writingFeedbackService.CountWords(model.Content),
            CreatedAt = DateTime.Now
        };

        FeedbackReport report = await AnalyzeWithAiOrFallbackAsync(model);
        essay.FeedbackReports.Add(report);

        _context.Essays.Add(essay);
        await _context.SaveChangesAsync();

        if (!TempData.ContainsKey("SuccessMessage") && !TempData.ContainsKey("WarningMessage"))
        {
            TempData["SuccessMessage"] = _localizer["Essay analyzed successfully."].Value;
        }

        return RedirectToAction(nameof(Details), new { id = essay.Id });
    }

    [HttpGet]
    public async Task<IActionResult> History()
    {
        int userId = GetCurrentUserId();

        var essays = await _context.Essays
            .Where(essay => essay.UserId == userId)
            .OrderByDescending(essay => essay.CreatedAt)
            .ToListAsync();

        return View(essays);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        int userId = GetCurrentUserId();

        var essay = await _context.Essays
            .Include(item => item.FeedbackReports)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

        if (essay == null)
        {
            return NotFound();
        }

        var viewModel = new EssayDetailsViewModel
        {
            Essay = essay,
            LatestFeedbackReport = essay.FeedbackReports
                .OrderByDescending(report => report.CreatedAt)
                .FirstOrDefault()
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> ExportFeedback(int id)
    {
        int userId = GetCurrentUserId();
        Essay? essay = await _context.Essays
            .Include(item => item.FeedbackReports)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

        if (essay == null)
        {
            return NotFound();
        }

        FeedbackReport? report = essay.FeedbackReports
            .OrderByDescending(item => item.CreatedAt)
            .FirstOrDefault();

        if (report == null)
        {
            return NotFound();
        }

        var content = new StringBuilder()
            .AppendLine($"Title: {essay.Title}")
            .AppendLine($"Essay type: {essay.EssayType}")
            .AppendLine($"Overall score: {report.OverallScore}")
            .AppendLine()
            .AppendLine("Grammar / Wording")
            .AppendLine(report.GrammarFeedback)
            .AppendLine()
            .AppendLine("Academic Tone")
            .AppendLine(report.AcademicToneFeedback)
            .AppendLine()
            .AppendLine("Thesis Statement")
            .AppendLine(report.ThesisFeedback)
            .AppendLine()
            .AppendLine("Structure")
            .AppendLine(report.StructureFeedback)
            .AppendLine()
            .AppendLine("Logic")
            .AppendLine(report.LogicFeedback)
            .AppendLine()
            .AppendLine("Citation")
            .AppendLine(report.CitationFeedback)
            .AppendLine()
            .AppendLine("General Suggestions")
            .AppendLine(report.GeneralSuggestions)
            .ToString();

        return this.TxtFile("writing-feedback", content, report.CreatedAt);
    }

    private int GetCurrentUserId()
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdValue!);
    }

    private async Task<FeedbackReport> AnalyzeWithAiOrFallbackAsync(EssayAnalyzeViewModel model)
    {
        int userId = GetCurrentUserId();
        UserAISetting? aiSetting = await _context.UserAISettings
            .AsNoTracking()
            .FirstOrDefaultAsync(setting => setting.UserId == userId);

        if (aiSetting == null || !aiSetting.IsEnabled || string.IsNullOrWhiteSpace(aiSetting.ApiKey))
        {
            return _writingFeedbackService.AnalyzeEssay(model.Title, model.EssayType, model.Content);
        }

        try
        {
            string aiFeedback = await _aiService.GenerateWritingFeedbackAsync(model.EssayType, model.Content, aiSetting);
            TempData["SuccessMessage"] = _localizer["Writing_AiFeedbackSuccess"].Value;
            return ParseAiFeedbackReport(aiFeedback);
        }
        catch
        {
            TempData["WarningMessage"] = _localizer["Writing_AiFeedbackFallback"].Value;
            return _writingFeedbackService.AnalyzeEssay(model.Title, model.EssayType, model.Content);
        }
    }

    private static FeedbackReport ParseAiFeedbackReport(string aiFeedback)
    {
        string json = StripJsonFence(aiFeedback);
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;

        return new FeedbackReport
        {
            OverallScore = Math.Clamp(GetInt(root, "overallScore"), 0, 100),
            GrammarFeedback = GetString(root, "grammarFeedback"),
            AcademicToneFeedback = GetString(root, "academicToneFeedback"),
            ThesisFeedback = GetString(root, "thesisFeedback"),
            StructureFeedback = GetString(root, "structureFeedback"),
            LogicFeedback = GetString(root, "logicFeedback"),
            CitationFeedback = GetString(root, "citationFeedback"),
            GeneralSuggestions = GetString(root, "generalSuggestions"),
            CreatedAt = DateTime.Now
        };
    }

    private static string StripJsonFence(string value)
    {
        string text = value.Trim();

        if (text.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            text = text[7..].Trim();
        }
        else if (text.StartsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            text = text[3..].Trim();
        }

        if (text.EndsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            text = text[..^3].Trim();
        }

        int start = text.IndexOf('{');
        int end = text.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            text = text[start..(end + 1)];
        }

        return text;
    }

    private static string GetString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out JsonElement property))
        {
            throw new JsonException($"Missing property: {propertyName}");
        }

        string? value = property.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new JsonException($"Empty property: {propertyName}");
        }

        return value.Trim();
    }

    private static int GetInt(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out JsonElement property))
        {
            throw new JsonException($"Missing property: {propertyName}");
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out int value))
        {
            return value;
        }

        if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out value))
        {
            return value;
        }

        throw new JsonException($"Invalid integer property: {propertyName}");
    }
}
