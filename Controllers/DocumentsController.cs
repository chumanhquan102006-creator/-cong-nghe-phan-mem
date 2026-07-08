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
public class DocumentsController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly PdfTextExtractionService _pdfTextExtractionService;
    private readonly DocumentSummaryService _documentSummaryService;
    private readonly IAIService _aiService;
    private readonly IFileValidationService _fileValidationService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DocumentsController(
        AppDbContext context,
        IWebHostEnvironment environment,
        PdfTextExtractionService pdfTextExtractionService,
        DocumentSummaryService documentSummaryService,
        IAIService aiService,
        IFileValidationService fileValidationService,
        IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _environment = environment;
        _pdfTextExtractionService = pdfTextExtractionService;
        _documentSummaryService = documentSummaryService;
        _aiService = aiService;
        _fileValidationService = fileValidationService;
        _localizer = localizer;
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
        FileValidationResult validationResult = _fileValidationService.ValidatePdf(file);
        if (!validationResult.IsValid)
        {
            string message = _localizer[validationResult.ErrorMessageKey];
            TempData["SuccessMessage"] = _localizer["PdfUploadSuccess"].Value;
            ModelState.AddModelError(nameof(model.File), message);
            return View(model);
        }

        string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "documents");
        Directory.CreateDirectory(uploadFolder);

        string storedFileName = $"{Guid.NewGuid()}{validationResult.SafeExtension}";
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

        TempData["SuccessMessage"] = _localizer["PDF uploaded successfully."].Value;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, string? tab)
    {
        int userId = GetCurrentUserId();

        var document = await _context.Documents
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

        if (document == null)
        {
            return NotFound();
        }

        var messages = await _context.DocumentChatMessages
            .Where(message => message.DocumentId == id && message.UserId == userId)
            .OrderBy(message => message.CreatedAt)
            .ToListAsync();

        return View(new DocumentWorkspaceViewModel
        {
            Document = document,
            Messages = messages,
            ActiveTab = DocumentWorkspaceViewModel.NormalizeTab(tab),
            FileExists = System.IO.File.Exists(GetPhysicalPath(document.StoredFileName))
        });
    }

    [HttpGet]
    public async Task<IActionResult> ExportSummary(int id)
    {
        Document? document = await FindOwnedDocumentAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(document.Summary))
        {
            return NotFound();
        }

        DateTime createdAt = document.SummaryGeneratedAt ?? document.UploadedAt;
        return this.TxtFile("document-summary", document.Summary, createdAt);
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
            TempData["ErrorMessage"] = _localizer["Documents_PdfMissingOnServer"].Value;
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
            TempData["SuccessMessage"] = _localizer["Text extracted successfully."].Value;
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
            TempData["ErrorMessage"] = _localizer["Documents_ExtractBeforeSummary"].Value;
            return RedirectToAction(nameof(Details), new { id, tab = DocumentWorkspaceViewModel.SummaryTab });
        }

        string summary;
        bool usedAi = false;

        int userId = GetCurrentUserId();

        UserAISetting? aiSetting = await _context.UserAISettings
            .AsNoTracking()
            .FirstOrDefaultAsync(setting => setting.UserId == userId);

        if (aiSetting != null && aiSetting.IsEnabled && !string.IsNullOrWhiteSpace(aiSetting.ApiKey))
        {
            try
            {
                summary = await _aiService.GenerateDocumentSummaryAsync(document.ExtractedText, aiSetting);
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
            TempData["ErrorMessage"] = _localizer["ExtractTextBeforeSummary"].Value;
        }
        else
        {
            if (usedAi)
            {
                TempData["SuccessMessage"] = _localizer["Documents_SummaryGeneratedByAi"].Value;
            }
            else
            {
                TempData["WarningMessage"] = _localizer["Documents_SummaryGeneratedFallback"].Value;
            }
        }

        return RedirectToAction(nameof(Details), new { id, tab = DocumentWorkspaceViewModel.SummaryTab });
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

        TempData["SuccessMessage"] = _localizer["Document deleted successfully."].Value;
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
