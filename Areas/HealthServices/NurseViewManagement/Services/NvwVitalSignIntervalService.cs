using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Services
{
    public class NvwVitalSignIntervalService
    {
        public const int MinIntervalMinutes = 1;
        public const int MaxIntervalMinutes = 1440;
        public const int MaxRatificationReferenceLength = 200;

        private const string ReplaceLockKey = "NVW_VITAL_SIGN_INTERVAL_REPLACE";

        private readonly ApplicationDbContext _dbContext;

        public NvwVitalSignIntervalService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public static string LevelLabel(EwsRiskLevel level) => level switch
        {
            EwsRiskLevel.Low => "Rendah",
            EwsRiskLevel.Medium => "Sedang",
            EwsRiskLevel.High => "Tinggi",
            EwsRiskLevel.Critical => "Kritis",
            _ => level.ToString()
        };

        public static string BuildAuditMessage(NvwVitalSignIntervalReplaceRequest request)
        {
            var items = (request.Items ?? new List<NvwVitalSignIntervalItemRequest>())
                .OrderBy(x => x.EwsRiskLevel)
                .Select(x => $"{LevelLabel(x.EwsRiskLevel)}={x.IntervalMinutes}")
                .ToList();

            var change = items.Count == 0
                ? "Seluruh interval dicabut."
                : $"Interval ukur ulang TTV diganti: {string.Join(", ", items)}.";

            return $"{change} Rujukan: {request.RatificationReference?.Trim()}; disahkan {request.RatifiedOn?.ToString("yyyy-MM-dd")}.";
        }

        public async Task<NvwVitalSignIntervalSetResponse> GetActiveAsync(CancellationToken cancellationToken)
        {
            var rows = await _dbContext.NvwVitalSignIntervals.AsNoTracking()
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.EwsRiskLevel)
                .ToListAsync(cancellationToken);

            if (rows.Count == 0)
                return new NvwVitalSignIntervalSetResponse { IsConfigured = false };

            var latest = rows.OrderByDescending(x => x.CreateDateTime).First();
            var recorder = await _dbContext.Users.AsNoTracking()
                .Where(x => x.Id == latest.CreateBy)
                .Select(x => new { x.UserName, x.DisplayName })
                .FirstOrDefaultAsync(cancellationToken);

            return new NvwVitalSignIntervalSetResponse
            {
                Items = rows.Select(x => new NvwVitalSignIntervalItemResponse
                {
                    EwsRiskLevel = x.EwsRiskLevel,
                    EwsRiskLevelLabel = LevelLabel(x.EwsRiskLevel),
                    IntervalMinutes = x.IntervalMinutes
                }).ToList(),
                RatificationReference = latest.RatificationReference,
                RatifiedOn = latest.RatifiedOn,
                RecordedByName = string.IsNullOrWhiteSpace(recorder?.DisplayName) ? recorder?.UserName : recorder.DisplayName,
                RecordedAt = latest.CreateDateTime,
                IsConfigured = true
            };
        }

        public async Task<NvwResult<NvwVitalSignIntervalSetResponse>> ReplaceAsync(
            NvwVitalSignIntervalReplaceRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            if (request.Items == null)
                return NvwResult<NvwVitalSignIntervalSetResponse>.Invalid(NvwErrorCodes.IntervalItemsRequired, NvwMessages.IntervalItemsRequired);

            var seenLevels = new HashSet<EwsRiskLevel>();
            foreach (var item in request.Items)
            {
                if (item == null || !IsRatifiableLevel(item.EwsRiskLevel))
                    return NvwResult<NvwVitalSignIntervalSetResponse>.Invalid(NvwErrorCodes.EwsLevelUnknown, NvwMessages.EwsLevelUnknown);

                if (!seenLevels.Add(item.EwsRiskLevel))
                    return NvwResult<NvwVitalSignIntervalSetResponse>.Invalid(NvwErrorCodes.EwsLevelDuplicate, NvwMessages.EwsLevelDuplicate);

                if (item.IntervalMinutes < MinIntervalMinutes || item.IntervalMinutes > MaxIntervalMinutes)
                    return NvwResult<NvwVitalSignIntervalSetResponse>.Invalid(NvwErrorCodes.IntervalOutOfRange, NvwMessages.IntervalOutOfRange);
            }

            var reference = request.RatificationReference?.Trim();
            if (string.IsNullOrEmpty(reference) || reference.Length > MaxRatificationReferenceLength)
                return NvwResult<NvwVitalSignIntervalSetResponse>.Invalid(NvwErrorCodes.RatificationReference, NvwMessages.RatificationReference);

            if (!request.RatifiedOn.HasValue)
                return NvwResult<NvwVitalSignIntervalSetResponse>.Invalid(NvwErrorCodes.RatifiedOn, NvwMessages.RatifiedOnRequired);

            if (request.RatifiedOn.Value > DateOnly.FromDateTime(AppDateTimeHelper.OperationalDate()))
                return NvwResult<NvwVitalSignIntervalSetResponse>.Invalid(NvwErrorCodes.RatifiedOn, NvwMessages.RatifiedOnFuture);

            var now = DateTime.UtcNow;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            await AcquireReplaceLockAsync(cancellationToken);

            var activeRows = await _dbContext.NvwVitalSignIntervals
                .Where(x => !x.IsDelete)
                .ToListAsync(cancellationToken);

            foreach (var row in activeRows)
            {
                row.IsDelete = true;
                row.DeleteDateTime = now;
                row.DeleteBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _dbContext.NvwVitalSignIntervals.AddRange(request.Items.Select(item => new NvwVitalSignInterval
            {
                EwsRiskLevel = item.EwsRiskLevel,
                IntervalMinutes = item.IntervalMinutes,
                RatificationReference = reference,
                RatifiedOn = request.RatifiedOn.Value,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            }));

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return NvwResult<NvwVitalSignIntervalSetResponse>.Ok(await GetActiveAsync(cancellationToken));
        }

        private async Task AcquireReplaceLockAsync(CancellationToken cancellationToken)
        {
            if (!_dbContext.Database.IsNpgsql())
                return;

            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [ReplaceLockKey],
                cancellationToken);
        }

        private static bool IsRatifiableLevel(EwsRiskLevel level) =>
            level is EwsRiskLevel.Low or EwsRiskLevel.Medium or EwsRiskLevel.High or EwsRiskLevel.Critical;
    }
}
