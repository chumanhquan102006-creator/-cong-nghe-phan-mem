using System.Text.RegularExpressions;
using AcademicAIAssistant.Models;

namespace AcademicAIAssistant.Services;

public class WritingFeedbackService
{
    public FeedbackReport AnalyzeEssay(string title, string essayType, string content)
    {
        string normalizedContent = NormalizeText(content);
        int wordCount = CountWords(normalizedContent);
        int score = 100;

        string grammarFeedback = BuildGrammarFeedback(normalizedContent, wordCount, ref score);
        string academicToneFeedback = BuildAcademicToneFeedback(normalizedContent, ref score);
        string thesisFeedback = BuildThesisFeedback(normalizedContent, ref score);
        string structureFeedback = BuildStructureFeedback(content, ref score);
        string logicFeedback = BuildLogicFeedback(normalizedContent, ref score);
        string citationFeedback = BuildCitationFeedback(normalizedContent, ref score);

        score = Math.Clamp(score, 0, 100);

        return new FeedbackReport
        {
            OverallScore = score,
            GrammarFeedback = grammarFeedback,
            AcademicToneFeedback = academicToneFeedback,
            ThesisFeedback = thesisFeedback,
            StructureFeedback = structureFeedback,
            LogicFeedback = logicFeedback,
            CitationFeedback = citationFeedback,
            GeneralSuggestions = BuildGeneralSuggestions(score, essayType, wordCount),
            CreatedAt = DateTime.Now
        };
    }

    public int CountWords(string content)
    {
        return Regex.Matches(NormalizeText(content), @"\b[\p{L}\p{N}']+\b").Count;
    }

    private static string BuildGrammarFeedback(string content, int wordCount, ref int score)
    {
        var feedback = new List<string>();

        if (wordCount < 150)
        {
            feedback.Add("The essay is too short. Add more explanation, evidence, and analysis.");
            score -= 20;
        }
        else if (wordCount <= 300)
        {
            feedback.Add("The essay has a basic length, but key ideas should be developed further.");
            score -= 10;
        }
        else
        {
            feedback.Add("The essay length is acceptable for an early draft.");
        }

        if (Regex.IsMatch(content, @"\s+[.,;:!?]"))
        {
            feedback.Add("Check spacing before punctuation marks.");
            score -= 5;
        }

        return string.Join(" ", feedback);
    }

    private static string BuildAcademicToneFeedback(string content, ref int score)
    {
        string[] informalWords =
        {
            "a lot of",
            "really",
            "very",
            "things",
            "stuff",
            "I think",
            "you know"
        };

        var foundWords = informalWords
            .Where(word => content.Contains(word, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (foundWords.Count == 0)
        {
            return "The academic tone is acceptable. Continue using precise and formal wording.";
        }

        score -= Math.Min(15, foundWords.Count * 4);
        return $"Consider replacing informal wording: {string.Join(", ", foundWords)}. Use more precise academic expressions.";
    }

    private static string BuildThesisFeedback(string content, ref int score)
    {
        string firstParagraph = GetParagraphs(content).FirstOrDefault() ?? content;
        string[] thesisSignals =
        {
            "argue",
            "suggest",
            "indicate",
            "demonstrate",
            "this essay",
            "this paper",
            "the purpose of this"
        };

        bool hasThesisSignal = thesisSignals.Any(signal =>
            firstParagraph.Contains(signal, StringComparison.OrdinalIgnoreCase));

        if (hasThesisSignal)
        {
            return "The opening paragraph appears to include a thesis or clear purpose statement.";
        }

        score -= 15;
        return "The thesis statement is not clear in the opening paragraph. Add a sentence that states the main argument or purpose.";
    }

    private static string BuildStructureFeedback(string content, ref int score)
    {
        var paragraphs = GetParagraphs(content);
        var feedback = new List<string>();

        if (paragraphs.Count <= 1 && CountWordsStatic(content) > 150)
        {
            feedback.Add("The essay appears to be one long paragraph. Divide it into introduction, body paragraphs, and conclusion.");
            score -= 15;
        }

        bool hasLongParagraph = paragraphs.Any(paragraph => CountWordsStatic(paragraph) > 250);
        if (hasLongParagraph)
        {
            feedback.Add("At least one paragraph is longer than 250 words. Consider splitting long paragraphs.");
            score -= 8;
        }

        bool hasIntroductionSignal = Regex.IsMatch(content, @"\b(introduction|background|purpose)\b", RegexOptions.IgnoreCase);
        bool hasConclusionSignal = Regex.IsMatch(content, @"\b(conclusion|in conclusion|to conclude)\b", RegexOptions.IgnoreCase);

        if (!hasIntroductionSignal)
        {
            feedback.Add("The introduction could be clearer. Add background or purpose for the topic.");
            score -= 5;
        }

        if (!hasConclusionSignal)
        {
            feedback.Add("The conclusion is not clearly signposted. Add a short concluding paragraph.");
            score -= 5;
        }

        if (feedback.Count == 0)
        {
            feedback.Add("The structure is acceptable for a draft.");
        }

        return string.Join(" ", feedback);
    }

    private static string BuildLogicFeedback(string content, ref int score)
    {
        string[] transitions =
        {
            "however",
            "therefore",
            "moreover",
            "furthermore",
            "in contrast",
            "as a result",
            "consequently"
        };

        int transitionCount = transitions.Count(word =>
            content.Contains(word, StringComparison.OrdinalIgnoreCase));

        var feedback = new List<string>();

        if (transitionCount < 2)
        {
            feedback.Add("Use more transition words to connect ideas and improve logical flow.");
            score -= 10;
        }
        else
        {
            feedback.Add("The essay uses some transition words to connect ideas.");
        }

        if (Regex.IsMatch(content, @"\b(prove|always|never)\b", RegexOptions.IgnoreCase))
        {
            feedback.Add("Avoid overly strong claims such as 'prove', 'always', or 'never' unless strongly supported by evidence.");
            score -= 8;
        }

        return string.Join(" ", feedback);
    }

    private static string BuildCitationFeedback(string content, ref int score)
    {
        bool hasApaCitation = Regex.IsMatch(
            content,
            @"\([A-Z][A-Za-z]+(?:\s*&\s*[A-Z][A-Za-z]+|\s+et al\.)?,\s*\d{4}\)",
            RegexOptions.IgnoreCase);

        if (hasApaCitation)
        {
            return "At least one APA-style in-text citation was found.";
        }

        score -= 15;
        return "No APA-style citation was found. Add academic sources, for example: (Smith, 2020).";
    }

    private static string BuildGeneralSuggestions(int score, string essayType, int wordCount)
    {
        if (score >= 80)
        {
            return $"This {essayType} is a strong draft. Continue improving evidence, citation quality, and paragraph-level clarity.";
        }

        if (score >= 60)
        {
            return $"This {essayType} is a workable draft with {wordCount} words. Improve thesis clarity, source support, and logical transitions.";
        }

        return $"This {essayType} needs significant revision. Start by expanding content, clarifying the thesis, adding citations, and improving structure.";
    }

    private static List<string> GetParagraphs(string content)
    {
        return Regex.Split(content.Replace("\r\n", "\n"), @"\n\s*\n")
            .Select(paragraph => paragraph.Trim())
            .Where(paragraph => !string.IsNullOrWhiteSpace(paragraph))
            .ToList();
    }

    private static string NormalizeText(string value)
    {
        return Regex.Replace(value, @"\s+", " ").Trim();
    }

    private static int CountWordsStatic(string content)
    {
        return Regex.Matches(NormalizeText(content), @"\b[\p{L}\p{N}']+\b").Count;
    }
}
