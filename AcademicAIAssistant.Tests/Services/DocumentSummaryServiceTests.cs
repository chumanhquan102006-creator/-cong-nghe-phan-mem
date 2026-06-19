using AcademicAIAssistant.Services;

namespace AcademicAIAssistant.Tests.Services;

public class DocumentSummaryServiceTests
{
    private readonly DocumentSummaryService _service = new();

    [Fact]
    public void GenerateMockSummary_WithLongText_ReturnsFirstSentences()
    {
        string text = string.Join(" ", Enumerable.Range(1, 8)
            .Select(index => $"Sentence {index} explains academic writing support with enough words for summarization."));

        string summary = _service.GenerateMockSummary(text);

        Assert.NotEmpty(summary);
        Assert.Contains("Sentence 1", summary);
        Assert.Contains("Sentence 5", summary);
        Assert.DoesNotContain("Sentence 6", summary);
    }

    [Fact]
    public void GenerateMockSummary_WithEmptyText_ReturnsNotEnoughText()
    {
        string summary = _service.GenerateMockSummary(string.Empty);

        Assert.Equal("Not enough text to summarize.", summary);
    }

    [Fact]
    public void GenerateMockSummary_WithShortText_ReturnsNotEnoughText()
    {
        string summary = _service.GenerateMockSummary("Short text only.");

        Assert.Equal("Not enough text to summarize.", summary);
    }
}
