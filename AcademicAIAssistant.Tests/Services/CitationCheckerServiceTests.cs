using AcademicAIAssistant.Models;
using AcademicAIAssistant.Services;

namespace AcademicAIAssistant.Tests.Services;

public class CitationCheckerServiceTests
{
    private readonly CitationCheckerService _service = new();

    [Fact]
    public void CheckCitation_WithValidApaCitationAndReference_Passes()
    {
        string essay = """
            AI tools support academic writing (Nguyen, 2023).

            References
            Nguyen, A. (2023). AI tools for academic writing. Journal of Education.
            """;

        CitationCheck result = _service.CheckCitation(essay);

        Assert.Equal(1, result.TotalInTextCitations);
        Assert.Equal(1, result.TotalReferences);
        Assert.Equal("Passed", result.OverallStatus);
    }

    [Fact]
    public void CheckCitation_WithMissingReferencesSection_Fails()
    {
        string essay = "AI tools support academic writing (Nguyen, 2023).";

        CitationCheck result = _service.CheckCitation(essay);

        Assert.Equal("Failed", result.OverallStatus);
        Assert.Contains("No References section found", result.FormatIssues);
    }

    [Fact]
    public void CheckCitation_WithUnusedReference_ReturnsWarning()
    {
        string essay = """
            AI tools support academic writing.

            References
            Nguyen, A. (2023). AI tools for academic writing. Journal of Education.
            """;

        CitationCheck result = _service.CheckCitation(essay);

        Assert.Equal("Warning", result.OverallStatus);
        Assert.Contains("Nguyen, 2023", result.UnusedReferences);
    }

    [Fact]
    public void CheckCitation_WithInvalidYearFormat_ReportsFormatIssue()
    {
        string essay = """
            AI tools support academic writing (Nguyen, 20).

            References
            Nguyen, A. (2020). AI tools for academic writing. Journal of Education.
            """;

        CitationCheck result = _service.CheckCitation(essay);

        Assert.Contains("Citation year should have 4 digits", result.FormatIssues);
    }
}
