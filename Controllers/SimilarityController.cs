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
public class SimilarityController : Controller
{
    private readonly AppDbContext _context;
    private readonly SimilarityCheckerService _similarityCheckerService;

    public SimilarityController(AppDbContext context, SimilarityCheckerService similarityCheckerService)
    {
        _context = context;
        _similarityCheckerService = similarityCheckerService;
    }

    [HttpGet("Similarity/Check/{essayId:int}")]
    public async Task<IActionResult> Check(int essayId)
    {
        var essay = await FindOwnedEssayAsync(essayId);
        if (essay == null)
        {
            TempData["ErrorMessage"] = "Essay not found or you do not have permission to access it.";
            return NotFound();
        }

        int userId = GetCurrentUserId();
        int extractedDocumentCount = await _context.Documents
            .CountAsync(document => document.UserId == userId && !string.IsNullOrWhiteSpace(document.ExtractedText));

        return View(new SimilarityCheckViewModel
        {
            Essay = essay,
            ExtractedDocumentCount = extractedDocumentCount
        });
    }

    [HttpPost("Similarity/Check/{essayId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckPost(int essayId)
    {
        var essay = await FindOwnedEssayAsync(essayId);
        if (essay == null)
        {
            TempData["ErrorMessage"] = "Essay not found or you do not have permission to access it.";
            return NotFound();
        }

        int userId = GetCurrentUserId();
        var documents = await _context.Documents
            .Where(document => document.UserId == userId && !string.IsNullOrWhiteSpace(document.ExtractedText))
            .ToListAsync();

        if (documents.Count == 0)
        {
            TempData["WarningMessage"] = "No extracted documents available for similarity checking.";
            return View("Check", new SimilarityCheckViewModel
            {
                Essay = essay,
                ExtractedDocumentCount = 0,
                ErrorMessage = "No extracted documents available for similarity checking."
            });
        }

        SimilarityCheck check = _similarityCheckerService.CheckSimilarity(essay.Content, documents);
        check.EssayId = essay.Id;

        _context.SimilarityChecks.Add(check);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Similarity check completed.";
        return RedirectToAction(nameof(Result), new { id = check.Id });
    }

    [HttpGet("Similarity/Result/{id:int}")]
    public async Task<IActionResult> Result(int id)
    {
        int userId = GetCurrentUserId();

        var check = await _context.SimilarityChecks
            .Include(item => item.Essay)
            .Include(item => item.Matches)
            .ThenInclude(match => match.Document)
            .FirstOrDefaultAsync(item => item.Id == id && item.Essay != null && item.Essay.UserId == userId);

        if (check == null)
        {
            TempData["ErrorMessage"] = "Similarity result not found or you do not have permission to access it.";
            return NotFound();
        }

        return View(check);
    }

    private async Task<Essay?> FindOwnedEssayAsync(int essayId)
    {
        int userId = GetCurrentUserId();

        return await _context.Essays
            .FirstOrDefaultAsync(item => item.Id == essayId && item.UserId == userId);
    }

    private int GetCurrentUserId()
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdValue!);
    }
}
