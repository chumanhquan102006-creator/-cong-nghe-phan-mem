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
public class OCRController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly OCRService _ocrService;
    private readonly IFileValidationService _fileValidationService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public OCRController(
        AppDbContext context,
        IWebHostEnvironment environment,
        OCRService ocrService,
        IFileValidationService fileValidationService,
        IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _environment = environment;
        _ocrService = ocrService;
        _fileValidationService = fileValidationService;
        _localizer = localizer;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new OCRUploadViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(OCRUploadViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        IFormFile file = model.ImageFile!;
        FileValidationResult validationResult = _fileValidationService.ValidateImage(file);
        if (!validationResult.IsValid)
        {
            string message = _localizer[validationResult.ErrorMessageKey];
            TempData["ErrorMessage"] = message;
            ModelState.AddModelError(nameof(model.ImageFile), message);
            return View("Index", model);
        }

        string language = model.Language is "eng" or "vie" ? model.Language : "eng";
        string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "ocr");
        Directory.CreateDirectory(uploadFolder);

        string storedFileName = $"{Guid.NewGuid()}{validationResult.SafeExtension}";
        string fullPath = Path.Combine(uploadFolder, storedFileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        string extractedText = string.Empty;
        bool ocrSucceeded = true;

        try
        {
            extractedText = _ocrService.ExtractTextFromImage(fullPath, language);
            if (string.IsNullOrWhiteSpace(extractedText))
            {
                ocrSucceeded = false;
                TempData["WarningMessage"] = _localizer["No readable text was detected."];
            }
        }
        catch (Exception ex)
        {
            ocrSucceeded = false;
            TempData["ErrorMessage"] = ex.Message;
        }

        var scan = new OCRScan
        {
            UserId = GetCurrentUserId(),
            Title = model.Title,
            OriginalFileName = Path.GetFileName(file.FileName),
            StoredFileName = storedFileName,
            FilePath = $"/uploads/ocr/{storedFileName}",
            ContentType = file.ContentType,
            FileSize = file.Length,
            ExtractedText = extractedText,
            Language = language,
            CreatedAt = DateTime.Now
        };

        _context.OCRScans.Add(scan);
        await _context.SaveChangesAsync();

        if (ocrSucceeded)
        {
            TempData["SuccessMessage"] = _localizer["OCR_ImageScannedSuccessfully"];
        }

        return RedirectToAction(nameof(Details), new { id = scan.Id });
    }

    [HttpGet]
    public async Task<IActionResult> History()
    {
        int userId = GetCurrentUserId();

        var scans = await _context.OCRScans
            .Where(scan => scan.UserId == userId)
            .OrderByDescending(scan => scan.CreatedAt)
            .ToListAsync();

        return View(scans);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        OCRScan? scan = await FindOwnedScanAsync(id);
        if (scan == null)
        {
            return Forbid();
        }

        ViewData["FileExists"] = System.IO.File.Exists(GetPhysicalPath(scan.StoredFileName));
        return View(scan);
    }

    [HttpGet]
    public async Task<IActionResult> ExportText(int id)
    {
        OCRScan? scan = await FindOwnedScanAsync(id);
        if (scan == null)
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(scan.ExtractedText))
        {
            return NotFound();
        }

        return this.TxtFile("ocr-result", scan.ExtractedText, scan.CreatedAt);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        OCRScan? scan = await FindOwnedScanAsync(id);
        if (scan == null)
        {
            return Forbid();
        }

        string fullPath = GetPhysicalPath(scan.StoredFileName);
        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }

        _context.OCRScans.Remove(scan);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = _localizer["OCR_DeleteSuccess"];
        return RedirectToAction(nameof(History));
    }

    private async Task<OCRScan?> FindOwnedScanAsync(int id)
    {
        int userId = GetCurrentUserId();

        return await _context.OCRScans
            .FirstOrDefaultAsync(scan => scan.Id == id && scan.UserId == userId);
    }

    private string GetPhysicalPath(string storedFileName)
    {
        return Path.Combine(_environment.WebRootPath, "uploads", "ocr", storedFileName);
    }

    private int GetCurrentUserId()
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdValue!);
    }
}
