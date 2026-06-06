namespace AcademicAIAssistant.Services;

public interface ILoginRateLimiter
{
    LoginRateLimitResult GetLockoutStatus(string email, string ipAddress);

    void RecordFailedAttempt(string email, string ipAddress);

    void ResetAttempts(string email, string ipAddress);

    int GetFailedAttemptCount(string email, string ipAddress);
}
