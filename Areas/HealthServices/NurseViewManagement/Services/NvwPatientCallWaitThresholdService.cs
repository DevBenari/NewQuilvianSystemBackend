using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Services
{
    public class NvwPatientCallWaitThresholdService
    {
        public const int MinWarningAfterMinutes = 1;
        public const int MaxWarningAfterMinutes = 240;
        public const int MaxCriticalAfterMinutes = 480;
        public const int MaxRatificationReferenceLength = 200;

        public const string WaitLevelNone = "None";
        public const string WaitLevelWarning = "Warning";
        public const string WaitLevelCritical = "Critical";

        private const int SecondsPerMinute = 60;
        private const string ReplaceLockKey = "NVW_PATIENT_CALL_WAIT_THRESHOLD_REPLACE";

        private readonly ApplicationDbContext _dbContext;

        public NvwPatientCallWaitThresholdService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public static string ResolveWaitLevel(NvwPatientCallWaitThresholdResponse? threshold, int waitingSeconds)
        {
            if (threshold is not { IsConfigured: true, WarningAfterMinutes: int warningMinutes, CriticalAfterMinutes: int criticalMinutes })
                return WaitLevelNone;

            if (waitingSeconds >= criticalMinutes * SecondsPerMinute)
                return WaitLevelCritical;

            if (waitingSeconds >= warningMinutes * SecondsPerMinute)
                return WaitLevelWarning;

            return WaitLevelNone;
        }

        public static string BuildAuditMessage(NvwPatientCallWaitThresholdReplaceRequest request)
        {
            var change = IsRevocation(request)
                ? "Ambang lama menunggu panggilan pasien dicabut; panggilan tampil tanpa warna."
                : $"Ambang lama menunggu panggilan pasien diganti: kuning {request.WarningAfterMinutes} menit, merah {request.CriticalAfterMinutes} menit.";

            return $"{change} Lingkup: {NvwPatientCallWaitThreshold.GlobalScopeCode}. Rujukan: {request.RatificationReference?.Trim()}; disahkan {request.RatifiedOn?.ToString("yyyy-MM-dd")}.";
        }

        public async Task<NvwPatientCallWaitThresholdResponse> GetActiveAsync(CancellationToken cancellationToken)
        {
            var active = await _dbContext.NvwPatientCallWaitThresholds.AsNoTracking()
                .Where(x => !x.IsDelete && x.ScopeCode == NvwPatientCallWaitThreshold.GlobalScopeCode)
                .OrderByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);

            if (active == null)
                return new NvwPatientCallWaitThresholdResponse { IsConfigured = false };

            var recorder = await _dbContext.Users.AsNoTracking()
                .Where(x => x.Id == active.CreateBy)
                .Select(x => new { x.UserName, x.DisplayName })
                .FirstOrDefaultAsync(cancellationToken);

            return new NvwPatientCallWaitThresholdResponse
            {
                IsConfigured = true,
                WarningAfterMinutes = active.WarningAfterMinutes,
                CriticalAfterMinutes = active.CriticalAfterMinutes,
                RatificationReference = active.RatificationReference,
                RatifiedOn = active.RatifiedOn,
                RecordedByName = string.IsNullOrWhiteSpace(recorder?.DisplayName) ? recorder?.UserName : recorder.DisplayName,
                RecordedAt = active.CreateDateTime
            };
        }

        public async Task<NvwResult<NvwPatientCallWaitThresholdResponse>> ReplaceAsync(
            NvwPatientCallWaitThresholdReplaceRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var revoke = IsRevocation(request);
            if (!revoke && !IsValidPair(request.WarningAfterMinutes, request.CriticalAfterMinutes))
                return NvwResult<NvwPatientCallWaitThresholdResponse>.Invalid(NvwErrorCodes.WaitThresholdInvalid, NvwMessages.WaitThresholdRange);

            var reference = request.RatificationReference?.Trim();
            if (string.IsNullOrEmpty(reference) || reference.Length > MaxRatificationReferenceLength)
                return NvwResult<NvwPatientCallWaitThresholdResponse>.Invalid(NvwErrorCodes.WaitThresholdInvalid, NvwMessages.RatificationReference);

            if (!request.RatifiedOn.HasValue)
                return NvwResult<NvwPatientCallWaitThresholdResponse>.Invalid(NvwErrorCodes.WaitThresholdInvalid, NvwMessages.RatifiedOnRequired);

            if (request.RatifiedOn.Value > DateOnly.FromDateTime(AppDateTimeHelper.OperationalDate()))
                return NvwResult<NvwPatientCallWaitThresholdResponse>.Invalid(NvwErrorCodes.WaitThresholdInvalid, NvwMessages.RatifiedOnFuture);

            var now = DateTime.UtcNow;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            await AcquireReplaceLockAsync(cancellationToken);

            var activeRows = await _dbContext.NvwPatientCallWaitThresholds
                .Where(x => !x.IsDelete && x.ScopeCode == NvwPatientCallWaitThreshold.GlobalScopeCode)
                .ToListAsync(cancellationToken);

            foreach (var row in activeRows)
            {
                row.IsDelete = true;
                row.DeleteDateTime = now;
                row.DeleteBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            if (!revoke)
            {
                _dbContext.NvwPatientCallWaitThresholds.Add(new NvwPatientCallWaitThreshold
                {
                    ScopeCode = NvwPatientCallWaitThreshold.GlobalScopeCode,
                    WarningAfterMinutes = request.WarningAfterMinutes!.Value,
                    CriticalAfterMinutes = request.CriticalAfterMinutes!.Value,
                    RatificationReference = reference,
                    RatifiedOn = request.RatifiedOn.Value,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                });

                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);

            return NvwResult<NvwPatientCallWaitThresholdResponse>.Ok(await GetActiveAsync(cancellationToken));
        }

        private static bool IsRevocation(NvwPatientCallWaitThresholdReplaceRequest request) =>
            !request.WarningAfterMinutes.HasValue && !request.CriticalAfterMinutes.HasValue;

        private static bool IsValidPair(int? warningMinutes, int? criticalMinutes) =>
            warningMinutes.HasValue &&
            criticalMinutes.HasValue &&
            warningMinutes.Value >= MinWarningAfterMinutes &&
            warningMinutes.Value <= MaxWarningAfterMinutes &&
            criticalMinutes.Value > warningMinutes.Value &&
            criticalMinutes.Value <= MaxCriticalAfterMinutes;

        private async Task AcquireReplaceLockAsync(CancellationToken cancellationToken)
        {
            if (!_dbContext.Database.IsNpgsql())
                return;

            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [ReplaceLockKey],
                cancellationToken);
        }
    }
}
