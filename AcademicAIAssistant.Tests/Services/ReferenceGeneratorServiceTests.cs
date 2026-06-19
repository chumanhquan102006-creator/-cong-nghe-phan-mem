using AcademicAIAssistant.Models;
using AcademicAIAssistant.Services;

namespace AcademicAIAssistant.Tests.Services;

public class ReferenceGeneratorServiceTests
{
    private readonly ReferenceGeneratorService _service = new();

    [Fact]
    public void GenerateApaReference_ForBook_ContainsCoreFields()
    {
        var item = new ReferenceItem
        {
            SourceType = "Book",
            Author = "Nguyen, A.",
            Year = "2023",
            Title = "AI Tools for Academic Writing",
            JournalOrPublisher = "Education Press"
        };

        string reference = _service.GenerateApaReference(item);

        Assert.Contains("Nguyen", reference);
        Assert.Contains("(2023)", reference);
        Assert.Contains("AI Tools for Academic Writing", reference);
        Assert.Contains("Education Press", reference);
    }

    [Fact]
    public void GenerateApaInText_WithAuthorAndYear_ReturnsAuthorYear()
    {
        var item = new ReferenceItem { Author = "Nguyen", Year = "2023", Title = "AI Tools" };

        string citation = _service.GenerateApaInText(item);

        Assert.Equal("(Nguyen, 2023)", citation);
    }

    [Fact]
    public void GenerateApaInText_WithoutAuthor_UsesShortTitle()
    {
        var item = new ReferenceItem { Title = "AI in Education", Year = "2024" };

        string citation = _service.GenerateApaInText(item);

        Assert.Equal("(\"AI in Education\", 2024)", citation);
    }

    [Fact]
    public void GenerateMlaReference_ForWebsite_ContainsWebsiteYearAndUrl()
    {
        var item = new ReferenceItem
        {
            SourceType = "Website",
            Title = "AI in Education",
            WebsiteName = "Education Today",
            Year = "2024",
            Url = "https://example.com"
        };

        string reference = _service.GenerateMlaReference(item);

        Assert.Contains("\"AI in Education.\"", reference);
        Assert.Contains("Education Today", reference);
        Assert.Contains("2024", reference);
        Assert.Contains("https://example.com", reference);
    }
}
