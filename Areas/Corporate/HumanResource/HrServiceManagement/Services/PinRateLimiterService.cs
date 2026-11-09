using System.Collections.Concurrent;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services
{
    public class RateLimitCheckResult
    {
        public bool IsBlocked { get; set; }
        public int RemainingAttempts { get; set; } = 3;
        public TimeSpan? LockoutTimeRemaining { get; set; }
        public string? BlockedMessage { get; set; }
    }

    public interface IPinRateLimiterService
    {
        RateLimitCheckResult CheckRateLimit(string verificationHash, string clientIp);
        void RecordFailedAttempt(string verificationHash, string clientIp);
        void ResetAttempts(string verificationHash, string clientIp);
    }

    public class PinRateLimiterService : IPinRateLimiterService
    {
        private class AttemptRecord
        {
            public int FailedCount { get; set; }
            public DateTime? LockedUntilUtc { get; set; }
            public DateTime LastAttemptUtc { get; set; }
        }

        private static readonly ConcurrentDictionary<string, AttemptRecord> AttemptTracker = new();
        private const int MaxFailedAttempts = 3;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(30);

        private static string BuildKey(string verificationHash, string clientIp)
        {
            return $"{verificationHash.Trim().ToLowerInvariant()}_{clientIp.Trim()}";
        }

        public RateLimitCheckResult CheckRateLimit(string verificationHash, string clientIp)
        {
            var key = BuildKey(verificationHash, clientIp);
            var now = DateTime.UtcNow;

            if (AttemptTracker.TryGetValue(key, out var record))
            {
                if (record.LockedUntilUtc.HasValue)
                {
                    if (now < record.LockedUntilUtc.Value)
                    {
                        var remaining = record.LockedUntilUtc.Value - now;
                        return new RateLimitCheckResult
                        {
                            IsBlocked = true,
                            RemainingAttempts = 0,
                            LockoutTimeRemaining = remaining,
                            BlockedMessage = $"Akses pembukaan PIN finansial diblokir sementara selama 30 menit karena salah memasukkan PIN sebanyak 3 kali berturut-turut (VR-HRS-12). Sisa waktu blokir: {Math.Ceiling(remaining.TotalMinutes)} menit."
                        };
                    }
                    else
                    {
                        // Masa blokir 30 menit sudah habis, reset percobaan
                        record.FailedCount = 0;
                        record.LockedUntilUtc = null;
                    }
                }

                return new RateLimitCheckResult
                {
                    IsBlocked = false,
                    RemainingAttempts = Math.Max(0, MaxFailedAttempts - record.FailedCount)
                };
            }

            return new RateLimitCheckResult
            {
                IsBlocked = false,
                RemainingAttempts = MaxFailedAttempts
            };
        }

        public void RecordFailedAttempt(string verificationHash, string clientIp)
        {
            var key = BuildKey(verificationHash, clientIp);
            var now = DateTime.UtcNow;

            AttemptTracker.AddOrUpdate(
                key,
                _ => new AttemptRecord
                {
                    FailedCount = 1,
                    LastAttemptUtc = now
                },
                (_, existing) =>
                {
                    existing.FailedCount++;
                    existing.LastAttemptUtc = now;
                    if (existing.FailedCount >= MaxFailedAttempts)
                    {
                        existing.LockedUntilUtc = now.Add(LockoutDuration);
                    }
                    return existing;
                });
        }

        public void ResetAttempts(string verificationHash, string clientIp)
        {
            var key = BuildKey(verificationHash, clientIp);
            AttemptTracker.TryRemove(key, out _);
        }
    }
}
