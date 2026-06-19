using AcademicAIAssistant.Data;
using AcademicAIAssistant.Models;
using AcademicAIAssistant.Services;
using AcademicAIAssistant.Tests.Helpers;

namespace AcademicAIAssistant.Tests.Services;

public class TextScanServiceTests
{
    [Fact]
    public async Task ScanTextAsync_WithExactDocumentMatch_ReturnsHighRisk()
    {
        await using AppDbContext context = TestDbContextFactory.Create();
        context.Documents.Add(new Document
        {
            UserId = 1,
            Title = "AI Paper",
            ExtractedText = RepeatedAcademicText("AI tools help students improve academic writing and organize research ideas through structured planning, citation practice, argument revision, evidence review, and paragraph development.")
        });
        await context.SaveChangesAsync();

        var service = new TextScanService(context);
        TextScan result = await service.ScanTextAsync(1, "Scan", RepeatedAcademicText("AI tools help students improve academic writing and organize research ideas through structured planning, citation practice, argument revision, evidence review, and paragraph development."));

        Assert.Equal("High", result.RiskLevel);
        Assert.True(result.OverallSimilarityScore >= 60);
        Assert.NotEmpty(result.Matches);
        Assert.Contains(result.Matches, match => match.SourceType == "Document");
    }

    [Fact]
    public async Task ScanTextAsync_WithEssaySource_FindsEssayMatch()
    {
        await using AppDbContext context = TestDbContextFactory.Create();
        context.Essays.Add(new Essay
        {
            UserId = 1,
            Title = "Old Essay",
            Content = RepeatedAcademicText("Academic writing tools support citation practice and research reading through careful source analysis, paragraph planning, evidence selection, topic refinement, and revision.")
        });
        await context.SaveChangesAsync();

        var service = new TextScanService(context);
        TextScan result = await service.ScanTextAsync(1, "Scan", RepeatedAcademicText("Academic writing tools support citation practice and research reading through careful source analysis, paragraph planning, evidence selection, topic refinement, and revision."));

        Assert.Contains(result.Matches, match => match.SourceType == "Essay");
    }

    [Fact]
    public async Task ScanTextAsync_WithOcrSource_FindsOcrMatch()
    {
        await using AppDbContext context = TestDbContextFactory.Create();
        context.OCRScans.Add(new OCRScan
        {
            UserId = 1,
            Title = "OCR Notes",
            ExtractedText = RepeatedAcademicText("Students use structured notes to improve research planning and argument clarity by organizing evidence, comparing sources, refining claims, and preparing academic paragraphs.")
        });
        await context.SaveChangesAsync();

        var service = new TextScanService(context);
        TextScan result = await service.ScanTextAsync(1, "Scan", RepeatedAcademicText("Students use structured notes to improve research planning and argument clarity by organizing evidence, comparing sources, refining claims, and preparing academic paragraphs."));

        Assert.Contains(result.Matches, match => match.SourceType == "OCRScan");
    }

    [Fact]
    public async Task ScanTextAsync_WithNoSourceData_ReturnsLowRiskAndNoMatches()
    {
        await using AppDbContext context = TestDbContextFactory.Create();
        var service = new TextScanService(context);

        TextScan result = await service.ScanTextAsync(1, "Scan", RepeatedAcademicText("This paragraph discusses sports training and physical health routines through exercise planning, nutrition habits, recovery schedules, teamwork skills, and coaching methods."));

        Assert.Equal("Low", result.RiskLevel);
        Assert.Equal(0, result.OverallSimilarityScore);
        Assert.Empty(result.Matches);
    }

    private static string RepeatedAcademicText(string sentence)
    {
        return string.Join(" ", Enumerable.Repeat(sentence, 12));
    }
}
