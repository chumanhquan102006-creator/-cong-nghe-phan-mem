using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
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
public class TextScanController : Controller
{
    private readonly AppDbContext _context;
    private readonly TextScanService _textScanService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public TextScanController(
        AppDbContext context,
        TextScanService textScanService,
        IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _textScanService = textScanService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? prefillFromOcrId, int? prefillFromEssayId)
    {
        int userId = GetCurrentUserId();

        if (prefillFromOcrId.HasValue)
        {
            OCRScan? scan = await _context.OCRScans
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == prefillFromOcrId.Value && item.UserId == userId);

            if (scan == null)
            {
                TempData["ErrorMessage"] = _localizer["TextScan_OcrNotFoundOrForbidden"];
                return View(new TextScanInputViewModel());
            }

            if (string.IsNullOrWhiteSpace(scan.ExtractedText))
            {
                TempData["WarningMessage"] = _localizer["TextScan_NoOcrTextAvailable"];
                return View(new TextScanInputViewModel());
            }

            return View(new TextScanInputViewModel
            {
                Title = $"OCR Scan - {scan.Title}",
                InputText = scan.ExtractedText
            });
        }

        if (prefillFromEssayId.HasValue)
        {
            Essay? essay = await _context.Essays
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == prefillFromEssayId.Value && item.UserId == userId);

            if (essay == null)
            {
                TempData["ErrorMessage"] = _localizer["TextScan_EssayNotFoundOrForbidden"];
                return View(new TextScanInputViewModel());
            }

            return View(new TextScanInputViewModel
            {
                Title = $"Essay Scan - {essay.Title}",
                InputText = essay.Content
            });
        }

        return View(new TextScanInputViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Scan(TextScanInputViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        if (CountWords(model.InputText) < 50)
        {
            string message = _localizer["TextScan_MinimumWordsValidation"];
            TempData["ErrorMessage"] = message;
            ModelState.AddModelError(nameof(model.InputText), message);
            return View("Index", model);
        }

        try
        {
            TextScan scan = await _textScanService.ScanTextAsync(GetCurrentUserId(), model.Title, model.InputText);
            TempData["SuccessMessage"] = _localizer["TextScan_Success"];
            return RedirectToAction(nameof(Result), new { id = scan.Id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return View("Index", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> History()
    {
        int userId = GetCurrentUserId();

        var scans = await _context.TextScans
            .Where(scan => scan.UserId == userId)
            .OrderByDescending(scan => scan.CreatedAt)
            .ToListAsync();

        return View(scans);
    }

    [HttpGet]
    public async Task<IActionResult> Result(int id)
    {
        TextScan? scan = await _context.TextScans
            .Include(item => item.Matches.OrderByDescending(match => match.SimilarityScore))
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == GetCurrentUserId());

        if (scan == null)
        {
            return Forbid();
        }

        return View(scan);
    }

    [HttpGet]
    public async Task<IActionResult> ExportResult(int id)
    {
        TextScan? scan = await _context.TextScans
            .Include(item => item.Matches.OrderByDescending(match => match.SimilarityScore))
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == GetCurrentUserId());

        if (scan == null)
        {
            return Forbid();
        }

        var content = new StringBuilder()
            .AppendLine($"Title: {scan.Title}")
            .AppendLine($"Word count: {scan.WordCount}")
            .AppendLine($"Similarity score: {scan.OverallSimilarityScore:0.##}%")
            .AppendLine($"Risk level: {scan.RiskLevel}")
            .AppendLine()
            .AppendLine("Input Text")
            .AppendLine(scan.InputText);

        foreach (TextScanMatch match in scan.Matches.OrderByDescending(item => item.SimilarityScore))
        {
            content
                .AppendLine()
                .AppendLine($"Match: {match.SourceType} - {match.SourceTitle} ({match.SimilarityScore:0.##}%)")
                .AppendLine("Input Segment")
                .AppendLine(match.InputSegment)
                .AppendLine("Matched Source Segment")
                .AppendLine(match.MatchedSegment);
        }

        return this.TxtFile("text-scan-result", content.ToString(), scan.CreatedAt);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        TextScan? scan = await _context.TextScans
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == GetCurrentUserId());

        if (scan == null)
        {
            return Forbid();
        }

        _context.TextScans.Remove(scan);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = _localizer["TextScan_DeleteSuccess"];
        return RedirectToAction(nameof(History));
    }

    private int GetCurrentUserId()
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdValue!);
    }

    private static int CountWords(string text)
    {
        return Regex.Matches(text ?? string.Empty, @"\b[\p{L}\p{N}']+\b").Count;
    }
}
