using Microsoft.Extensions.Caching.Memory;

namespace AcademicAIAssistant.Services;

public class LoginRateLimiter : ILoginRateLimiter
{
    private const int MaxFailedAttempts = 5;
    private const int LockoutDurationMinutes = 15;
    private const int TrackingWindowMinutes = 15;

    private readonly IMemoryCache _cache;

    public LoginRateLimiter(IMemoryCache cache)
    {
        _cache = cache;
    }

    public LoginRateLimitResult GetLockoutStatus(string email, string ipAddress)
    {
        DateTimeOffset? emailLockoutEnd = GetLockoutEnd(GetEmailLockKey(email));
        DateTimeOffset? ipLockoutEnd = GetLockoutEnd(GetIpLockKey(ipAddress));

        DateTimeOffset? lockoutEnd = new[] { emailLockoutEnd, ipLockoutEnd }
            .Where(value => value.HasValue)
            .Max();

        if (!lockoutEnd.HasValue || lockoutEnd.Value <= DateTimeOffset.UtcNow)
        {
            return new LoginRateLimitResult { IsLockedOut = false };
        }

        int remainingMinutes = Math.Max(1, (int)Math.Ceiling((lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes));
        return new LoginRateLimitResult
        {
            IsLockedOut = true,
            RemainingMinutes = remainingMinutes,
            Reason = "Too many failed login attempts."
        };
    }

    public void RecordFailedAttempt(string email, string ipAddress)
    {
        int emailAttempts = IncrementAttempt(GetEmailFailKey(email));
        int ipAttempts = IncrementAttempt(GetIpFailKey(ipAddress));

        if (emailAttempts >= MaxFailedAttempts)
        {
            SetLockout(GetEmailLockKey(email));
        }

        if (ipAttempts >= MaxFailedAttempts)
        {
            SetLockout(GetIpLockKey(ipAddress));
        }
    }

    public void ResetAttempts(string email, string ipAddress)
    {
        _cache.Remove(GetEmailFailKey(email));
        _cache.Remove(GetEmailLockKey(email));
        _cache.Remove(GetIpFailKey(ipAddress));
        _cache.Remove(GetIpLockKey(ipAddress));
    }

    public int GetFailedAttemptCount(string email, string ipAddress)
    {
        int emailAttempts = _cache.TryGetValue(GetEmailFailKey(email), out int emailCount) ? emailCount : 0;
        int ipAttempts = _cache.TryGetValue(GetIpFailKey(ipAddress), out int ipCount) ? ipCount : 0;

        return Math.Max(emailAttempts, ipAttempts);
    }

    private int IncrementAttempt(string key)
    {
        int attempts = _cache.TryGetValue(key, out int currentValue) ? currentValue : 0;
        attempts++;

        _cache.Set(key, attempts, TimeSpan.FromMinutes(TrackingWindowMinutes));
        return attempts;
    }

    private void SetLockout(string key)
    {
        DateTimeOffset lockoutEnd = DateTimeOffset.UtcNow.AddMinutes(LockoutDurationMinutes);
        _cache.Set(key, lockoutEnd, TimeSpan.FromMinutes(LockoutDurationMinutes));
    }

    private DateTimeOffset? GetLockoutEnd(string key)
    {
        return _cache.TryGetValue(key, out DateTimeOffset lockoutEnd) ? lockoutEnd : null;
    }

    private static string GetEmailFailKey(string email)
    {
        return $"login_fail_email:{NormalizeEmail(email)}";
    }

    private static string GetEmailLockKey(string email)
    {
        return $"login_lock_email:{NormalizeEmail(email)}";
    }

    private static string GetIpFailKey(string ipAddress)
    {
        return $"login_fail_ip:{NormalizeIp(ipAddress)}";
    }

    private static string GetIpLockKey(string ipAddress)
    {
        return $"login_lock_ip:{NormalizeIp(ipAddress)}";
    }

    private static string NormalizeEmail(string email)
    {
        return string.IsNullOrWhiteSpace(email) ? "empty" : email.Trim().ToLowerInvariant();
    }

    private static string NormalizeIp(string ipAddress)
    {
        return string.IsNullOrWhiteSpace(ipAddress) ? "unknown" : ipAddress.Trim();
    }
}
