using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AcademicAIAssistant.Data;
using AcademicAIAssistant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AcademicAIAssistant.Services;

public class AIService : IAIService
{
    private const string DefaultGeminiModel = "gemini-1.5-flash";

    private readonly HttpClient _httpClient;
    private readonly AISettings _settings;
    private readonly ILogger<AIService> _logger;
    private readonly AppDbContext _context;

    public AIService(HttpClient httpClient, IOptions<AISettings> settings, ILogger<AIService> logger, AppDbContext context)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
        _context = context;

        int timeoutSeconds = _settings.TimeoutSeconds > 0 ? _settings.TimeoutSeconds : 30;
        _httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
    }

    public bool IsEnabled => IsConfigured(_settings);

    public async Task<bool> IsEnabledForUserAsync(int userId)
    {
        UserAISetting? userSetting = await GetUserSettingAsync(userId);
        return userSetting == null ? IsConfigured(_settings) : IsConfigured(userSetting);
    }

    public async Task<string> TestConnectionAsync(int userId)
    {
        UserAISetting? userSetting = await GetUserSettingAsync(userId);
        string prompt = "Reply with OK if the connection works.";

        return userSetting == null
            ? await CallProviderAsync(prompt, ToUserSetting(_settings))
            : await CallProviderAsync(prompt, userSetting);
    }

    public async Task<bool> TestConnectionAsync(UserAISetting setting)
    {
        string response = await CallProviderAsync("Reply with OK if the connection works.", setting);
        return !string.IsNullOrWhiteSpace(response);
    }

    public async Task<string> GenerateDocumentSummaryAsync(string documentText, string language = "en", int? userId = null)
    {
        UserAISetting? userSetting = userId.HasValue ? await GetUserSettingAsync(userId.Value) : null;
        string prompt = BuildDocumentSummaryPrompt(language, Truncate(documentText, 8000));

        return userSetting == null
            ? await CallProviderAsync(prompt, ToUserSetting(_settings))
            : await CallProviderAsync(prompt, userSetting);
    }

    public async Task<string> GenerateDocumentSummaryAsync(string documentText, UserAISetting setting)
    {
        string prompt = BuildDocumentSummaryPrompt("en", Truncate(documentText, 8000));
        return await CallProviderAsync(prompt, setting);
    }

    public async Task<string> GenerateWritingFeedbackAsync(string essayType, string essayContent, string language = "en", int? userId = null)
    {
        UserAISetting? userSetting = userId.HasValue ? await GetUserSettingAsync(userId.Value) : null;
        string prompt = BuildWritingFeedbackPrompt(essayType, essayContent, language);

        return userSetting == null
            ? await CallProviderAsync(prompt, ToUserSetting(_settings))
            : await CallProviderAsync(prompt, userSetting);
    }

    public async Task<string> GenerateWritingFeedbackAsync(string essayType, string essayContent, UserAISetting setting)
    {
        string prompt = BuildWritingFeedbackPrompt(essayType, essayContent, "en");
        return await CallProviderAsync(prompt, setting);
    }

    public async Task<string> AnswerQuestionAboutDocumentAsync(string question, string relevantDocumentText, string language = "en", int? userId = null)
    {
        UserAISetting? userSetting = userId.HasValue ? await GetUserSettingAsync(userId.Value) : null;
        string prompt = BuildDocumentQuestionPrompt(question, relevantDocumentText, language);

        return userSetting == null
            ? await CallProviderAsync(prompt, ToUserSetting(_settings))
            : await CallProviderAsync(prompt, userSetting);
    }

    public async Task<string> AnswerQuestionAboutDocumentAsync(string question, string relevantDocumentText, UserAISetting setting)
    {
        string prompt = BuildDocumentQuestionPrompt(question, relevantDocumentText, "en");
        return await CallProviderAsync(prompt, setting);
    }

    public async Task<string> GenerateWritingCoachResponseAsync(
        string mode,
        string topic,
        string essayType,
        string thesisStatement,
        string userInput,
        string language,
        UserAISetting setting)
    {
        string prompt = BuildWritingCoachPrompt(mode, topic, essayType, thesisStatement, userInput, language);
        return await CallProviderAsync(prompt, setting);
    }

    public async Task<string> CallGeminiAsync(string prompt, UserAISetting setting)
    {
        ValidateUserSetting(setting);

        string model = string.IsNullOrWhiteSpace(setting.ModelName)
            ? DefaultGeminiModel
            : setting.ModelName.Trim();

        string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(model)}:generateContent?key={Uri.EscapeDataString(setting.ApiKey)}";
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        try
        {
            using HttpResponseMessage response = await _httpClient.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(GetGeminiErrorMessage(response.StatusCode, json));
            }

            string text = ExtractGeminiText(json);
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new InvalidOperationException("Gemini returned an empty response.");
            }

            return text.Trim();
        }
        catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Gemini request timed out.");
            throw new InvalidOperationException("Gemini request timed out. Please check your network.", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Network error while connecting to Gemini.");
            throw new InvalidOperationException("Network error while connecting to Gemini.", ex);
        }
    }

    private async Task<string> CallProviderAsync(string prompt, UserAISetting setting)
    {
        if (setting.Provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
        {
            return await CallGeminiAsync(prompt, setting);
        }

        AISettings openAiSettings = new()
        {
            Provider = setting.Provider,
            ApiKey = setting.ApiKey,
            Endpoint = _settings.Endpoint,
            Model = string.IsNullOrWhiteSpace(setting.ModelName) ? _settings.Model : setting.ModelName,
            Enabled = setting.IsEnabled,
            TimeoutSeconds = _settings.TimeoutSeconds
        };

        return await CallOpenAiCompatibleAsync(prompt, string.Empty, openAiSettings);
    }

    private async Task<string> CallOpenAiCompatibleAsync(string systemPrompt, string userContent, AISettings settings)
    {
        if (!IsConfigured(settings))
        {
            throw new InvalidOperationException("AI service is disabled or missing configuration.");
        }

        var requestBody = new
        {
            model = settings.Model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userContent }
            },
            temperature = 0.2
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, settings.Endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        try
        {
            using HttpResponseMessage response = await _httpClient.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenAI-compatible request failed with status {StatusCode}.", response.StatusCode);
                throw new InvalidOperationException(GetOpenAiCompatibleErrorMessage(response.StatusCode));
            }

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
        catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "AI request timed out.");
            throw new InvalidOperationException("AI request timed out. Please check your network.", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Network error while connecting to AI provider.");
            throw new InvalidOperationException("Network error while connecting to AI provider.", ex);
        }
    }

    private async Task<UserAISetting?> GetUserSettingAsync(int userId)
    {
        return await _context.UserAISettings
            .AsNoTracking()
            .FirstOrDefaultAsync(setting => setting.UserId == userId);
    }

    private static void ValidateUserSetting(UserAISetting setting)
    {
        if (!setting.IsEnabled)
        {
            throw new InvalidOperationException("AI mode is disabled. Enable AI mode before testing.");
        }

        if (string.IsNullOrWhiteSpace(setting.ApiKey))
        {
            throw new InvalidOperationException("API key is missing.");
        }
    }

    private static string ExtractGeminiText(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);

        if (!document.RootElement.TryGetProperty("candidates", out JsonElement candidates)
            || candidates.ValueKind != JsonValueKind.Array
            || candidates.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("Gemini returned an empty response.");
        }

        JsonElement firstCandidate = candidates[0];
        if (!firstCandidate.TryGetProperty("content", out JsonElement content)
            || !content.TryGetProperty("parts", out JsonElement parts)
            || parts.ValueKind != JsonValueKind.Array
            || parts.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("Gemini returned an empty response.");
        }

        foreach (JsonElement part in parts.EnumerateArray())
        {
            if (part.TryGetProperty("text", out JsonElement textElement))
            {
                string? text = textElement.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }
        }

        throw new InvalidOperationException("Gemini returned an empty response.");
    }

    private static string BuildDocumentSummaryPrompt(string language, string documentText)
    {
        return $"""
            Summarize the research document below. Return plain text with exactly these sections:

            Short Summary:
            Research Problem:
            Methodology:
            Main Findings:
            Limitations:
            Keywords:

            Response language: {GetLanguageName(language)}.

            Document text:
            {documentText}
            """;
    }

    private static string BuildWritingFeedbackPrompt(string essayType, string essayContent, string language)
    {
        return $$"""
            You are an academic writing tutor. Review this {{essayType}}.

            Return ONLY valid JSON with this schema. Do not wrap it in markdown.
            {
              "overallScore": 0,
              "grammarFeedback": "",
              "academicToneFeedback": "",
              "thesisFeedback": "",
              "structureFeedback": "",
              "logicFeedback": "",
              "citationFeedback": "",
              "generalSuggestions": ""
            }

            The score must be from 0 to 100.
            Response language for feedback values: {{GetLanguageName(language)}}.

            Essay:
            {{Truncate(essayContent, 8000)}}
            """;
    }

    private static string BuildDocumentQuestionPrompt(string question, string relevantDocumentText, string language)
    {
        return $"""
            Answer the question using only the source snippet below.
            If the source snippet does not contain the information, reply exactly:
            "I could not find this information in the document."
            Keep the answer concise and clear.
            Response language: {GetLanguageName(language)}.

            Question:
            {question}

            Source snippet:
            {Truncate(relevantDocumentText, 5000)}
            """;
    }

    private static string BuildWritingCoachPrompt(string mode, string topic, string essayType, string thesisStatement, string userInput, string language)
    {
        string responseLanguage = GetLanguageName(language);
        string task = mode switch
        {
            "Outline" => """
                Create a detailed academic outline. Include:
                1. Introduction
                2. Thesis statement
                3. Body paragraph 1: main idea, evidence needed, explanation
                4. Body paragraph 2: main idea, evidence needed, explanation
                5. Body paragraph 3: main idea, evidence needed, explanation
                6. Counter-argument
                7. Rebuttal
                8. Conclusion
                Do not write the full essay.
                """,
            "CounterArgument" => """
                Generate counter-arguments based on the thesis or main claim. Include:
                1. Three counter-arguments
                2. Why an opposing reader might use each argument
                3. A rebuttal strategy for each counter-argument
                4. Academic transition sentences
                Do not invent specific sources.
                """,
            "ThesisImprover" => """
                Evaluate and improve the thesis statement. Include:
                1. What is currently too broad, unclear, or weak
                2. Whether it lacks a clear position, scope, or academic focus
                3. Three rewritten versions:
                   - Basic improved version
                   - Strong academic version
                   - Research-focused version
                4. Explain why the new versions are better.
                """,
            _ => """
                Help the student brainstorm. Include:
                1. Five to ten possible angles for the topic
                2. Five research questions
                3. Three possible thesis statements
                4. A recommendation about which direction is most suitable
                """
        };

        return $"""
            You are an academic writing coach for students.
            Your role is to guide the student's thinking step by step.
            Do not write a full essay for the student.
            Do not invent fake citations or sources.
            Give structured, readable guidance.
            Response language: {responseLanguage}.

            Mode: {mode}
            Essay type: {essayType}
            Topic: {topic}
            Thesis statement: {thesisStatement}
            Additional instructions: {userInput}

            Task:
            {task}
            """;
    }

    private static string GetGeminiErrorMessage(HttpStatusCode statusCode, string responseBody)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => "Gemini request failed: Bad request. Please check the model name and request format.",
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => "Gemini request failed: API key is invalid or does not have permission.",
            HttpStatusCode.NotFound => "Gemini request failed: Model not found. Please check the model name.",
            (HttpStatusCode)429 => "Gemini request failed: quota or rate limit exceeded.",
            _ => $"Gemini request failed: {ExtractSafeErrorMessage(responseBody)}"
        };
    }

    private static string GetOpenAiCompatibleErrorMessage(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => "AI request failed: Bad request. Please check the model name and request format.",
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => "AI request failed: API key is invalid or does not have permission.",
            HttpStatusCode.NotFound => "AI request failed: Model or endpoint not found.",
            (HttpStatusCode)429 => "AI request failed: quota or rate limit exceeded.",
            _ => "AI request failed."
        };
    }

    private static string ExtractSafeErrorMessage(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return "Unknown error.";
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(responseBody);
            if (document.RootElement.TryGetProperty("error", out JsonElement error))
            {
                if (error.TryGetProperty("message", out JsonElement message))
                {
                    return Truncate(message.GetString() ?? "Unknown error.", 180);
                }

                if (error.ValueKind == JsonValueKind.String)
                {
                    return Truncate(error.GetString() ?? "Unknown error.", 180);
                }
            }
        }
        catch (JsonException)
        {
            // Use a trimmed plain-text error below if the provider did not return JSON.
        }

        return Truncate(responseBody.Replace("\r", " ").Replace("\n", " "), 180);
    }

    private static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static string GetLanguageName(string language)
    {
        return language.Equals("vi", StringComparison.OrdinalIgnoreCase) ? "Vietnamese" : "English";
    }

    private static bool IsConfigured(AISettings settings)
    {
        if (settings.Provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
        {
            return settings.Enabled
                && !string.IsNullOrWhiteSpace(settings.ApiKey)
                && !string.IsNullOrWhiteSpace(settings.Model);
        }

        return settings.Enabled
            && !string.IsNullOrWhiteSpace(settings.ApiKey)
            && !string.IsNullOrWhiteSpace(settings.Endpoint)
            && !string.IsNullOrWhiteSpace(settings.Model);
    }

    private static bool IsConfigured(UserAISetting setting)
    {
        return setting.IsEnabled && !string.IsNullOrWhiteSpace(setting.ApiKey);
    }

    private static UserAISetting ToUserSetting(AISettings settings)
    {
        return new UserAISetting
        {
            Provider = settings.Provider,
            ApiKey = settings.ApiKey,
            ModelName = settings.Model,
            IsEnabled = settings.Enabled
        };
    }
}
