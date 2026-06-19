using AcademicAIAssistant.Models;
using AcademicAIAssistant.Services;

namespace AcademicAIAssistant.Tests.Services;

public class WritingFeedbackServiceTests
{
    private readonly WritingFeedbackService _service = new();

    [Fact]
    public void AnalyzeEssay_WithShortEssay_LowersScoreAndWarns()
    {
        FeedbackReport result = _service.AnalyzeEssay("Short", "Essay", "This essay is too short.");

        Assert.True(result.OverallScore < 80);
        Assert.Contains("too short", result.GrammarFeedback, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AnalyzeEssay_WithApaCitation_RecognizesCitation()
    {
        string content = LongEssayWith("This paper argues that AI tools support academic writing (Nguyen, 2023).");

        FeedbackReport result = _service.AnalyzeEssay("Citation", "Essay", content);

        Assert.Contains("citation was found", result.CitationFeedback, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AnalyzeEssay_WithoutCitation_SuggestsAcademicSources()
    {
        string content = LongEssayWith("This paper argues that AI tools support academic writing.");

        FeedbackReport result = _service.AnalyzeEssay("No Citation", "Essay", content);

        Assert.Contains("No APA-style citation", result.CitationFeedback);
    }

    [Fact]
    public void AnalyzeEssay_WithInformalTone_FlagsInformalWords()
    {
        string content = LongEssayWith("I think AI tools are really useful for many things and stuff.");

        FeedbackReport result = _service.AnalyzeEssay("Tone", "Essay", content);

        Assert.Contains("informal", result.AcademicToneFeedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("really", result.AcademicToneFeedback);
    }

    private static string LongEssayWith(string sentence)
    {
        return string.Join(" ", Enumerable.Repeat(sentence, 35));
    }
}
