using System.Text.RegularExpressions;

namespace AcademicAIAssistant.Services;

public class DocumentChatService
{
    private readonly KeywordExtractionService _keywordExtractionService;
    private readonly IAIService _aiService;

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "what", "is", "are", "the", "a", "an", "of", "to", "in", "for",
        "with", "about", "this", "that", "how", "why", "does", "do",
        "can", "could", "should", "please", "tell", "me"
    };

    public DocumentChatService(KeywordExtractionService keywordExtractionService, IAIService aiService)
    {
        _keywordExtractionService = keywordExtractionService;
        _aiService = aiService;
    }

    public async Task<DocumentChatResponse> AnswerQuestionAsync(string question, string documentText, string? summary = null)
    {
        if (string.IsNullOrWhiteSpace(question) || ExtractQuestionKeywords(question).Count < 2)
        {
            return new DocumentChatResponse
            {
                Answer = "Please enter a more specific question."
            };
        }

        string normalizedQuestion = question.ToLowerInvariant();

        if (normalizedQuestion.Contains("summary") || normalizedQuestion.Contains("summarize"))
        {
            if (!string.IsNullOrWhiteSpace(summary))
            {
                var response = new DocumentChatResponse
                {
                    Answer = $"Based on the current document summary: {Shorten(summary, 800)}",
                    SourceSnippet = summary
                };

                return await TryImproveWithAiAsync(question, response);
            }

            return await TryImproveWithAiAsync(question, BuildAnswerFromSnippet(FindBestSection(documentText, new[] { "abstract", "introduction", "conclusion" })
                ?? FindFirstSection(documentText)));
        }

        if (normalizedQuestion.Contains("keyword") || normalizedQuestion.Contains("keywords"))
        {
            List<string> keywords = _keywordExtractionService.ExtractKeywords(documentText, 8);
            string answer = keywords.Count == 0
                ? "I could not extract clear keywords from this document."
                : $"Important keywords in this document include: {string.Join(", ", keywords)}.";

            return new DocumentChatResponse { Answer = answer };
        }

        if (normalizedQuestion.Contains("methodology") || normalizedQuestion.Contains("method"))
        {
            return await TryImproveWithAiAsync(question, BuildAnswerFromSnippet(FindBestSection(documentText, new[] { "method", "methodology", "approach" })));
        }

        if (normalizedQuestion.Contains("finding") || normalizedQuestion.Contains("result"))
        {
            return await TryImproveWithAiAsync(question, BuildAnswerFromSnippet(FindBestSection(documentText, new[] { "finding", "findings", "result", "results", "conclusion" })));
        }

        if (normalizedQuestion.Contains("citation") || normalizedQuestion.Contains("reference"))
        {
            return await TryImproveWithAiAsync(question, BuildAnswerFromSnippet(FindBestSection(documentText, new[] { "references", "bibliography" })));
        }

        if (normalizedQuestion.Contains("abstract"))
        {
            return await TryImproveWithAiAsync(question, BuildAnswerFromSnippet(FindBestSection(documentText, new[] { "abstract" })));
        }

        List<string> questionKeywords = ExtractQuestionKeywords(question);
        string? snippet = FindBestKeywordMatch(documentText, questionKeywords);

        return await TryImproveWithAiAsync(question, BuildAnswerFromSnippet(snippet));
    }

    private async Task<DocumentChatResponse> TryImproveWithAiAsync(string question, DocumentChatResponse fallbackResponse)
    {
        if (!_aiService.IsEnabled || string.IsNullOrWhiteSpace(fallbackResponse.SourceSnippet))
        {
            return fallbackResponse;
        }

        try
        {
            string aiAnswer = await _aiService.AnswerQuestionAboutDocumentAsync(question, fallbackResponse.SourceSnippet);
            fallbackResponse.Answer = aiAnswer;
        }
        catch
        {
            // Keep the rule-based answer if AI is unavailable, times out, or returns an error.
        }

        return fallbackResponse;
    }

    private static DocumentChatResponse BuildAnswerFromSnippet(string? snippet)
    {
        if (string.IsNullOrWhiteSpace(snippet))
        {
            return new DocumentChatResponse
            {
                Answer = "I could not find a clearly relevant section in this document. Please try asking a more specific question."
            };
        }

        return new DocumentChatResponse
        {
            Answer = $"Based on the uploaded document, the most relevant information is: {Shorten(snippet, 700)}",
            SourceSnippet = snippet
        };
    }

    private static string? FindBestKeywordMatch(string documentText, List<string> questionKeywords)
    {
        var sections = SplitIntoSections(documentText);

        return sections
            .Select(section => new
            {
                Text = section,
                Score = questionKeywords.Count(keyword => section.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            })
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Text.Length)
            .Select(item => item.Text)
            .FirstOrDefault();
    }

    private static string? FindFirstSection(string documentText)
    {
        return SplitIntoSections(documentText).FirstOrDefault();
    }

    private static string? FindBestSection(string documentText, IEnumerable<string> targetWords)
    {
        var sections = SplitIntoSections(documentText);

        return sections
            .Select(section => new
            {
                Text = section,
                Score = targetWords.Count(word => section.Contains(word, StringComparison.OrdinalIgnoreCase))
            })
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Text.Length)
            .Select(item => item.Text)
            .FirstOrDefault();
    }

    private static List<string> SplitIntoSections(string text)
    {
        string normalizedText = text.Replace("\r\n", "\n").Replace("\r", "\n");

        var paragraphs = Regex.Split(normalizedText, @"\n\s*\n")
            .Select(item => Regex.Replace(item, @"\s+", " ").Trim())
            .Where(item => CountWords(item) >= 12)
            .ToList();

        if (paragraphs.Count > 0)
        {
            return SplitLongSections(paragraphs);
        }

        var sentences = Regex.Split(normalizedText, @"(?<=[.!?])\s+")
            .Select(item => Regex.Replace(item, @"\s+", " ").Trim())
            .Where(item => CountWords(item) >= 12)
            .ToList();

        return SplitLongSections(sentences);
    }

    private static List<string> SplitLongSections(List<string> sections)
    {
        var result = new List<string>();

        foreach (string section in sections)
        {
            if (section.Length <= 900)
            {
                result.Add(section);
                continue;
            }

            result.AddRange(Regex.Split(section, @"(?<=[.!?])\s+")
                .Select(item => item.Trim())
                .Where(item => CountWords(item) >= 12));
        }

        return result;
    }

    private static List<string> ExtractQuestionKeywords(string question)
    {
        return Regex.Matches(question.ToLowerInvariant(), @"\b[\p{L}\p{N}']+\b")
            .Select(match => match.Value)
            .Where(word => word.Length > 2 && !StopWords.Contains(word))
            .Distinct()
            .ToList();
    }

    private static int CountWords(string text)
    {
        return Regex.Matches(text, @"\b[\p{L}\p{N}']+\b").Count;
    }

    private static string Shorten(string text, int maxLength)
    {
        string normalizedText = Regex.Replace(text, @"\s+", " ").Trim();
        return normalizedText.Length <= maxLength ? normalizedText : normalizedText[..maxLength] + "...";
    }
}
