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
public class WritingController : Controller
{
    private readonly AppDbContext _context;
    private readonly WritingFeedbackService _writingFeedbackService;
    private readonly IAIService _aiService;

    public WritingController(AppDbContext context, WritingFeedbackService writingFeedbackService, IAIService aiService)
    {
        _context = context;
        _writingFeedbackService = writingFeedbackService;
        _aiService = aiService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new EssayAnalyzeViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Analyze(EssayAnalyzeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please complete the essay title, type, and content before analyzing.";
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

        TempData["SuccessMessage"] = "Essay analyzed successfully.";
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

    private int GetCurrentUserId()
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdValue!);
    }

    private async Task<FeedbackReport> AnalyzeWithAiOrFallbackAsync(EssayAnalyzeViewModel model)
    {
        if (!_aiService.IsEnabled)
        {
            return _writingFeedbackService.AnalyzeEssay(model.Title, model.EssayType, model.Content);
        }

        try
        {
            string aiFeedback = await _aiService.GenerateWritingFeedbackAsync(model.EssayType, model.Content);

            return new FeedbackReport
            {
                OverallScore = TryExtractScore(aiFeedback),
                GrammarFeedback = "See AI feedback in General Suggestions.",
                AcademicToneFeedback = "See AI feedback in General Suggestions.",
                ThesisFeedback = "See AI feedback in General Suggestions.",
                StructureFeedback = "See AI feedback in General Suggestions.",
                LogicFeedback = "See AI feedback in General Suggestions.",
                CitationFeedback = "See AI feedback in General Suggestions.",
                GeneralSuggestions = aiFeedback,
                CreatedAt = DateTime.Now
            };
        }
        catch
        {
            return _writingFeedbackService.AnalyzeEssay(model.Title, model.EssayType, model.Content);
        }
    }

    private static int TryExtractScore(string feedback)
    {
        var match = System.Text.RegularExpressions.Regex.Match(feedback, @"\b(\d{1,3})\s*/\s*100\b|\bscore\D+(\d{1,3})\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        string value = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;

        return int.TryParse(value, out int score) ? Math.Clamp(score, 0, 100) : 0;
    }
}
