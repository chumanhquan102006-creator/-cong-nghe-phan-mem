using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AcademicAIAssistant.Data;
using AcademicAIAssistant.Models;
using AcademicAIAssistant.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace AcademicAIAssistant.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly AppDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DashboardController(AppDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index()
    {
        int userId = GetCurrentUserId();

        var recentDocuments = await _context.Documents
            .Where(document => document.UserId == userId)
            .OrderByDescending(document => document.UploadedAt)
            .Take(5)
            .ToListAsync();

        var recentEssays = await _context.Essays
            .Where(essay => essay.UserId == userId)
            .OrderByDescending(essay => essay.CreatedAt)
            .Take(5)
            .ToListAsync();

        var recentOcrScans = await _context.OCRScans
            .Where(scan => scan.UserId == userId)
            .OrderByDescending(scan => scan.CreatedAt)
            .Take(5)
            .ToListAsync();

        var recentTextScans = await _context.TextScans
            .Where(scan => scan.UserId == userId)
            .OrderByDescending(scan => scan.CreatedAt)
            .Take(5)
            .ToListAsync();

        var recentWritingCoachSessions = await _context.WritingCoachSessions
            .Where(session => session.UserId == userId)
            .OrderByDescending(session => session.CreatedAt)
            .Take(5)
            .ToListAsync();

        var recentReferences = await _context.ReferenceItems
            .Where(reference => reference.UserId == userId)
            .OrderByDescending(reference => reference.CreatedAt)
            .Take(5)
            .ToListAsync();

        var viewModel = new DashboardViewModel
        {
            FullName = User.FindFirstValue(ClaimTypes.Name) ?? "Student",
            TotalDocuments = await _context.Documents.CountAsync(document => document.UserId == userId),
            ExtractedDocuments = await _context.Documents.CountAsync(document =>
                document.UserId == userId &&
                (document.ExtractedAt != null || !string.IsNullOrWhiteSpace(document.ExtractedText))),
            SummarizedDocuments = await _context.Documents.CountAsync(document =>
                document.UserId == userId &&
                (document.SummaryGeneratedAt != null || !string.IsNullOrWhiteSpace(document.Summary))),
            TotalEssays = await _context.Essays.CountAsync(essay => essay.UserId == userId),
            TotalFeedbackReports = await _context.FeedbackReports.CountAsync(report =>
                report.Essay != null && report.Essay.UserId == userId),
            TotalCitationChecks = await _context.CitationChecks.CountAsync(check =>
                check.Essay != null && check.Essay.UserId == userId),
            TotalSimilarityChecks = await _context.SimilarityChecks.CountAsync(check =>
                check.Essay != null && check.Essay.UserId == userId),
            TotalGraphNodes = await _context.GraphNodes.CountAsync(node => node.UserId == userId),
            TotalGraphEdges = await _context.GraphEdges.CountAsync(edge => edge.UserId == userId),
            RecentDocuments = recentDocuments,
            RecentEssays = recentEssays
        };

        viewModel.RecentActivities = BuildRecentActivities(
            recentDocuments,
            recentEssays,
            recentOcrScans,
            recentTextScans,
            recentWritingCoachSessions,
            recentReferences);

        return View(viewModel);
    }

    private int GetCurrentUserId()
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdValue!);
    }

    private List<RecentActivityViewModel> BuildRecentActivities(
        List<Document> documents,
        List<Essay> essays,
        List<OCRScan> ocrScans,
        List<TextScan> textScans,
        List<WritingCoachSession> writingCoachSessions,
        List<ReferenceItem> references)
    {
        var activities = new List<RecentActivityViewModel>();

        activities.AddRange(documents.Select(document => new RecentActivityViewModel
        {
            ActivityType = _localizer["Document Uploaded"],
            Title = document.Title,
            Description = $"{_localizer["Document Uploaded"]}: {document.OriginalFileName}",
            CreatedAt = document.UploadedAt,
            LinkUrl = $"/Documents/Details/{document.Id}",
            Icon = "bi bi-file-earmark-pdf",
            BadgeClass = "text-bg-primary"
        }));

        activities.AddRange(essays.Select(essay => new RecentActivityViewModel
        {
            ActivityType = _localizer["Essay Created"],
            Title = essay.Title,
            Description = $"{_localizer["Essay Created"]}: {essay.EssayType}",
            CreatedAt = essay.CreatedAt,
            LinkUrl = $"/Writing/Details/{essay.Id}",
            Icon = "bi bi-journal-text",
            BadgeClass = "text-bg-success"
        }));

        activities.AddRange(ocrScans.Select(scan => new RecentActivityViewModel
        {
            ActivityType = _localizer["OCR Scan Created"],
            Title = scan.Title,
            Description = $"{_localizer["OCR Scan Created"]}: {scan.OriginalFileName}",
            CreatedAt = scan.CreatedAt,
            LinkUrl = $"/OCR/Details/{scan.Id}",
            Icon = "bi bi-card-image",
            BadgeClass = "text-bg-info"
        }));

        activities.AddRange(textScans.Select(scan => new RecentActivityViewModel
        {
            ActivityType = _localizer["Text Scan Created"],
            Title = scan.Title,
            Description = $"{_localizer["Text Scan Created"]}: {scan.OverallSimilarityScore:0.#}%",
            CreatedAt = scan.CreatedAt,
            LinkUrl = $"/TextScan/Result/{scan.Id}",
            Icon = "bi bi-file-earmark-text",
            BadgeClass = "text-bg-danger"
        }));

        activities.AddRange(writingCoachSessions.Select(session => new RecentActivityViewModel
        {
            ActivityType = _localizer["Writing Coach Session Created"],
            Title = session.Topic,
            Description = $"{_localizer["Writing Coach Session Created"]}: {session.Mode}",
            CreatedAt = session.CreatedAt,
            LinkUrl = $"/WritingCoach/Details/{session.Id}",
            Icon = "bi bi-lightbulb",
            BadgeClass = "text-bg-warning"
        }));

        activities.AddRange(references.Select(reference => new RecentActivityViewModel
        {
            ActivityType = _localizer["Reference Created"],
            Title = reference.Title,
            Description = $"{_localizer["Reference Created"]}: {Shorten(reference.Author, 60)}",
            CreatedAt = reference.CreatedAt,
            LinkUrl = $"/References/Details/{reference.Id}",
            Icon = "bi bi-bookmark-plus",
            BadgeClass = "text-bg-secondary"
        }));

        return activities
            .OrderByDescending(activity => activity.CreatedAt)
            .Take(10)
            .ToList();
    }

    private static string Shorten(string text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length <= maxLength)
        {
            return text;
        }

        return text[..maxLength] + "...";
    }
}
