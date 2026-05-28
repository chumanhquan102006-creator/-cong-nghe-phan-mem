using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AcademicAIAssistant.Data;
using AcademicAIAssistant.Models;
using AcademicAIAssistant.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AcademicAIAssistant.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
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

        var recentCitationChecks = await _context.CitationChecks
            .Include(check => check.Essay)
            .Where(check => check.Essay != null && check.Essay.UserId == userId)
            .OrderByDescending(check => check.CheckedAt)
            .Take(5)
            .ToListAsync();

        var recentSimilarityChecks = await _context.SimilarityChecks
            .Include(check => check.Essay)
            .Where(check => check.Essay != null && check.Essay.UserId == userId)
            .OrderByDescending(check => check.CheckedAt)
            .Take(5)
            .ToListAsync();

        var recentChatMessages = await _context.DocumentChatMessages
            .Include(message => message.Document)
            .Where(message => message.UserId == userId)
            .OrderByDescending(message => message.CreatedAt)
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
            RecentEssays = recentEssays,
            RecentCitationChecks = recentCitationChecks,
            RecentSimilarityChecks = recentSimilarityChecks,
            RecentChatMessages = recentChatMessages
        };

        viewModel.RecentActivities = BuildRecentActivities(
            recentDocuments,
            recentEssays,
            recentCitationChecks,
            recentSimilarityChecks,
            recentChatMessages);

        return View(viewModel);
    }

    private int GetCurrentUserId()
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdValue!);
    }

    private static List<RecentActivityViewModel> BuildRecentActivities(
        List<Document> documents,
        List<Essay> essays,
        List<CitationCheck> citationChecks,
        List<SimilarityCheck> similarityChecks,
        List<DocumentChatMessage> chatMessages)
    {
        var activities = new List<RecentActivityViewModel>();

        activities.AddRange(documents.Select(document => new RecentActivityViewModel
        {
            ActivityType = "Document",
            Title = document.Title,
            Description = $"Uploaded document: {document.OriginalFileName}",
            CreatedAt = document.UploadedAt,
            LinkUrl = $"/Documents/Details/{document.Id}",
            BadgeClass = "text-bg-primary"
        }));

        activities.AddRange(essays.Select(essay => new RecentActivityViewModel
        {
            ActivityType = "Essay",
            Title = essay.Title,
            Description = $"Analyzed {essay.EssayType} with {essay.WordCount} words",
            CreatedAt = essay.CreatedAt,
            LinkUrl = $"/Writing/Details/{essay.Id}",
            BadgeClass = "text-bg-success"
        }));

        activities.AddRange(citationChecks.Select(check => new RecentActivityViewModel
        {
            ActivityType = "Citation",
            Title = check.Essay?.Title ?? "Citation check",
            Description = $"Citation status: {check.OverallStatus}",
            CreatedAt = check.CheckedAt,
            LinkUrl = $"/Citation/Result/{check.Id}",
            BadgeClass = "text-bg-warning"
        }));

        activities.AddRange(similarityChecks.Select(check => new RecentActivityViewModel
        {
            ActivityType = "Similarity",
            Title = check.Essay?.Title ?? "Similarity check",
            Description = $"Similarity status: {check.Status} ({check.OverallSimilarityScore:0.#}%)",
            CreatedAt = check.CheckedAt,
            LinkUrl = $"/Similarity/Result/{check.Id}",
            BadgeClass = "text-bg-danger"
        }));

        activities.AddRange(chatMessages.Select(message => new RecentActivityViewModel
        {
            ActivityType = "Chat",
            Title = message.Document?.Title ?? "Document chat",
            Description = $"Asked: {Shorten(message.Question, 80)}",
            CreatedAt = message.CreatedAt,
            LinkUrl = $"/DocumentChat/{message.DocumentId}",
            BadgeClass = "text-bg-info"
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
