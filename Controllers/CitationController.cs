using System.Security.Claims;
using System.Text;
using AcademicAIAssistant.Data;
using AcademicAIAssistant.Helpers;
using AcademicAIAssistant.Models;
using AcademicAIAssistant.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicAIAssistant.Controllers;

[Authorize]
public class CitationController : Controller
{
    private readonly AppDbContext _context;
    private readonly CitationCheckerService _citationCheckerService;

    public CitationController(AppDbContext context, CitationCheckerService citationCheckerService)
    {
        _context = context;
        _citationCheckerService = citationCheckerService;
    }

    [HttpGet("Citation/Check/{essayId:int}")]
    public async Task<IActionResult> Check(int essayId)
    {
        var essay = await FindOwnedEssayAsync(essayId);
        if (essay == null)
        {
            TempData["ErrorMessage"] = "Essay not found or you do not have permission to access it.";
            return NotFound();
        }

        return View(essay);
    }

    [HttpPost("Citation/Check/{essayId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckPost(int essayId)
    {
        var essay = await FindOwnedEssayAsync(essayId);
        if (essay == null)
        {
            TempData["ErrorMessage"] = "Essay not found or you do not have permission to access it.";
            return NotFound();
        }

        CitationCheck check = _citationCheckerService.CheckCitation(essay.Content);
        check.EssayId = essay.Id;

        _context.CitationChecks.Add(check);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Citation check completed.";
        return RedirectToAction(nameof(Result), new { id = check.Id });
    }

    [HttpGet("Citation/Result/{id:int}")]
    public async Task<IActionResult> Result(int id)
    {
        int userId = GetCurrentUserId();

        var check = await _context.CitationChecks
            .Include(item => item.Essay)
            .FirstOrDefaultAsync(item => item.Id == id && item.Essay != null && item.Essay.UserId == userId);

        if (check == null)
        {
            TempData["ErrorMessage"] = "Citation result not found or you do not have permission to access it.";
            return NotFound();
        }

        return View(check);
    }

    [HttpGet("Citation/Export/{id:int}")]
    public async Task<IActionResult> Export(int id)
    {
        int userId = GetCurrentUserId();
        CitationCheck? check = await _context.CitationChecks
            .Include(item => item.Essay)
            .FirstOrDefaultAsync(item =>
                item.Id == id
                && item.Essay != null
                && item.Essay.UserId == userId);

        if (check == null)
        {
            return NotFound();
        }

        var content = new StringBuilder()
            .AppendLine($"Essay: {check.Essay?.Title}")
            .AppendLine($"Status: {check.OverallStatus}")
            .AppendLine($"Total in-text citations: {check.TotalInTextCitations}")
            .AppendLine($"Total references: {check.TotalReferences}")
            .AppendLine()
            .AppendLine("Missing References")
            .AppendLine(string.IsNullOrWhiteSpace(check.MissingReferences) ? "None" : check.MissingReferences)
            .AppendLine()
            .AppendLine("Unused References")
            .AppendLine(string.IsNullOrWhiteSpace(check.UnusedReferences) ? "None" : check.UnusedReferences)
            .AppendLine()
            .AppendLine("Format Issues")
            .AppendLine(string.IsNullOrWhiteSpace(check.FormatIssues) ? "None" : check.FormatIssues)
            .ToString();

        return this.TxtFile("citation-result", content, check.CheckedAt);
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
