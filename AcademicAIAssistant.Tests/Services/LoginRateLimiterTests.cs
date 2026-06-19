using AcademicAIAssistant.Services;
using Microsoft.Extensions.Caching.Memory;

namespace AcademicAIAssistant.Tests.Services;

public class LoginRateLimiterTests
{
    [Fact]
    public void GetLockoutStatus_InitiallyNotLocked()
    {
        LoginRateLimiter limiter = CreateLimiter();

        LoginRateLimitResult result = limiter.GetLockoutStatus("student@test.com", "127.0.0.1");

        Assert.False(result.IsLockedOut);
    }

    [Fact]
    public void RecordFailedAttempt_AfterFourFailures_DoesNotLock()
    {
        LoginRateLimiter limiter = CreateLimiter();

        for (int i = 0; i < 4; i++)
        {
            limiter.RecordFailedAttempt("student@test.com", "127.0.0.1");
        }

        Assert.False(limiter.GetLockoutStatus("student@test.com", "127.0.0.1").IsLockedOut);
    }

    [Fact]
    public void RecordFailedAttempt_AfterFiveFailures_LocksOut()
    {
        LoginRateLimiter limiter = CreateLimiter();

        for (int i = 0; i < 5; i++)
        {
            limiter.RecordFailedAttempt("student@test.com", "127.0.0.1");
        }

        LoginRateLimitResult result = limiter.GetLockoutStatus("student@test.com", "127.0.0.1");

        Assert.True(result.IsLockedOut);
        Assert.True(result.RemainingMinutes > 0);
    }

    [Fact]
    public void ResetAttempts_RemovesCounterAndLockout()
    {
        LoginRateLimiter limiter = CreateLimiter();
        for (int i = 0; i < 5; i++)
        {
            limiter.RecordFailedAttempt("student@test.com", "127.0.0.1");
        }

        limiter.ResetAttempts("student@test.com", "127.0.0.1");

        Assert.False(limiter.GetLockoutStatus("student@test.com", "127.0.0.1").IsLockedOut);
        Assert.Equal(0, limiter.GetFailedAttemptCount("student@test.com", "127.0.0.1"));
    }

    private static LoginRateLimiter CreateLimiter()
    {
        return new LoginRateLimiter(new MemoryCache(new MemoryCacheOptions()));
    }
}
