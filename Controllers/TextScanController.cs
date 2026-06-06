using System.Security.Claims;
using System.Text.RegularExpressions;
using AcademicAIAssistant.Data;
using AcademicAIAssistant.Models;
using AcademicAIAssistant.Models.ViewModels;
using AcademicAIAssistant.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicAIAssistant.Controllers;

[Authorize]
public class TextScanController : Controller
{
    private readonly AppDbContext _context;
    private readonly TextScanService _textScanService;

    public TextScanController(AppDbContext context, TextScanService textScanService)
    {
        _context = context;
        _textScanService = textScanService;
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
                TempData["ErrorMessage"] = "OCR scan not found or you do not have permission to access it.";
                return View(new TextScanInputViewModel());
            }

            if (string.IsNullOrWhiteSpace(scan.ExtractedText))
            {
                TempData["WarningMessage"] = "No OCR text available to scan.";
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
                TempData["ErrorMessage"] = "Essay not found or you do not have permission to access it.";
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
            TempData["ErrorMessage"] = "Please enter at least 50 words for a meaningful scan.";
            ModelState.AddModelError(nameof(model.InputText), "Please enter at least 50 words for a meaningful scan.");
            return View("Index", model);
        }

        try
        {
            TextScan scan = await _textScanService.ScanTextAsync(GetCurrentUserId(), model.Title, model.InputText);
            TempData["SuccessMessage"] = "Text scan completed successfully.";
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

        TempData["SuccessMessage"] = "Text scan deleted successfully.";
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
