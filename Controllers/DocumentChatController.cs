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
public class DocumentChatController : Controller
{
    private readonly AppDbContext _context;
    private readonly DocumentChatService _documentChatService;

    public DocumentChatController(AppDbContext context, DocumentChatService documentChatService)
    {
        _context = context;
        _documentChatService = documentChatService;
    }

    [HttpGet("DocumentChat/{documentId:int}")]
    public async Task<IActionResult> Index(int documentId)
    {
        var document = await FindOwnedDocumentAsync(documentId);
        if (document == null)
        {
            return NotFound();
        }

        var messages = await _context.DocumentChatMessages
            .Where(message => message.DocumentId == documentId && message.UserId == GetCurrentUserId())
            .OrderBy(message => message.CreatedAt)
            .ToListAsync();

        return View(new DocumentChatViewModel
        {
            Document = document,
            Messages = messages
        });
    }

    [HttpPost("DocumentChat/Ask/{documentId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ask(int documentId, string question, string? returnTo)
    {
        var document = await FindOwnedDocumentAsync(documentId);
        if (document == null)
        {
            TempData["ErrorMessage"] = "Document not found or you do not have permission to access it.";
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(document.ExtractedText))
        {
            TempData["WarningMessage"] = "Please extract text first before chatting with this PDF.";
            if (ShouldReturnToWorkspace(returnTo))
            {
                return RedirectToWorkspaceChat(documentId);
            }

            return await ReturnChatWithError(document, "Please extract text first before chatting with this PDF.");
        }

        if (string.IsNullOrWhiteSpace(question))
        {
            TempData["ErrorMessage"] = "Question cannot be empty.";
            if (ShouldReturnToWorkspace(returnTo))
            {
                return RedirectToWorkspaceChat(documentId);
            }

            return await ReturnChatWithError(document, "Question cannot be empty.");
        }

        DocumentChatResponse response = await _documentChatService.AnswerQuestionAsync(
            question,
            document.ExtractedText,
            document.Summary,
            GetCurrentUserId());

        var message = new DocumentChatMessage
        {
            DocumentId = document.Id,
            UserId = GetCurrentUserId(),
            Question = question,
            Answer = response.Answer,
            SourceSnippet = response.SourceSnippet,
            CreatedAt = DateTime.Now
        };

        _context.DocumentChatMessages.Add(message);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Question answered.";
        if (ShouldReturnToWorkspace(returnTo))
        {
            return RedirectToWorkspaceChat(documentId);
        }

        return RedirectToAction(nameof(Index), new { documentId });
    }

    private IActionResult RedirectToWorkspaceChat(int documentId)
    {
        return RedirectToAction("Details", "Documents", new { id = documentId, tab = DocumentWorkspaceViewModel.ChatTab });
    }

    private static bool ShouldReturnToWorkspace(string? returnTo)
    {
        return string.Equals(returnTo, "workspace", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<IActionResult> ReturnChatWithError(Document document, string errorMessage)
    {
        var messages = await _context.DocumentChatMessages
            .Where(message => message.DocumentId == document.Id && message.UserId == GetCurrentUserId())
            .OrderBy(message => message.CreatedAt)
            .ToListAsync();

        return View("Index", new DocumentChatViewModel
        {
            Document = document,
            Messages = messages,
            ErrorMessage = errorMessage
        });
    }

    private async Task<Document?> FindOwnedDocumentAsync(int documentId)
    {
        int userId = GetCurrentUserId();

        return await _context.Documents
            .FirstOrDefaultAsync(document => document.Id == documentId && document.UserId == userId);
    }

    private int GetCurrentUserId()
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdValue!);
    }
}
