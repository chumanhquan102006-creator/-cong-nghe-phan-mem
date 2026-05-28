using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AcademicAIAssistant.Models;
using Microsoft.Extensions.Options;

namespace AcademicAIAssistant.Services;

public class AIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly AISettings _settings;
    private readonly ILogger<AIService> _logger;

    public AIService(HttpClient httpClient, IOptions<AISettings> settings, ILogger<AIService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        if (_settings.TimeoutSeconds > 0)
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        }
    }

    public bool IsEnabled =>
        _settings.Enabled
        && !string.IsNullOrWhiteSpace(_settings.ApiKey)
        && !string.IsNullOrWhiteSpace(_settings.Endpoint)
        && !string.IsNullOrWhiteSpace(_settings.Model);

    public Task<string> GenerateDocumentSummaryAsync(string documentText)
    {
        string prompt = """
            Summarize the research document below. Return plain text with these sections:
            - Short summary
            - Research problem
            - Methodology
            - Main findings
            - Limitations
            - Keywords
            """;

        return SendChatAsync(prompt, Truncate(documentText, 8000));
    }

    public Task<string> GenerateWritingFeedbackAsync(string essayType, string essayContent)
    {
        string prompt = $"""
            You are an academic writing tutor. Review this {essayType}.
            Return plain text feedback with these sections:
            - Grammar / Wording
            - Academic Tone
            - Thesis Statement
            - Structure
            - Logic
            - Citation
            - General Suggestions
            - Estimated Score 0-100
            """;

        return SendChatAsync(prompt, Truncate(essayContent, 8000));
    }

    public Task<string> AnswerQuestionAboutDocumentAsync(string question, string relevantDocumentText)
    {
        string prompt = $"""
            Answer the user's question using only the document text provided.
            If the answer is not clearly supported by the text, say that the document does not contain enough information.
            Keep the answer concise and easy to understand.

            Question:
            {question}
            """;

        return SendChatAsync(prompt, Truncate(relevantDocumentText, 5000));
    }

    private async Task<string> SendChatAsync(string systemPrompt, string userContent)
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException("AI service is disabled or missing configuration.");
        }

        var requestBody = new
        {
            model = _settings.Model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userContent }
            },
            temperature = 0.2
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _settings.Endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        try
        {
            using HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("AI request failed with status {StatusCode}: {Body}", response.StatusCode, errorBody);
                throw new InvalidOperationException("AI request failed.");
            }

            string json = await response.Content.ReadAsStringAsync();
            using JsonDocument document = JsonDocument.Parse(json);

            string? content = document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("AI response was empty.");
            }

            return content.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI service call failed. Falling back to rule-based logic.");
            throw;
        }
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
