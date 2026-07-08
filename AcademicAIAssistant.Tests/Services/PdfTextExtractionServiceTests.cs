using AcademicAIAssistant.Services;

namespace AcademicAIAssistant.Tests.Services;

public class PdfTextExtractionServiceTests
{
    [Fact]
    public void NormalizeText_PreservesWordBoundariesAcrossPdfLineBreaks()
    {
        string text = PdfTextExtractionService.NormalizeText("research\ndocuments because\nlearning materials");

        Assert.Equal("research documents because learning materials", text);
    }

    [Fact]
    public void NormalizeText_AddsSpaceAfterJoinedPunctuation()
    {
        string text = PdfTextExtractionService.NormalizeText("First sentence.Next sentence,Another phrase");

        Assert.Equal("First sentence. Next sentence, Another phrase", text);
    }
}
