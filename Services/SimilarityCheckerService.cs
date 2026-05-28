using System.Text.RegularExpressions;
using AcademicAIAssistant.Models;

namespace AcademicAIAssistant.Services;

public class SimilarityCheckerService
{
    private const double MatchThreshold = 45;

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "an", "and", "are", "as", "at", "be", "by", "for", "from",
        "has", "have", "in", "is", "it", "of", "on", "or", "that", "the",
        "this", "to", "was", "were", "with", "which", "who", "will"
    };

    public SimilarityCheck CheckSimilarity(string essayContent, List<Document> documents)
    {
        var essaySegments = SplitEssaySegments(essayContent);
        var matches = new List<SimilarityMatch>();

        foreach (var document in documents)
        {
            if (string.IsNullOrWhiteSpace(document.ExtractedText))
            {
                continue;
            }

            var documentSegments = SplitDocumentSegments(document.ExtractedText);

            foreach (string essaySegment in essaySegments)
            {
                foreach (string documentSegment in documentSegments)
                {
                    double score = CalculateTokenOverlapScore(essaySegment, documentSegment);
                    if (score >= MatchThreshold)
                    {
                        matches.Add(new SimilarityMatch
                        {
                            DocumentId = document.Id,
                            EssaySegment = TrimForStorage(essaySegment),
                            MatchedText = TrimForStorage(documentSegment),
                            SimilarityScore = Math.Round(score, 2),
                            CreatedAt = DateTime.Now
                        });
                    }
                }
            }
        }

        var topMatches = matches
            .OrderByDescending(match => match.SimilarityScore)
            .Take(10)
            .ToList();

        double overallScore = topMatches.Count == 0 ? 0 : topMatches.Max(match => match.SimilarityScore);

        return new SimilarityCheck
        {
            OverallSimilarityScore = Math.Round(overallScore, 2),
            TotalDocumentsCompared = documents.Count,
            TotalMatches = topMatches.Count,
            Status = GetStatus(overallScore),
            CheckedAt = DateTime.Now,
            Matches = topMatches
        };
    }

    private static List<string> SplitEssaySegments(string content)
    {
        var paragraphs = Regex.Split(NormalizeText(content), @"\n\s*\n")
            .Select(item => item.Trim())
            .Where(item => CountWords(item) >= 20)
            .ToList();

        var segments = new List<string>();
        foreach (string paragraph in paragraphs)
        {
            if (CountWords(paragraph) <= 120)
            {
                segments.Add(paragraph);
                continue;
            }

            segments.AddRange(Regex.Split(paragraph, @"(?<=[.!?])\s+")
                .Select(item => item.Trim())
                .Where(item => CountWords(item) >= 20));
        }

        return segments;
    }

    private static List<string> SplitDocumentSegments(string content)
    {
        string text = Regex.Replace(NormalizeText(content), @"\s+", " ");
        var segments = new List<string>();

        for (int index = 0; index < text.Length; index += 400)
        {
            int length = Math.Min(500, text.Length - index);
            string segment = text.Substring(index, length).Trim();
            if (CountWords(segment) >= 20)
            {
                segments.Add(segment);
            }
        }

        return segments;
    }

    private static double CalculateTokenOverlapScore(string essaySegment, string documentSegment)
    {
        var essayTokens = Tokenize(essaySegment);
        var documentTokens = Tokenize(documentSegment);

        if (essayTokens.Count == 0 || documentTokens.Count == 0)
        {
            return 0;
        }

        int intersection = essayTokens.Intersect(documentTokens).Count();
        return (double)intersection / essayTokens.Count * 100;
    }

    private static HashSet<string> Tokenize(string value)
    {
        return Regex.Matches(value.ToLowerInvariant(), @"\b[\p{L}\p{N}']+\b")
            .Select(match => match.Value)
            .Where(token => token.Length > 2 && !StopWords.Contains(token))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static string GetStatus(double score)
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

    private static string NormalizeText(string value)
    {
        return value.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
    }

    private static int CountWords(string value)
    {
        return Regex.Matches(value, @"\b[\p{L}\p{N}']+\b").Count;
    }

    private static string TrimForStorage(string value)
    {
        string text = Regex.Replace(value, @"\s+", " ").Trim();
        return text.Length <= 1000 ? text : text[..1000] + "...";
    }
}
