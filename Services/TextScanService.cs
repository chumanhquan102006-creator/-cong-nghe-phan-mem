using System.Text.RegularExpressions;
using AcademicAIAssistant.Data;
using AcademicAIAssistant.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicAIAssistant.Services;

public class TextScanService
{
    private const double MatchThreshold = 45;

    private readonly AppDbContext _context;

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an", "and", "or", "of", "to", "in", "for", "with", "on", "by",
        "is", "are", "be", "this", "that", "was", "were", "as", "at", "from",
        "la", "là", "và", "của", "trong", "với", "cho", "các", "những", "một",
        "có", "được", "để", "khi", "này", "đó"
    };

    public TextScanService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TextScan> ScanTextAsync(int userId, string title, string inputText)
    {
        int wordCount = CountWords(inputText);
        if (wordCount < 50)
        {
            throw new InvalidOperationException("Please enter at least 50 words for a meaningful scan.");
        }

        List<string> inputSegments = SplitIntoSegments(inputText);
        List<SourceTextItem> sources = await LoadSourceTextsAsync(userId);
        var matches = new List<TextScanMatch>();

        foreach (SourceTextItem source in sources)
        {
            List<string> sourceSegments = SplitIntoSegments(source.Text);

            foreach (string inputSegment in inputSegments)
            {
                foreach (string sourceSegment in sourceSegments)
                {
                    double score = CalculateJaccardScore(inputSegment, sourceSegment);
                    if (score >= MatchThreshold)
                    {
                        matches.Add(new TextScanMatch
                        {
                            SourceType = source.SourceType,
                            SourceId = source.SourceId,
                            SourceTitle = source.SourceTitle,
                            InputSegment = TrimForStorage(inputSegment),
                            MatchedSegment = TrimForStorage(sourceSegment),
                            SimilarityScore = Math.Round(score, 2),
                            CreatedAt = DateTime.Now
                        });
                    }
                }
            }
        }

        List<TextScanMatch> topMatches = matches
            .OrderByDescending(match => match.SimilarityScore)
            .Take(10)
            .ToList();

        double overallScore = topMatches.Count == 0 ? 0 : topMatches.Max(match => match.SimilarityScore);

        var scan = new TextScan
        {
            UserId = userId,
            Title = title.Trim(),
            InputText = inputText.Trim(),
            WordCount = wordCount,
            OverallSimilarityScore = Math.Round(overallScore, 2),
            RiskLevel = GetRiskLevel(overallScore),
            CreatedAt = DateTime.Now,
            Matches = topMatches
        };

        _context.TextScans.Add(scan);
        await _context.SaveChangesAsync();

        return scan;
    }

    private async Task<List<SourceTextItem>> LoadSourceTextsAsync(int userId)
    {
        var sources = new List<SourceTextItem>();

        var documents = await _context.Documents
            .AsNoTracking()
            .Where(document => document.UserId == userId && !string.IsNullOrWhiteSpace(document.ExtractedText))
            .Select(document => new SourceTextItem("Document", document.Id, document.Title, document.ExtractedText!))
            .ToListAsync();
        sources.AddRange(documents);

        var essays = await _context.Essays
            .AsNoTracking()
            .Where(essay => essay.UserId == userId && !string.IsNullOrWhiteSpace(essay.Content))
            .Select(essay => new SourceTextItem("Essay", essay.Id, essay.Title, essay.Content))
            .ToListAsync();
        sources.AddRange(essays);

        var ocrScans = await _context.OCRScans
            .AsNoTracking()
            .Where(scan => scan.UserId == userId && !string.IsNullOrWhiteSpace(scan.ExtractedText))
            .Select(scan => new SourceTextItem("OCRScan", scan.Id, scan.Title, scan.ExtractedText))
            .ToListAsync();
        sources.AddRange(ocrScans);

        return sources;
    }

    private static List<string> SplitIntoSegments(string text)
    {
        string normalizedText = NormalizeText(text);
        var paragraphs = Regex.Split(normalizedText, @"\n\s*\n")
            .Select(item => item.Trim())
            .Where(item => CountWords(item) >= 20)
            .ToList();

        var result = new List<string>();
        foreach (string paragraph in paragraphs)
        {
            if (CountWords(paragraph) <= 120)
            {
                result.Add(paragraph);
                continue;
            }

            result.AddRange(Regex.Split(paragraph, @"(?<=[.!?])\s+")
                .Select(sentence => sentence.Trim())
                .Where(sentence => CountWords(sentence) >= 20));
        }

        if (result.Count > 0)
        {
            return result;
        }

        return Regex.Split(normalizedText, @"(?<=[.!?])\s+")
            .Select(sentence => sentence.Trim())
            .Where(sentence => CountWords(sentence) >= 20)
            .ToList();
    }

    private static double CalculateJaccardScore(string first, string second)
    {
        HashSet<string> firstTokens = Tokenize(first);
        HashSet<string> secondTokens = Tokenize(second);

        if (firstTokens.Count == 0 || secondTokens.Count == 0)
        {
            return 0;
        }

        int intersection = firstTokens.Intersect(secondTokens).Count();
        int union = firstTokens.Union(secondTokens).Count();

        return union == 0 ? 0 : (double)intersection / union * 100;
    }

    private static HashSet<string> Tokenize(string text)
    {
        return Regex.Matches(text.ToLowerInvariant(), @"\b[\p{L}\p{N}']+\b")
            .Select(match => match.Value)
            .Where(token => token.Length > 2 && !StopWords.Contains(token))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static string NormalizeText(string text)
    {
        return text.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
    }

    private static int CountWords(string text)
    {
        return Regex.Matches(text, @"\b[\p{L}\p{N}']+\b").Count;
    }

    private static string GetRiskLevel(double score)
    {
        if (score >= 60)
        {
            return "High";
        }

        if (score >= 30)
        {
            return "Medium";
        }

        return "Low";
    }

    private static string TrimForStorage(string text)
    {
        string normalizedText = Regex.Replace(text, @"\s+", " ").Trim();
        return normalizedText.Length <= 1000 ? normalizedText : normalizedText[..1000] + "...";
    }

    private record SourceTextItem(string SourceType, int SourceId, string SourceTitle, string Text);
}
