using AcademicAIAssistant.Data;
using AcademicAIAssistant.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicAIAssistant.Services;

public class KnowledgeGraphService
{
    private readonly AppDbContext _context;
    private readonly KeywordExtractionService _keywordExtractionService;

    public KnowledgeGraphService(AppDbContext context, KeywordExtractionService keywordExtractionService)
    {
        _context = context;
        _keywordExtractionService = keywordExtractionService;
    }

    public async Task BuildGraphForUser(int userId)
    {
        await DeleteOldGraphAsync(userId);

        var documents = await _context.Documents
            .Where(document => document.UserId == userId
                && ((document.ExtractedText != null && document.ExtractedText != "")
                    || (document.Summary != null && document.Summary != "")))
            .ToListAsync();

        var essays = await _context.Essays
            .Where(essay => essay.UserId == userId && essay.Content != "")
            .ToListAsync();

        var keywordNodes = new Dictionary<string, GraphNode>(StringComparer.OrdinalIgnoreCase);
        var documentNodes = new Dictionary<int, GraphNode>();
        var essayNodes = new Dictionary<int, GraphNode>();
        var documentKeywords = new Dictionary<int, List<string>>();
        var essayKeywords = new Dictionary<int, List<string>>();
        var edges = new List<GraphEdge>();

        foreach (var document in documents)
        {
            var documentNode = CreateNode(userId, "Document", document.Title, "Document", document.Id, document.OriginalFileName);
            documentNodes[document.Id] = documentNode;
            _context.GraphNodes.Add(documentNode);

            string text = string.IsNullOrWhiteSpace(document.ExtractedText) ? document.Summary ?? string.Empty : document.ExtractedText;
            List<string> keywords = _keywordExtractionService.ExtractKeywords(text);
            documentKeywords[document.Id] = keywords;

            foreach (string keyword in keywords)
            {
                GraphNode keywordNode = GetOrCreateKeywordNode(userId, keyword, keywordNodes);
                edges.Add(CreateEdge(userId, documentNode, keywordNode, "HAS_KEYWORD", 1));
            }
        }

        foreach (var essay in essays)
        {
            var essayNode = CreateNode(userId, "Essay", essay.Title, "Essay", essay.Id, essay.EssayType);
            essayNodes[essay.Id] = essayNode;
            _context.GraphNodes.Add(essayNode);

            List<string> keywords = _keywordExtractionService.ExtractKeywords(essay.Content);
            essayKeywords[essay.Id] = keywords;

            foreach (string keyword in keywords)
            {
                GraphNode keywordNode = GetOrCreateKeywordNode(userId, keyword, keywordNodes);
                edges.Add(CreateEdge(userId, essayNode, keywordNode, "HAS_KEYWORD", 1));
            }
        }

        AddEssayDocumentEdges(userId, essayNodes, documentNodes, essayKeywords, documentKeywords, edges);
        AddRelatedDocumentEdges(userId, documentNodes, documentKeywords, edges);

        _context.GraphEdges.AddRange(edges);
        await _context.SaveChangesAsync();
    }

    private async Task DeleteOldGraphAsync(int userId)
    {
        var oldEdges = await _context.GraphEdges
            .Where(edge => edge.UserId == userId)
            .ToListAsync();

        var oldNodes = await _context.GraphNodes
            .Where(node => node.UserId == userId)
            .ToListAsync();

        _context.GraphEdges.RemoveRange(oldEdges);
        _context.GraphNodes.RemoveRange(oldNodes);
        await _context.SaveChangesAsync();
    }

    private GraphNode GetOrCreateKeywordNode(int userId, string keyword, Dictionary<string, GraphNode> keywordNodes)
    {
        if (keywordNodes.TryGetValue(keyword, out GraphNode? existingNode))
        {
            return existingNode;
        }

        var node = CreateNode(userId, "Keyword", keyword, "Keyword", null, null);
        keywordNodes[keyword] = node;
        _context.GraphNodes.Add(node);

        return node;
    }

    private static void AddEssayDocumentEdges(
        int userId,
        Dictionary<int, GraphNode> essayNodes,
        Dictionary<int, GraphNode> documentNodes,
        Dictionary<int, List<string>> essayKeywords,
        Dictionary<int, List<string>> documentKeywords,
        List<GraphEdge> edges)
    {
        foreach (var essayItem in essayKeywords)
        {
            foreach (var documentItem in documentKeywords)
            {
                int sharedCount = essayItem.Value.Intersect(documentItem.Value, StringComparer.OrdinalIgnoreCase).Count();
                if (sharedCount > 0)
                {
                    edges.Add(CreateEdge(userId, essayNodes[essayItem.Key], documentNodes[documentItem.Key], "USED_IN_ESSAY", sharedCount));
                }
            }
        }
    }

    private static void AddRelatedDocumentEdges(
        int userId,
        Dictionary<int, GraphNode> documentNodes,
        Dictionary<int, List<string>> documentKeywords,
        List<GraphEdge> edges)
    {
        var documentIds = documentKeywords.Keys.ToList();
        for (int i = 0; i < documentIds.Count; i++)
        {
            for (int j = i + 1; j < documentIds.Count; j++)
            {
                int firstId = documentIds[i];
                int secondId = documentIds[j];
                int sharedCount = documentKeywords[firstId].Intersect(documentKeywords[secondId], StringComparer.OrdinalIgnoreCase).Count();

                if (sharedCount >= 2)
                {
                    edges.Add(CreateEdge(userId, documentNodes[firstId], documentNodes[secondId], "RELATED_TO", sharedCount));
                }
            }
        }
    }

    private static GraphNode CreateNode(int userId, string nodeType, string label, string sourceType, int? sourceId, string? metadata)
    {
        return new GraphNode
        {
            UserId = userId,
            NodeType = nodeType,
            Label = label,
            SourceType = sourceType,
            SourceId = sourceId,
            Metadata = metadata,
            CreatedAt = DateTime.Now
        };
    }

    private static GraphEdge CreateEdge(int userId, GraphNode source, GraphNode target, string relationType, double weight)
    {
        return new GraphEdge
        {
            UserId = userId,
            SourceNode = source,
            TargetNode = target,
            RelationType = relationType,
            Weight = weight,
            CreatedAt = DateTime.Now
        };
    }
}
