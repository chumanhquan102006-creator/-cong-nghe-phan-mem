using System.Security.Claims;
using AcademicAIAssistant.Data;
using AcademicAIAssistant.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicAIAssistant.Controllers;

[Authorize]
public class GraphController : Controller
{
    private readonly AppDbContext _context;
    private readonly KnowledgeGraphService _knowledgeGraphService;

    public GraphController(AppDbContext context, KnowledgeGraphService knowledgeGraphService)
    {
        _context = context;
        _knowledgeGraphService = knowledgeGraphService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        int userId = GetCurrentUserId();

        ViewData["HasSourceData"] = await HasSourceDataAsync(userId);
        ViewData["HasGraph"] = await _context.GraphNodes.AnyAsync(node => node.UserId == userId);

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Build()
    {
        int userId = GetCurrentUserId();

        if (!await HasSourceDataAsync(userId))
        {
            TempData["WarningMessage"] = "No documents or essays available to build a knowledge graph.";
            return RedirectToAction(nameof(Index));
        }

        await _knowledgeGraphService.BuildGraphForUser(userId);
        TempData["SuccessMessage"] = "Knowledge graph built successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Data()
    {
        int userId = GetCurrentUserId();

        var nodes = await _context.GraphNodes
            .Where(node => node.UserId == userId)
            .Select(node => new
            {
                id = node.Id,
                label = node.Label,
                type = node.NodeType,
                metadata = node.Metadata
            })
            .ToListAsync();

        var edges = await _context.GraphEdges
            .Where(edge => edge.UserId == userId)
            .Select(edge => new
            {
                from = edge.SourceNodeId,
                to = edge.TargetNodeId,
                label = edge.RelationType,
                weight = edge.Weight
            })
            .ToListAsync();

        return Json(new { nodes, edges });
    }

    private async Task<bool> HasSourceDataAsync(int userId)
    {
        bool hasDocument = await _context.Documents.AnyAsync(document =>
            document.UserId == userId
            && ((document.ExtractedText != null && document.ExtractedText != "")
                || (document.Summary != null && document.Summary != "")));

        bool hasEssay = await _context.Essays.AnyAsync(essay =>
            essay.UserId == userId && essay.Content != "");

        return hasDocument || hasEssay;
    }

    private int GetCurrentUserId()
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdValue!);
    }
}
