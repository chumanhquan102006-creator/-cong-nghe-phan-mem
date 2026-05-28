using System.Text.RegularExpressions;

namespace AcademicAIAssistant.Services;

public class KeywordExtractionService
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an", "and", "or", "of", "to", "in", "for", "with",
        "on", "by", "is", "are", "be", "this", "that", "students",
        "paper", "essay", "from", "have", "has", "was", "were", "will",
        "their", "there", "which", "about", "into", "using", "used"
    };

    public List<string> ExtractKeywords(string text, int maxKeywords = 8)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new List<string>();
        }

        return Regex.Matches(text.ToLowerInvariant(), @"\b[a-z][a-z]+\b")
            .Select(match => match.Value)
            .Where(word => word.Length >= 4 && !StopWords.Contains(word))
            .GroupBy(word => word)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .Take(maxKeywords)
            .Select(group => group.Key)
            .ToList();
    }
}
