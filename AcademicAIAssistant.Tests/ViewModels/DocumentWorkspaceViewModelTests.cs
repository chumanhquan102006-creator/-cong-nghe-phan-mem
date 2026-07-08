using AcademicAIAssistant.Models;
using AcademicAIAssistant.Models.ViewModels;

namespace AcademicAIAssistant.Tests.ViewModels;

public class DocumentWorkspaceViewModelTests
{
    [Theory]
    [InlineData(null, DocumentWorkspaceViewModel.OverviewTab)]
    [InlineData("", DocumentWorkspaceViewModel.OverviewTab)]
    [InlineData("overview", DocumentWorkspaceViewModel.OverviewTab)]
    [InlineData("summary", DocumentWorkspaceViewModel.SummaryTab)]
    [InlineData("chat", DocumentWorkspaceViewModel.ChatTab)]
    [InlineData("SUMMARY", DocumentWorkspaceViewModel.SummaryTab)]
    [InlineData("unknown", DocumentWorkspaceViewModel.OverviewTab)]
    public void NormalizeTab_ReturnsSupportedTabOrOverview(string? tab, string expected)
    {
        Assert.Equal(expected, DocumentWorkspaceViewModel.NormalizeTab(tab));
    }

    [Fact]
    public void StatusProperties_ReflectUploadedOnlyDocument()
    {
        var viewModel = new DocumentWorkspaceViewModel
        {
            Document = new Document()
        };

        Assert.False(viewModel.HasExtractedText);
        Assert.False(viewModel.HasSummary);
    }

    [Fact]
    public void StatusProperties_ReflectExtractedDocumentWithoutSummary()
    {
        var viewModel = new DocumentWorkspaceViewModel
        {
            Document = new Document
            {
                ExtractedText = "Extracted document text."
            }
        };

        Assert.True(viewModel.HasExtractedText);
        Assert.False(viewModel.HasSummary);
    }

    [Fact]
    public void StatusProperties_ReflectSummarizedDocument()
    {
        var viewModel = new DocumentWorkspaceViewModel
        {
            Document = new Document
            {
                ExtractedText = "Extracted document text.",
                Summary = "This is the first sentence. This is the second sentence. This is the third sentence. This sentence should stay out."
            }
        };

        Assert.True(viewModel.HasExtractedText);
        Assert.True(viewModel.HasSummary);
        Assert.Equal("This is the first sentence. This is the second sentence. This is the third sentence.", viewModel.SummaryPreview);
    }
}
