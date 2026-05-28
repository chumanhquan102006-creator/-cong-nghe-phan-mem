namespace AcademicAIAssistant.Models;

public class AISettings
{
    public string Provider { get; set; } = "OpenAI";

    public string ApiKey { get; set; } = string.Empty;

    public string Endpoint { get; set; } = "https://api.openai.com/v1/chat/completions";

    public string Model { get; set; } = "gpt-4o-mini";

    public bool Enabled { get; set; }

    public int TimeoutSeconds { get; set; } = 30;
}
