using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimeRateResolverService
    {
        private readonly ApplicationDbContext _dbContext;

        public OvertimeRateResolverService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OvertimeRateResolutionResponse> ResolveAsync(
            OvertimeRateResolveRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.OvertimePolicyId == Guid.Empty)
            {
                return Fail("Overtime policy wajib dipilih.");
            }

            var dayType = NormalizeToken(
                request.DayType,
                OvertimeValueConstants.DayType.All);

            if (dayType == null)
            {
                return Fail("Day type tidak valid.");
            }

            var preferredTimeBand = string.IsNullOrWhiteSpace(request.PreferredTimeBand)
                ? null
                : NormalizeToken(
                    request.PreferredTimeBand,
                    OvertimeValueConstants.TimeBand.All);

            if (!string.IsNullOrWhiteSpace(request.PreferredTimeBand) &&
                preferredTimeBand == null)
            {
                return Fail("Preferred time band tidak valid.");
            }

            var effectiveDate = (request.EffectiveDate ?? DateTime.UtcNow).Date;

            var policyExists = await _dbContext.MstOvertimePolicies
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == request.OvertimePolicyId &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= effectiveDate) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= effectiveDate),
                    cancellationToken);

            if (!policyExists)
            {
                return Fail("Overtime policy tidak ditemukan, tidak aktif, atau di luar periode efektif.");
            }

            var entities = await _dbContext.MstOvertimeRates
                .AsNoTracking()
                .Where(x =>
                    x.OvertimePolicyId == request.OvertimePolicyId &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.DayType == dayType &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= effectiveDate) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= effectiveDate) &&
                    x.MinimumEligibleMinutes <= request.EligibleMinutes &&
                    (!x.MaximumEligibleMinutes.HasValue || x.MaximumEligibleMinutes.Value >= request.EligibleMinutes))
                .ToListAsync(cancellationToken);

            var applicable = entities
                .Where(x => preferredTimeBand == null ||
                    string.Equals(x.TimeBand, preferredTimeBand, StringComparison.OrdinalIgnoreCase))
                .Where(x => IsApplicable(
                    x,
                    request.MinutePosition,
                    request.OccurrenceTime))
                .Select(MapCandidate)
                .OrderByDescending(x => x.Priority)
                .ThenByDescending(x => x.ApplicabilityScore)
                .ThenByDescending(x => x.EffectiveStartDate ?? DateTime.MinValue)
                .ThenBy(x => x.OvertimeRateCode)
                .ToList();

            if (applicable.Count == 0)
            {
                return Fail("Tidak ada overtime rate aktif yang sesuai dengan policy, day type, time band, dan posisi menit lembur.");
            }

            var selected = applicable[0];
            var isAmbiguous = applicable.Skip(1).Any(x => HasSameRank(selected, x));

            return new OvertimeRateResolutionResponse
            {
                IsResolved = true,
                IsAmbiguous = isAmbiguous,
                Message = isAmbiguous
                    ? "Overtime rate berhasil dipilih secara deterministik, tetapi terdapat rate lain dengan ranking yang sama. Perbaiki priority atau interval rate sebelum digunakan untuk transaksi."
                    : "Overtime rate berhasil di-resolve.",
                SelectedRate = selected,
                Candidates = applicable.Take(20).ToList()
            };
        }

        public async Task<OvertimeRateOverlapResponse> CheckAmbiguousOverlapAsync(
            Guid? excludeId,
            OvertimeRateDefinitionInput input,
            CancellationToken cancellationToken = default)
        {
            if (!input.IsActive)
            {
                return new OvertimeRateOverlapResponse();
            }

            var query = _dbContext.MstOvertimeRates
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.OvertimePolicyId == input.OvertimePolicyId &&
                    x.DayType == input.DayType &&
                    x.Priority == input.Priority);

            if (excludeId.HasValue && excludeId.Value != Guid.Empty)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            var candidates = await query.ToListAsync(cancellationToken);
            var conflict = candidates.FirstOrDefault(x =>
                DateRangesOverlap(
                    x.EffectiveStartDate,
                    x.EffectiveEndDate,
                    input.EffectiveStartDate,
                    input.EffectiveEndDate) &&
                ApplicabilityRangesOverlap(x, input));

            return conflict == null
                ? new OvertimeRateOverlapResponse()
                : new OvertimeRateOverlapResponse
                {
                    HasAmbiguousOverlap = true,
                    ConflictingRateId = conflict.Id,
                    ConflictingRateCode = conflict.OvertimeRateCode,
                    ConflictingRateName = conflict.OvertimeRateName
                };
        }

        private static bool IsApplicable(
            MstOvertimeRate rate,
            int minutePosition,
            TimeOnly? occurrenceTime)
        {
            if (string.Equals(
                rate.TimeBand,
                OvertimeValueConstants.TimeBand.AllDay,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (OvertimeValueConstants.TimeBand.UsesMinuteRange(rate.TimeBand))
            {
                return minutePosition >= rate.StartMinute &&
                    (!rate.EndMinute.HasValue || minutePosition < rate.EndMinute.Value);
            }

            if (OvertimeValueConstants.TimeBand.UsesClockRange(rate.TimeBand))
            {
                if (!occurrenceTime.HasValue ||
                    !rate.StartTime.HasValue ||
                    !rate.EndTime.HasValue)
                {
                    return false;
                }

                return IsTimeWithinRange(
                    occurrenceTime.Value,
                    rate.StartTime.Value,
                    rate.EndTime.Value);
            }

            return false;
        }

        private static bool ApplicabilityRangesOverlap(
            MstOvertimeRate existing,
            OvertimeRateDefinitionInput candidate)
        {
            var existingScore = GetApplicabilityScore(existing.TimeBand);
            var candidateScore = GetApplicabilityScore(candidate.TimeBand);

            // Perbedaan specificity tetap deterministik pada resolver.
            if (existingScore != candidateScore)
            {
                return false;
            }

            if (string.Equals(
                    existing.TimeBand,
                    OvertimeValueConstants.TimeBand.AllDay,
                    StringComparison.OrdinalIgnoreCase) &&
                string.Equals(
                    candidate.TimeBand,
                    OvertimeValueConstants.TimeBand.AllDay,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (OvertimeValueConstants.TimeBand.UsesMinuteRange(existing.TimeBand) &&
                OvertimeValueConstants.TimeBand.UsesMinuteRange(candidate.TimeBand))
            {
                var existingEnd = existing.EndMinute ?? int.MaxValue;
                var candidateEnd = candidate.EndMinute ?? int.MaxValue;
                return existing.StartMinute < candidateEnd && candidate.StartMinute < existingEnd;
            }

            if (OvertimeValueConstants.TimeBand.UsesClockRange(existing.TimeBand) &&
                OvertimeValueConstants.TimeBand.UsesClockRange(candidate.TimeBand) &&
                existing.StartTime.HasValue &&
                existing.EndTime.HasValue &&
                candidate.StartTime.HasValue &&
                candidate.EndTime.HasValue)
            {
                var existingIntervals = ExpandClockRange(
                    existing.StartTime.Value,
                    existing.EndTime.Value);
                var candidateIntervals = ExpandClockRange(
                    candidate.StartTime.Value,
                    candidate.EndTime.Value);

                return existingIntervals.Any(left =>
                    candidateIntervals.Any(right =>
                        left.Start < right.End && right.Start < left.End));
            }

            return false;
        }

        private static List<MinuteInterval> ExpandClockRange(
            TimeOnly start,
            TimeOnly end)
        {
            var startMinute = start.Hour * 60 + start.Minute;
            var endMinute = end.Hour * 60 + end.Minute;

            if (startMinute == endMinute)
            {
                return new List<MinuteInterval>
                {
                    new(0, 1440)
                };
            }

            if (startMinute < endMinute)
            {
                return new List<MinuteInterval>
                {
                    new(startMinute, endMinute)
                };
            }

            return new List<MinuteInterval>
            {
                new(startMinute, 1440),
                new(0, endMinute)
            };
        }

        private static bool IsTimeWithinRange(
            TimeOnly value,
            TimeOnly start,
            TimeOnly end)
        {
            if (start == end)
            {
                return true;
            }

            if (start < end)
            {
                return value >= start && value < end;
            }

            // Mendukung interval overnight, contoh 22:00 sampai 06:00.
            return value >= start || value < end;
        }

        private static OvertimeRateResolutionCandidateResponse MapCandidate(
            MstOvertimeRate entity) => new()
        {
            Id = entity.Id,
            OvertimeRateCode = entity.OvertimeRateCode,
            OvertimeRateName = entity.OvertimeRateName,
            DayType = entity.DayType,
            TimeBand = entity.TimeBand,
            CalculationMethod = entity.CalculationMethod,
            RateMultiplier = entity.RateMultiplier,
            FixedAmount = entity.FixedAmount,
            Priority = entity.Priority,
            ApplicabilityScore = GetApplicabilityScore(entity.TimeBand),
            EffectiveStartDate = entity.EffectiveStartDate,
            EffectiveEndDate = entity.EffectiveEndDate
        };

        private static int GetApplicabilityScore(string timeBand)
        {
            if (string.Equals(
                timeBand,
                OvertimeValueConstants.TimeBand.Custom,
                StringComparison.OrdinalIgnoreCase)) return 50;

            if (string.Equals(
                timeBand,
                OvertimeValueConstants.TimeBand.Night,
                StringComparison.OrdinalIgnoreCase)) return 40;

            if (OvertimeValueConstants.TimeBand.UsesMinuteRange(timeBand)) return 30;

            if (string.Equals(
                timeBand,
                OvertimeValueConstants.TimeBand.AllDay,
                StringComparison.OrdinalIgnoreCase)) return 10;

            return 0;
        }

        private static bool HasSameRank(
            OvertimeRateResolutionCandidateResponse left,
            OvertimeRateResolutionCandidateResponse right) =>
            left.Priority == right.Priority &&
            left.ApplicabilityScore == right.ApplicabilityScore &&
            (left.EffectiveStartDate ?? DateTime.MinValue).Date ==
            (right.EffectiveStartDate ?? DateTime.MinValue).Date;

        private static bool DateRangesOverlap(
            DateTime? leftStart,
            DateTime? leftEnd,
            DateTime? rightStart,
            DateTime? rightEnd)
        {
            var leftStartValue = leftStart?.Date ?? DateTime.MinValue.Date;
            var leftEndValue = leftEnd?.Date ?? DateTime.MaxValue.Date;
            var rightStartValue = rightStart?.Date ?? DateTime.MinValue.Date;
            var rightEndValue = rightEnd?.Date ?? DateTime.MaxValue.Date;

            return leftStartValue <= rightEndValue && rightStartValue <= leftEndValue;
        }

        private static string? NormalizeToken(
            string? value,
            IReadOnlyCollection<string> allowed)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return allowed.FirstOrDefault(x =>
                x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static OvertimeRateResolutionResponse Fail(string message) => new()
        {
            IsResolved = false,
            IsAmbiguous = false,
            Message = message
        };

        private sealed record MinuteInterval(int Start, int End);
    }
}
