using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services
{
    public class RateLimitCheckResult
    {
        public bool IsBlocked { get; set; }
        public int RemainingAttempts { get; set; } = 3;
        public TimeSpan? LockoutTimeRemaining { get; set; }
        public string? BlockedMessage { get; set; }
    }

    /// <summary>
    /// Pembatas percobaan PIN pembukaan nominal surat finansial di portal verifikasi publik (VR-HRS-12, BE-HRD-204):
    /// maksimal 3 kali salah per dokumen per alamat IP, lalu terkunci 30 menit.
    /// </summary>
    public interface IPinRateLimiterService
    {
        Task<RateLimitCheckResult> CheckRateLimitAsync(string verificationHash, string clientIp, CancellationToken cancellationToken = default);
        Task RecordFailedAttemptAsync(string verificationHash, string clientIp, CancellationToken cancellationToken = default);
        Task ResetAttemptsAsync(string verificationHash, string clientIp, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Implementasi pembatas percobaan PIN yang penghitungnya disimpan di tabel <c>HrdPinAttemptRecord</c>.
    /// </summary>
    /// <remarks>
    /// Sebelum 10 November 2026 penghitung disimpan di <c>ConcurrentDictionary</c> statis: penyerang yang terkunci cukup
    /// menunggu aplikasi di-restart (atau permintaannya jatuh ke instance lain) untuk mencoba lagi. Kini penguncian
    /// bertahan di database dan berlaku untuk seluruh instance.
    /// </remarks>
    public class PinRateLimiterService : IPinRateLimiterService
    {
        private const int MaxFailedAttempts = 3;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(30);

        private readonly ApplicationDbContext _dbContext;

        public PinRateLimiterService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        private static string BuildKey(string verificationHash, string clientIp)
        {
            return $"{verificationHash.Trim().ToLowerInvariant()}_{clientIp.Trim()}";
        }

        public async Task<RateLimitCheckResult> CheckRateLimitAsync(string verificationHash, string clientIp, CancellationToken cancellationToken = default)
        {
            var key = BuildKey(verificationHash, clientIp);
            var now = DateTime.UtcNow;

            var record = await _dbContext.HrdPinAttemptRecords
                .FirstOrDefaultAsync(x => x.AttemptKey == key && !x.IsDelete, cancellationToken);

            if (record != null)
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

                    // Masa blokir 30 menit sudah habis, reset percobaan
                    record.FailedCount = 0;
                    record.LockedUntilUtc = null;
                    record.UpdateDateTime = now;
                    await _dbContext.SaveChangesAsync(cancellationToken);
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

        public async Task RecordFailedAttemptAsync(string verificationHash, string clientIp, CancellationToken cancellationToken = default)
        {
            var key = BuildKey(verificationHash, clientIp);
            var now = DateTime.UtcNow;

            var record = await _dbContext.HrdPinAttemptRecords
                .FirstOrDefaultAsync(x => x.AttemptKey == key && !x.IsDelete, cancellationToken);

            if (record == null)
            {
                record = new HrdPinAttemptRecord
                {
                    Id = Guid.NewGuid(),
                    AttemptKey = key,
                    VerificationHash = verificationHash.Trim().ToLowerInvariant(),
                    ClientIp = clientIp.Trim(),
                    FailedCount = 0,
                    CreateDateTime = now
                };
                _dbContext.HrdPinAttemptRecords.Add(record);
            }

            record.FailedCount++;
            record.LastAttemptUtc = now;
            record.UpdateDateTime = now;
            if (record.FailedCount >= MaxFailedAttempts)
            {
                record.LockedUntilUtc = now.Add(LockoutDuration);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task ResetAttemptsAsync(string verificationHash, string clientIp, CancellationToken cancellationToken = default)
        {
            var key = BuildKey(verificationHash, clientIp);
            var records = await _dbContext.HrdPinAttemptRecords
                .Where(x => x.AttemptKey == key && !x.IsDelete)
                .ToListAsync(cancellationToken);

            if (records.Count == 0) return;

            _dbContext.HrdPinAttemptRecords.RemoveRange(records);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
