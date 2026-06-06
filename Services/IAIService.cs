using AcademicAIAssistant.Models;

namespace AcademicAIAssistant.Services;

public interface IAIService
{
    bool IsEnabled { get; }

    Task<bool> IsEnabledForUserAsync(int userId);

    Task<string> TestConnectionAsync(int userId);

    Task<bool> TestConnectionAsync(UserAISetting setting);

    Task<string> GenerateDocumentSummaryAsync(string documentText, string language = "en", int? userId = null);

    Task<string> GenerateDocumentSummaryAsync(string documentText, UserAISetting setting);

    Task<string> GenerateWritingFeedbackAsync(string essayType, string essayContent, string language = "en", int? userId = null);

    Task<string> GenerateWritingFeedbackAsync(string essayType, string essayContent, UserAISetting setting);

    Task<string> AnswerQuestionAboutDocumentAsync(string question, string relevantDocumentText, string language = "en", int? userId = null);

    Task<string> AnswerQuestionAboutDocumentAsync(string question, string relevantDocumentText, UserAISetting setting);

    Task<string> GenerateWritingCoachResponseAsync(
        string mode,
        string topic,
        string essayType,
        string thesisStatement,
        string userInput,
        string language,
        UserAISetting setting);
}
