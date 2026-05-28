namespace AcademicAIAssistant.Services;

public interface IAIService
{
    bool IsEnabled { get; }

    Task<string> GenerateDocumentSummaryAsync(string documentText);

    Task<string> GenerateWritingFeedbackAsync(string essayType, string essayContent);

    Task<string> AnswerQuestionAboutDocumentAsync(string question, string relevantDocumentText);
}
