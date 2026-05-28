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
public class DocumentsController : Controller
{
    private const long MaxFileSize = 10 * 1024 * 1024;
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly PdfTextExtractionService _pdfTextExtractionService;
    private readonly DocumentSummaryService _documentSummaryService;
    private readonly IAIService _aiService;

    public DocumentsController(
        AppDbContext context,
        IWebHostEnvironment environment,
        PdfTextExtractionService pdfTextExtractionService,
        DocumentSummaryService documentSummaryService,
        IAIService aiService)
    {
        _context = context;
        _environment = environment;
        _pdfTextExtractionService = pdfTextExtractionService;
        _documentSummaryService = documentSummaryService;
        _aiService = aiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        int userId = GetCurrentUserId();

        var documents = await _context.Documents
            .Where(document => document.UserId == userId)
            .OrderByDescending(document => document.UploadedAt)
            .ToListAsync();

        return View(documents);
    }

    [HttpGet]
    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(UploadDocumentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        IFormFile file = model.File!;
        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (file.Length == 0)
        {
            TempData["ErrorMessage"] = "Please choose a non-empty PDF file.";
            ModelState.AddModelError(nameof(model.File), "Please choose a non-empty PDF file.");
            return View(model);
        }

        if (file.Length > MaxFileSize)
        {
            TempData["ErrorMessage"] = "File size must be 10MB or smaller.";
            ModelState.AddModelError(nameof(model.File), "File size must be 10MB or smaller.");
            return View(model);
        }

        if (extension != ".pdf")
        {
            TempData["ErrorMessage"] = "Only .pdf files are allowed.";
            ModelState.AddModelError(nameof(model.File), "Only .pdf files are allowed.");
            return View(model);
        }

        if (!string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Invalid content type. Please upload a PDF file.";
            ModelState.AddModelError(nameof(model.File), "Invalid content type. Please upload a PDF file.");
            return View(model);
        }

        string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "documents");
        Directory.CreateDirectory(uploadFolder);

        string storedFileName = $"{Guid.NewGuid()}.pdf";
        string fullPath = Path.Combine(uploadFolder, storedFileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var document = new Document
        {
            UserId = GetCurrentUserId(),
            Title = model.Title,
            OriginalFileName = Path.GetFileName(file.FileName),
            StoredFileName = storedFileName,
            FilePath = $"/uploads/documents/{storedFileName}",
            FileSize = file.Length,
            ContentType = file.ContentType,
            UploadedAt = DateTime.Now
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "PDF uploaded successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        int userId = GetCurrentUserId();

        var document = await _context.Documents
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

        if (document == null)
        {
            return NotFound();
        }

        ViewData["FileExists"] = System.IO.File.Exists(GetPhysicalPath(document.StoredFileName));
        return View(document);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExtractText(int id)
    {
        var document = await FindOwnedDocumentAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        string fullPath = GetPhysicalPath(document.StoredFileName);
        if (!System.IO.File.Exists(fullPath))
        {
            TempData["ErrorMessage"] = "The PDF file no longer exists on the server.";
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            PdfTextExtractionResult result = _pdfTextExtractionService.ExtractText(fullPath);

            document.ExtractedText = result.Text;
            document.ExtractedAt = DateTime.Now;
            document.WordCount = result.WordCount;
            document.PageCount = result.PageCount;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Text extracted successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateSummary(int id)
    {
        var document = await FindOwnedDocumentAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(document.ExtractedText))
        {
            TempData["ErrorMessage"] = "Please extract text before generating a summary.";
            return RedirectToAction(nameof(Details), new { id });
        }

        string summary;
        bool usedAi = false;

        if (_aiService.IsEnabled)
        {
            try
            {
                summary = await _aiService.GenerateDocumentSummaryAsync(document.ExtractedText);
                usedAi = true;
            }
            catch
            {
                summary = _documentSummaryService.GenerateMockSummary(document.ExtractedText);
            }
        }
        else
        {
            summary = _documentSummaryService.GenerateMockSummary(document.ExtractedText);
        }

        document.Summary = summary;
        document.SummaryGeneratedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        if (summary == "Not enough text to summarize.")
        {
            TempData["ErrorMessage"] = summary;
        }
        else
        {
            TempData["SuccessMessage"] = usedAi
                ? "AI summary generated successfully."
                : "Rule-based fallback summary generated successfully.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        int userId = GetCurrentUserId();

        var document = await _context.Documents
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

        if (document == null)
        {
            return NotFound();
        }

        string fullPath = GetPhysicalPath(document.StoredFileName);
        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }

        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Document deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private int GetCurrentUserId()
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdValue!);
    }

    private string GetPhysicalPath(string storedFileName)
    {
        return Path.Combine(_environment.WebRootPath, "uploads", "documents", storedFileName);
    }

    private async Task<Document?> FindOwnedDocumentAsync(int id)
    {
        int userId = GetCurrentUserId();

        return await _context.Documents
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);
    }
}
