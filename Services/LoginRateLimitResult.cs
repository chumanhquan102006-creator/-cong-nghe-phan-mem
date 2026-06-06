namespace AcademicAIAssistant.Services;

public class LoginRateLimitResult
{
    public bool IsLockedOut { get; set; }

    public int RemainingMinutes { get; set; }

    public string Reason { get; set; } = string.Empty;
}
