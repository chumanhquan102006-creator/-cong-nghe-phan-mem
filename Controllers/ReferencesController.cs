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
public class ReferencesController : Controller
{
    private readonly AppDbContext _context;
    private readonly ReferenceGeneratorService _referenceGeneratorService;

    public ReferencesController(AppDbContext context, ReferenceGeneratorService referenceGeneratorService)
    {
        _context = context;
        _referenceGeneratorService = referenceGeneratorService;
    }

    [HttpGet("/References")]
    public async Task<IActionResult> Index(string? search, string? sourceType)
    {
        int userId = GetCurrentUserId();
        IQueryable<ReferenceItem> query = _context.ReferenceItems
            .Where(reference => reference.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(reference =>
                reference.Title.Contains(search)
                || reference.Author.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(sourceType))
        {
            query = query.Where(reference => reference.SourceType == sourceType);
        }

        ViewData["Search"] = search;
        ViewData["SourceType"] = sourceType;

        var references = await query
            .OrderByDescending(reference => reference.CreatedAt)
            .ToListAsync();

        return View(references);
    }

    [HttpGet("/References/Create")]
    public IActionResult Create()
    {
        return View(new ReferenceItemFormViewModel());
    }

    [HttpPost("/References/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReferenceItemFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var reference = ToReferenceItem(model);
        reference.UserId = GetCurrentUserId();
        reference.CreatedAt = DateTime.Now;
        _referenceGeneratorService.GenerateAllCitationFormats(reference);

        _context.ReferenceItems.Add(reference);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Reference created successfully.";
        return RedirectToAction(nameof(Details), new { id = reference.Id });
    }

    [HttpGet("/References/Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        ReferenceItem? reference = await FindOwnedReferenceAsync(id);
        if (reference == null)
        {
            return Forbid();
        }

        return View(reference);
    }

    [HttpGet("/References/Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        ReferenceItem? reference = await FindOwnedReferenceAsync(id);
        if (reference == null)
        {
            return Forbid();
        }

        return View(ToFormViewModel(reference));
    }

    [HttpPost("/References/Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ReferenceItemFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        ReferenceItem? reference = await FindOwnedReferenceAsync(id);
        if (reference == null)
        {
            return Forbid();
        }

        UpdateReferenceItem(reference, model);
        reference.UpdatedAt = DateTime.Now;
        _referenceGeneratorService.GenerateAllCitationFormats(reference);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Reference updated successfully.";
        return RedirectToAction(nameof(Details), new { id = reference.Id });
    }

    [HttpPost("/References/Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        ReferenceItem? reference = await FindOwnedReferenceAsync(id);
        if (reference == null)
        {
            return Forbid();
        }

        _context.ReferenceItems.Remove(reference);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Reference deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/References/Export")]
    public async Task<IActionResult> Export()
    {
        int userId = GetCurrentUserId();
        var references = await _context.ReferenceItems
            .Where(reference => reference.UserId == userId)
            .OrderBy(reference => reference.Author)
            .ThenBy(reference => reference.Title)
            .ToListAsync();

        return View(references);
    }

    [HttpPost("/References/SendToWriting/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendToWriting(int id)
    {
        ReferenceItem? reference = await FindOwnedReferenceAsync(id);
        if (reference == null)
        {
            return Forbid();
        }

        return RedirectToAction("Index", "Writing", new { insertReferenceId = id });
    }

    private async Task<ReferenceItem?> FindOwnedReferenceAsync(int id)
    {
        int userId = GetCurrentUserId();
        return await _context.ReferenceItems
            .FirstOrDefaultAsync(reference => reference.Id == id && reference.UserId == userId);
    }

    private int GetCurrentUserId()
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdValue!);
    }

    private static ReferenceItem ToReferenceItem(ReferenceItemFormViewModel model)
    {
        var item = new ReferenceItem();
        UpdateReferenceItem(item, model);
        return item;
    }

    private static void UpdateReferenceItem(ReferenceItem item, ReferenceItemFormViewModel model)
    {
        item.SourceType = model.SourceType;
        item.Title = model.Title.Trim();
        item.Author = model.Author?.Trim() ?? string.Empty;
        item.Year = model.Year?.Trim() ?? string.Empty;
        item.JournalOrPublisher = model.JournalOrPublisher?.Trim() ?? string.Empty;
        item.WebsiteName = model.WebsiteName?.Trim() ?? string.Empty;
        item.Url = model.Url?.Trim() ?? string.Empty;
        item.Doi = model.Doi?.Trim() ?? string.Empty;
        item.Volume = model.Volume?.Trim() ?? string.Empty;
        item.Issue = model.Issue?.Trim() ?? string.Empty;
        item.Pages = model.Pages?.Trim() ?? string.Empty;
        item.AccessDate = model.AccessDate;
        item.Notes = model.Notes?.Trim() ?? string.Empty;
    }

    private static ReferenceItemFormViewModel ToFormViewModel(ReferenceItem reference)
    {
        return new ReferenceItemFormViewModel
        {
            SourceType = reference.SourceType,
            Title = reference.Title,
            Author = reference.Author,
            Year = reference.Year,
            JournalOrPublisher = reference.JournalOrPublisher,
            WebsiteName = reference.WebsiteName,
            Url = reference.Url,
            Doi = reference.Doi,
            Volume = reference.Volume,
            Issue = reference.Issue,
            Pages = reference.Pages,
            AccessDate = reference.AccessDate,
            Notes = reference.Notes
        };
    }
}
