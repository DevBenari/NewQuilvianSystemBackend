using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    internal sealed class LeaveCarryForwardCandidateWorkItem
    {
        public LeaveCarryForwardCandidateResponse Response { get; set; } = new();
        public WfpLeaveBalance SourceBalance { get; set; } = null!;
        public MstLeaveCarryForwardPolicy Policy { get; set; } = null!;
        public TrxLeaveEntitlementPeriod SourcePeriod { get; set; } = null!;
        public TrxLeaveEntitlementPeriod DestinationPeriod { get; set; } = null!;
    }

    public class LeaveCarryForwardPolicyResolverService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly ApplicationDbContext _dbContext;

        public LeaveCarryForwardPolicyResolverService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<MstLeaveCarryForwardPolicy?> ResolvePolicyAsync(
            WfpLeaveBalance balance,
            Guid? explicitPolicyId,
            DateOnly executionDate,
            CancellationToken cancellationToken = default)
        {
            var date = executionDate.ToDateTime(TimeOnly.MinValue);

            if (explicitPolicyId.HasValue)
            {
                return await _dbContext.Set<MstLeaveCarryForwardPolicy>()
                    .AsNoTracking()
                    .Include(x => x.DestinationLeaveType)
                    .FirstOrDefaultAsync(x =>
                        x.Id == explicitPolicyId.Value &&
                        x.IsActive &&
                        x.IsCarryForwardEnabled &&
                        !x.IsDelete &&
                        (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= date.Date) &&
                        (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= date.Date),
                        cancellationToken);
            }

            if (!balance.LeaveEntitlementPolicyId.HasValue)
            {
                return null;
            }

            return await _dbContext.Set<MstLeaveCarryForwardPolicy>()
                .AsNoTracking()
                .Include(x => x.DestinationLeaveType)
                .Where(x =>
                    x.LeaveEntitlementPolicyId == balance.LeaveEntitlementPolicyId.Value &&
                    x.IsActive &&
                    x.IsCarryForwardEnabled &&
                    !x.IsDelete &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= date.Date) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= date.Date))
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);
        }

        internal async Task<LeaveCarryForwardCandidateWorkItem> CalculateAsync(
            WfpLeaveBalance balance,
            TrxLeaveEntitlementPeriod sourcePeriod,
            TrxLeaveEntitlementPeriod destinationPeriod,
            Guid? explicitPolicyId,
            DateOnly executionDate,
            bool forceReprocess,
            CancellationToken cancellationToken = default)
        {
            var response = new LeaveCarryForwardCandidateResponse
            {
                WorkforceProfileId = balance.WorkforceProfileId,
                WorkforceProfileCode = balance.WorkforceProfile?.ProfileCode,
                WorkforceDisplayName = balance.WorkforceProfile?.DisplayName,
                SourceLeaveBalanceId = balance.Id,
                SourceLeaveTypeId = balance.LeaveTypeId,
                SourceLeaveTypeCode = balance.LeaveType?.LeaveTypeCode,
                SourceLeaveTypeName = balance.LeaveType?.LeaveTypeName,
                SourceAvailableDays = Math.Max(0, balance.AvailableDays)
            };

            var policy = await ResolvePolicyAsync(
                balance,
                explicitPolicyId,
                executionDate,
                cancellationToken);

            if (policy == null)
            {
                response.ResultCode = LeaveValueConstants.CarryForwardSkipReason.PolicyDisabled;
                response.ResultMessage = "Carry-forward policy aktif tidak ditemukan.";
                return new LeaveCarryForwardCandidateWorkItem
                {
                    Response = response,
                    SourceBalance = balance,
                    SourcePeriod = sourcePeriod,
                    DestinationPeriod = destinationPeriod,
                    Policy = new MstLeaveCarryForwardPolicy()
                };
            }

            response.LeaveCarryForwardPolicyId = policy.Id;
            response.CarryForwardPolicyCode = policy.CarryForwardPolicyCode;
            response.DestinationLeaveTypeId = policy.DestinationLeaveTypeId ?? balance.LeaveTypeId;
            response.DestinationLeaveTypeCode = policy.DestinationLeaveType?.LeaveTypeCode ?? balance.LeaveType?.LeaveTypeCode;
            response.DestinationLeaveTypeName = policy.DestinationLeaveType?.LeaveTypeName ?? balance.LeaveType?.LeaveTypeName;

            if (balance.IsLocked ||
                string.Equals(balance.BalanceStatus, LeaveValueConstants.BalanceStatus.Locked, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(balance.BalanceStatus, LeaveValueConstants.BalanceStatus.Closed, StringComparison.OrdinalIgnoreCase))
            {
                response.ResultCode = LeaveValueConstants.CarryForwardSkipReason.BalanceLocked;
                response.ResultMessage = "Source leave balance sedang dikunci atau sudah ditutup.";
                return WorkItem(response, balance, policy, sourcePeriod, destinationPeriod);
            }

            if (destinationPeriod.IsLocked ||
                string.Equals(destinationPeriod.PeriodStatus, LeaveValueConstants.PeriodStatus.Closed, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(destinationPeriod.PeriodStatus, LeaveValueConstants.PeriodStatus.Cancelled, StringComparison.OrdinalIgnoreCase))
            {
                response.ResultCode = LeaveValueConstants.CarryForwardSkipReason.MissingDestinationPeriod;
                response.ResultMessage = "Destination entitlement period sedang dikunci, ditutup, atau dibatalkan.";
                return WorkItem(response, balance, policy, sourcePeriod, destinationPeriod);
            }

            if (response.SourceAvailableDays <= 0)
            {
                response.ResultCode = LeaveValueConstants.CarryForwardSkipReason.NoAvailableBalance;
                response.ResultMessage = "Tidak ada saldo tersedia untuk dipindahkan.";
                return WorkItem(response, balance, policy, sourcePeriod, destinationPeriod);
            }

            if (policy.MinimumCarryForwardDays.HasValue &&
                response.SourceAvailableDays < policy.MinimumCarryForwardDays.Value)
            {
                response.ResultCode = LeaveValueConstants.CarryForwardSkipReason.BelowMinimumBalance;
                response.ResultMessage = "Saldo tersedia lebih kecil dari minimum carry forward.";
                return WorkItem(response, balance, policy, sourcePeriod, destinationPeriod);
            }

            if (!forceReprocess)
            {
                var alreadyPosted = await _dbContext.Set<TrxLeaveCarryForward>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.WorkforceProfileId == balance.WorkforceProfileId &&
                        x.SourceLeaveTypeId == balance.LeaveTypeId &&
                        x.SourceLeaveEntitlementPeriodId == sourcePeriod.Id &&
                        x.DestinationLeaveEntitlementPeriodId == destinationPeriod.Id &&
                        x.CarryForwardStatus == LeaveValueConstants.CarryForwardStatus.Posted &&
                        !x.IsDelete,
                        cancellationToken);

                if (alreadyPosted)
                {
                    response.ResultCode = LeaveValueConstants.CarryForwardSkipReason.AlreadyProcessed;
                    response.ResultMessage = "Carry forward untuk workforce dan periode tersebut sudah pernah diposting.";
                    return WorkItem(response, balance, policy, sourcePeriod, destinationPeriod);
                }
            }

            if (policy.MaximumCarryForwardPeriods.HasValue && policy.MaximumCarryForwardPeriods.Value > 0)
            {
                var previousIncomingCount = await _dbContext.Set<TrxLeaveCarryForward>()
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == balance.WorkforceProfileId &&
                        x.DestinationLeaveTypeId == balance.LeaveTypeId &&
                        x.DestinationLeaveEntitlementPeriodId == sourcePeriod.Id &&
                        x.CarryForwardStatus == LeaveValueConstants.CarryForwardStatus.Posted &&
                        !x.IsDelete,
                        cancellationToken);

                if (previousIncomingCount >= policy.MaximumCarryForwardPeriods.Value)
                {
                    response.ResultCode = LeaveValueConstants.CarryForwardSkipReason.MaximumPeriodReached;
                    response.ResultMessage = "Maximum carry-forward period sudah tercapai.";
                    return WorkItem(response, balance, policy, sourcePeriod, destinationPeriod);
                }
            }

            var percentageAmount = response.SourceAvailableDays * policy.CarryForwardPercentage / 100m;
            var capped = policy.MaximumCarryForwardDays.HasValue
                ? Math.Min(percentageAmount, policy.MaximumCarryForwardDays.Value)
                : percentageAmount;
            var rounded = ApplyRounding(capped, policy.RoundingMethod);
            var carryDays = Math.Max(0, Math.Min(response.SourceAvailableDays, rounded));
            var excess = Math.Max(0, response.SourceAvailableDays - carryDays);
            var payout = 0m;
            var expired = 0m;

            var action = NormalizeExcessAction(policy.ExcessBalanceAction);
            if (string.Equals(action, LeaveValueConstants.ExcessBalanceAction.Payout, StringComparison.OrdinalIgnoreCase))
            {
                if (!policy.IsPayoutAllowed)
                {
                    expired = excess;
                }
                else
                {
                    payout = policy.PayoutMaximumDays.HasValue
                        ? Math.Min(excess, policy.PayoutMaximumDays.Value)
                        : excess;
                    expired = Math.Max(0, excess - payout);
                }
            }
            else if (string.Equals(action, LeaveValueConstants.ExcessBalanceAction.KeepInSource, StringComparison.OrdinalIgnoreCase))
            {
                expired = 0;
                payout = 0;
            }
            else if (string.Equals(action, "ManualReview", StringComparison.OrdinalIgnoreCase))
            {
                response.ResultCode = LeaveValueConstants.CarryForwardSkipReason.NotEligible;
                response.ResultMessage = "Excess balance membutuhkan manual review.";
                response.ExcessDays = excess;
                return WorkItem(response, balance, policy, sourcePeriod, destinationPeriod);
            }
            else
            {
                expired = excess;
            }

            response.EligibleDays = carryDays;
            response.CarryForwardDays = carryDays;
            response.ExcessDays = excess;
            response.ExpiredDays = expired;
            response.PayoutDays = payout;
            response.RoundingAdjustmentDays = carryDays - capped;
            response.CarryForwardExpiryDate = CalculateExpiryDate(policy, executionDate, destinationPeriod);
            response.IsEligible = carryDays > 0 || expired > 0 || payout > 0;
            response.ResultCode = response.IsEligible ? "Eligible" : LeaveValueConstants.CarryForwardSkipReason.NoAvailableBalance;
            response.ResultMessage = response.IsEligible
                ? "Saldo memenuhi aturan carry forward."
                : "Tidak ada nilai yang perlu diproses.";
            response.CalculationDetailJson = JsonSerializer.Serialize(new
            {
                sourceAvailableDays = response.SourceAvailableDays,
                minimumCarryForwardDays = policy.MinimumCarryForwardDays,
                maximumCarryForwardDays = policy.MaximumCarryForwardDays,
                carryForwardPercentage = policy.CarryForwardPercentage,
                rawCarryForwardDays = percentageAmount,
                cappedCarryForwardDays = capped,
                roundedCarryForwardDays = carryDays,
                roundingMethod = policy.RoundingMethod,
                excessBalanceAction = policy.ExcessBalanceAction,
                expiryMethod = policy.ExpiryMethod,
                carryForwardExpiryDate = response.CarryForwardExpiryDate
            }, JsonOptions);

            return WorkItem(response, balance, policy, sourcePeriod, destinationPeriod);
        }

        private static LeaveCarryForwardCandidateWorkItem WorkItem(
            LeaveCarryForwardCandidateResponse response,
            WfpLeaveBalance balance,
            MstLeaveCarryForwardPolicy policy,
            TrxLeaveEntitlementPeriod sourcePeriod,
            TrxLeaveEntitlementPeriod destinationPeriod)
        {
            return new LeaveCarryForwardCandidateWorkItem
            {
                Response = response,
                SourceBalance = balance,
                Policy = policy,
                SourcePeriod = sourcePeriod,
                DestinationPeriod = destinationPeriod
            };
        }

        private static decimal ApplyRounding(decimal value, string? method)
        {
            if (string.Equals(method, LeaveValueConstants.RoundingMethod.Up, StringComparison.OrdinalIgnoreCase))
            {
                return Math.Ceiling(value);
            }

            if (string.Equals(method, LeaveValueConstants.RoundingMethod.Down, StringComparison.OrdinalIgnoreCase))
            {
                return Math.Floor(value);
            }

            if (string.Equals(method, LeaveValueConstants.RoundingMethod.NearestHalfDay, StringComparison.OrdinalIgnoreCase))
            {
                return Math.Round(value * 2m, MidpointRounding.AwayFromZero) / 2m;
            }

            if (string.Equals(method, LeaveValueConstants.RoundingMethod.NearestDay, StringComparison.OrdinalIgnoreCase))
            {
                return Math.Round(value, MidpointRounding.AwayFromZero);
            }

            return value;
        }

        private static DateOnly? CalculateExpiryDate(
            MstLeaveCarryForwardPolicy policy,
            DateOnly executionDate,
            TrxLeaveEntitlementPeriod destinationPeriod)
        {
            if (string.Equals(policy.ExpiryMethod, "NoExpiry", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(policy.ExpiryMethod, LeaveValueConstants.ExpiryMethod.Never, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (string.Equals(policy.ExpiryMethod, LeaveValueConstants.ExpiryMethod.EndOfDestinationPeriod, StringComparison.OrdinalIgnoreCase))
            {
                return destinationPeriod.EndDate;
            }

            if (string.Equals(policy.ExpiryMethod, LeaveValueConstants.ExpiryMethod.FixedDate, StringComparison.OrdinalIgnoreCase) &&
                policy.ExpiryMonth.HasValue &&
                policy.ExpiryDay.HasValue)
            {
                var year = destinationPeriod.PeriodYear > 0
                    ? destinationPeriod.PeriodYear
                    : destinationPeriod.StartDate.Year;
                var day = Math.Min(policy.ExpiryDay.Value, DateTime.DaysInMonth(year, policy.ExpiryMonth.Value));
                var date = new DateOnly(year, policy.ExpiryMonth.Value, day);
                if (date < destinationPeriod.StartDate)
                {
                    date = date.AddYears(1);
                }
                return date;
            }

            var months = policy.ExpiryMonths.GetValueOrDefault(0);
            return months > 0 ? executionDate.AddMonths(months) : destinationPeriod.EndDate;
        }

        private static string NormalizeExcessAction(string? value)
        {
            if (string.Equals(value, "KeepWithoutExpiry", StringComparison.OrdinalIgnoreCase))
            {
                return LeaveValueConstants.ExcessBalanceAction.KeepInSource;
            }

            return string.IsNullOrWhiteSpace(value)
                ? LeaveValueConstants.ExcessBalanceAction.Forfeit
                : value.Trim();
        }
    }
}
