using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimeCompensatoryExpiryService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<OvertimeCompensatoryExpiryService> _logger;

        public OvertimeCompensatoryExpiryService(
            ApplicationDbContext dbContext,
            ILogger<OvertimeCompensatoryExpiryService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<OvertimeCompensatoryExpiryResponse> ExpireDueCreditsAsync(
            DateOnly asOfDate,
            Guid actorUserId,
            int maximumItems = 500,
            CancellationToken cancellationToken = default)
        {
            var response = new OvertimeCompensatoryExpiryResponse { AsOfDate = asOfDate };
            if (actorUserId == Guid.Empty)
            {
                response.Messages.Add("System actor user id belum dikonfigurasi; expiry tidak dijalankan.");
                return response;
            }

            var candidateIds = await _dbContext.TrxCompensatoryTimeOffs
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.ExpiryDate.HasValue &&
                    x.ExpiryDate.Value < asOfDate &&
                    x.RemainingMinutes > 0 &&
                    (x.CompensatoryStatus == OvertimeValueConstants.CompensatoryStatus.Available ||
                     x.CompensatoryStatus == OvertimeValueConstants.CompensatoryStatus.PartiallyUsed))
                .OrderBy(x => x.ExpiryDate)
                .ThenBy(x => x.CreateDateTime)
                .Select(x => x.Id)
                .Take(Math.Clamp(maximumItems, 1, 5000))
                .ToListAsync(cancellationToken);

            response.CandidateCount = candidateIds.Count;
            foreach (var id in candidateIds)
            {
                try
                {
                    var result = await ExpireOneAsync(id, asOfDate, actorUserId, cancellationToken);
                    if (result.Expired)
                    {
                        response.ExpiredCount++;
                        response.ExpiredMinutes += result.ExpiredMinutes;
                        response.ExpiredDays += result.ExpiredDays;
                    }
                    else
                    {
                        response.SkippedCount++;
                        if (!string.IsNullOrWhiteSpace(result.Message)) response.Messages.Add(result.Message);
                    }
                }
                catch (Exception ex)
                {
                    response.FailedCount++;
                    response.Messages.Add($"Expiry credit {id} gagal: {ex.Message}");
                    _logger.LogError(ex, "Compensatory expiry gagal untuk credit {CreditId}.", id);
                }
            }

            response.ExpiredDays = Math.Round(response.ExpiredDays, 4, MidpointRounding.AwayFromZero);
            return response;
        }

        private async Task<ExpiryOneResult> ExpireOneAsync(
            Guid creditId,
            DateOnly asOfDate,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var lockKey = "OTC-EXPIRY-" + creditId;
                await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT pg_advisory_xact_lock(hashtext({lockKey}))",
                    cancellationToken);

                var credit = await _dbContext.TrxCompensatoryTimeOffs
                    .FirstOrDefaultAsync(x => x.Id == creditId && !x.IsDelete && !x.IsCancel, cancellationToken);
                if (credit == null || !credit.IsActive || !credit.ExpiryDate.HasValue || credit.ExpiryDate.Value >= asOfDate || credit.RemainingMinutes <= 0)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return ExpiryOneResult.Skip($"Credit {creditId} sudah tidak memenuhi syarat expiry.");
                }

                if (credit.ReservedMinutes > 0)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return ExpiryOneResult.Skip($"Credit {credit.CreditNumber} masih mempunyai reserved minutes dan belum dapat dieksekusi expiry.");
                }

                var sourceLedger = credit.LeaveBalanceTransactionId.HasValue
                    ? await _dbContext.TrxLeaveBalanceTransactions
                        .FirstOrDefaultAsync(x => x.Id == credit.LeaveBalanceTransactionId.Value && !x.IsDelete, cancellationToken)
                    : null;
                sourceLedger ??= await _dbContext.TrxLeaveBalanceTransactions
                    .OrderByDescending(x => x.TransactionDateTime)
                    .FirstOrDefaultAsync(x =>
                        !x.IsDelete &&
                        x.SourceReferenceId == credit.Id &&
                        x.SourceType == OvertimeValueConstants.CompensatoryLedger.SourceTypeCredit,
                        cancellationToken);

                if (sourceLedger == null || credit.EarnedMinutes <= 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return ExpiryOneResult.Skip($"Credit {credit.CreditNumber} tidak mempunyai source ledger yang valid.");
                }

                var balance = await _dbContext.WfpLeaveBalances
                    .FromSqlInterpolated($@"SELECT * FROM public.""WfpLeaveBalance"" WHERE ""Id"" = {sourceLedger.LeaveBalanceId} AND ""IsDelete"" = false FOR UPDATE")
                    .FirstOrDefaultAsync(cancellationToken);
                if (balance == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return ExpiryOneResult.Skip($"Leave balance untuk credit {credit.CreditNumber} tidak ditemukan.");
                }

                var daysPerMinute = Math.Abs(sourceLedger.TransactionDays) / credit.EarnedMinutes;
                var expiryDays = Math.Round(daysPerMinute * credit.RemainingMinutes, 4, MidpointRounding.AwayFromZero);
                if (expiryDays <= 0 || balance.AvailableDays < expiryDays)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return ExpiryOneResult.Skip($"Saldo available tidak cukup untuk expiry credit {credit.CreditNumber}.");
                }

                var now = DateTime.UtcNow;
                var ledger = new TrxLeaveBalanceTransaction
                {
                    Id = Guid.NewGuid(),
                    TransactionNumber = GenerateLedgerNumber(),
                    LeaveBalanceId = balance.Id,
                    WorkforceProfileId = balance.WorkforceProfileId,
                    LeaveTypeId = balance.LeaveTypeId,
                    LeaveEntitlementPeriodId = balance.LeaveEntitlementPeriodId,
                    TransactionDateTime = now,
                    EffectiveDate = asOfDate,
                    TransactionSequence = balance.LastTransactionSequence + 1,
                    TransactionType = OvertimeValueConstants.CompensatoryLedger.TransactionTypeExpiry,
                    Direction = OvertimeValueConstants.CompensatoryLedger.DirectionDebit,
                    TransactionDays = -expiryDays,
                    AvailableDelta = -expiryDays,
                    PreviousOpeningBalanceDays = balance.OpeningBalanceDays,
                    PreviousAvailableDays = balance.AvailableDays,
                    PreviousReservedDays = balance.ReservedDays,
                    NewAvailableDays = balance.AvailableDays - expiryDays,
                    NewReservedDays = balance.ReservedDays,
                    NewUsedDays = balance.UsedDays,
                    IdempotencyKey = "OTC-EXPIRY-" + credit.Id,
                    PostingBatchType = OvertimeValueConstants.CompensatoryLedger.PostingBatchType,
                    PostingBatchId = credit.Id,
                    SourceType = OvertimeValueConstants.CompensatoryLedger.SourceTypeExpiry,
                    SourceReferenceId = credit.Id,
                    SourceReferenceNumber = credit.CreditNumber,
                    TransactionStatus = OvertimeValueConstants.CompensatoryLedger.Posted,
                    PostedAt = now,
                    PostedByUserId = actorUserId,
                    Remarks = "Automatic expiry compensatory leave dari Overtime Management.",
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                balance.ExpiredDays += expiryDays;
                balance.RemainingDays = CalculateRemaining(balance);
                balance.AvailableDays = balance.RemainingDays - balance.ReservedDays;
                balance.LastTransactionId = ledger.Id;
                balance.LastTransactionSequence = ledger.TransactionSequence;
                balance.BalanceVersion += 1;
                balance.LastCalculatedAt = now;
                balance.UpdateDateTime = now;
                balance.UpdateBy = actorUserId;
                ledger.NewAvailableDays = balance.AvailableDays;
                ledger.NewReservedDays = balance.ReservedDays;
                ledger.NewUsedDays = balance.UsedDays;

                var expiredMinutes = credit.RemainingMinutes;
                credit.ExpiredMinutes += expiredMinutes;
                credit.RemainingMinutes = 0;
                credit.CompensatoryStatus = OvertimeValueConstants.CompensatoryStatus.Expired;
                credit.ExpiredAt = now;
                credit.IsActive = false;
                credit.UpdateDateTime = now;
                credit.UpdateBy = actorUserId;

                _dbContext.TrxLeaveBalanceTransactions.Add(ledger);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return ExpiryOneResult.Ok(expiredMinutes, expiryDays);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private static decimal CalculateRemaining(WfpLeaveBalance balance) =>
            balance.OpeningBalanceDays +
            balance.EntitlementDays +
            balance.AccruedDays +
            balance.CarriedForwardDays +
            balance.AdjustmentDays +
            balance.CompensatoryDays +
            balance.RecalledDays -
            balance.UsedDays -
            balance.ExpiredDays -
            balance.EncashmentDays;

        private static string GenerateLedgerNumber() =>
            $"LBT-OTC-EX-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..48].ToUpperInvariant();

        private sealed class ExpiryOneResult
        {
            public bool Expired { get; private set; }
            public int ExpiredMinutes { get; private set; }
            public decimal ExpiredDays { get; private set; }
            public string? Message { get; private set; }

            public static ExpiryOneResult Ok(int minutes, decimal days) => new()
            {
                Expired = true,
                ExpiredMinutes = minutes,
                ExpiredDays = days
            };

            public static ExpiryOneResult Skip(string message) => new() { Message = message };
        }
    }
}
