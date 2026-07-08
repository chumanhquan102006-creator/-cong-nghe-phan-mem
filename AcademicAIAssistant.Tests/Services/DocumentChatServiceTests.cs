using AcademicAIAssistant.Data;
using AcademicAIAssistant.Services;
using AcademicAIAssistant.Tests.Helpers;
using Moq;

namespace AcademicAIAssistant.Tests.Services;

public class DocumentChatServiceTests
{
    [Theory]
    [InlineData("AI là gì?")]
    [InlineData("What is this paper about?")]
    [InlineData("Why is AI useful?")]
    [InlineData("Limitations?")]
    public async Task AnswerQuestionAsync_DoesNotRejectValidShortQuestions(string question)
    {
        await using AppDbContext context = TestDbContextFactory.Create();
        var service = new DocumentChatService(
            new KeywordExtractionService(),
            Mock.Of<IAIService>(),
            context);

        const string documentText = """
            Abstract

            This paper studies artificial intelligence in academic writing and explains how AI can help students review drafts, summarize research papers, and organize feedback.

            Benefits

            AI is useful because it can quickly identify patterns, highlight unclear writing, and support students while they remain responsible for final academic decisions.

            Limitations

            The document states that AI feedback may miss context, can produce incomplete suggestions, and should be checked against the original source material.
            """;

        DocumentChatResponse response = await service.AnswerQuestionAsync(question, documentText);

        Assert.DoesNotContain("Please enter a more specific question.", response.Answer);
        Assert.False(string.IsNullOrWhiteSpace(response.Answer));
    }
}
