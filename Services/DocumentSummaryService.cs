using System.Text.RegularExpressions;

namespace AcademicAIAssistant.Services;

public class DocumentSummaryService
{
    public string GenerateMockSummary(string extractedText)
    {
        string text = NormalizeText(extractedText);
        if (string.IsNullOrWhiteSpace(text) || CountWords(text) < 50)
        {
            return "Not enough text to summarize.";
        }

        var sentences = Regex.Split(text, @"(?<=[.!?])\s+")
            .Where(sentence => !string.IsNullOrWhiteSpace(sentence))
            .Take(5)
            .ToList();

        if (sentences.Count == 0)
        {
            return "Not enough text to summarize.";
        }

        return string.Join(" ", sentences);
    }

    private static string NormalizeText(string value)
    {
        string text = value.Replace("\r\n", "\n").Replace("\r", "\n");

        // Keep a space when PDF line breaks join two words together.
        text = Regex.Replace(text, @"(?<=[\p{Ll}\p{N}])\n(?=[\p{Lu}])", " ");
        text = Regex.Replace(text, @"(?<=[\p{L}\p{N}])\n(?=[\p{L}\p{N}])", " ");

        // Add missing spaces after punctuation, for example "word.Next" or "word,Next".
        text = Regex.Replace(text, @"([.,;:!?])(?=[\p{L}\p{N}])", "$1 ");

        // Add spaces in common PDF extraction joins like "UniversityAbstract".
        text = Regex.Replace(text, @"(?<=[\p{Ll}])(?=[\p{Lu}])", " ");

        return Regex.Replace(text, @"\s+", " ").Trim();
    }

    private static int CountWords(string value)
    {
        return Regex.Matches(value, @"\b[\p{L}\p{N}']+\b").Count;
    }
}
