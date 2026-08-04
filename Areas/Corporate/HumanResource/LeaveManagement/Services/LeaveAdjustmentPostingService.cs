using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Repositories;
using System.Data;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    /// <summary>
    /// Satu-satunya pintu posting Leave Adjustment ke immutable balance ledger.
    /// Service ini tidak menangani approval. Ia hanya menerima adjustment berstatus
    /// Approved dan mem-posting perubahan saldo secara atomic dan idempotent.
    /// </summary>
    public class LeaveAdjustmentPostingService
    {
        private readonly ApplicationDbContext _dbContext;

        public LeaveAdjustmentPostingService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<LeaveAdjustmentServiceResult<LeaveAdjustmentPostingResponse>> PostAsync(
            Guid leaveAdjustmentId,
            Guid actorUserId,
            string? note = null,
            string? idempotencyKey = null,
            CancellationToken cancellationToken = default)
        {
            if (leaveAdjustmentId == Guid.Empty || actorUserId == Guid.Empty)
            {
                return LeaveAdjustmentServiceResult<LeaveAdjustmentPostingResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Leave adjustment id atau actor user id tidak valid.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var adjustment = await _dbContext.Set<TrxLeaveAdjustment>()
                    .Include(x => x.LeaveBalance)
                        .ThenInclude(x => x!.LeavePolicy)
                    .Include(x => x.LeaveEntitlementPeriod)
                    .Include(x => x.OriginalAdjustment)
                    .FirstOrDefaultAsync(
                        x => x.Id == leaveAdjustmentId && !x.IsDelete,
                        cancellationToken);

                if (adjustment == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeaveAdjustmentServiceResult<LeaveAdjustmentPostingResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Leave adjustment tidak ditemukan.");
                }

                if (adjustment.LeaveBalance == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeaveAdjustmentServiceResult<LeaveAdjustmentPostingResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Leave balance adjustment tidak ditemukan.");
                }

                var existingTransaction = await _dbContext.Set<TrxLeaveBalanceTransaction>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => !x.IsDelete &&
                             x.LeaveAdjustmentId == adjustment.Id &&
                             x.TransactionStatus == LeaveValueConstants.TransactionStatus.Posted,
                        cancellationToken);

                if (existingTransaction != null)
                {
                    await transaction.CommitAsync(cancellationToken);

                    return LeaveAdjustmentServiceResult<LeaveAdjustmentPostingResponse>.Ok(
                        BuildResponse(
                            adjustment,
                            adjustment.LeaveBalance,
                            existingTransaction,
                            isIdempotent: true),
                        "Leave adjustment sudah pernah diposting. Tidak dibuat ledger duplikat.");
                }

                if (!string.Equals(
                        adjustment.AdjustmentStatus,
                        LeaveValueConstants.AdjustmentStatus.Approved,
                        StringComparison.OrdinalIgnoreCase))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeaveAdjustmentServiceResult<LeaveAdjustmentPostingResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Leave adjustment hanya dapat diposting dari status Approved.");
                }

                var balance = adjustment.LeaveBalance;
                if (balance.IsLocked ||
                    !balance.IsActive ||
                    balance.IsDelete ||
                    string.Equals(balance.BalanceStatus, LeaveValueConstants.BalanceStatus.Locked, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(balance.BalanceStatus, LeaveValueConstants.BalanceStatus.Closed, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(balance.BalanceStatus, LeaveValueConstants.BalanceStatus.Expired, StringComparison.OrdinalIgnoreCase))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeaveAdjustmentServiceResult<LeaveAdjustmentPostingResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Leave balance sedang terkunci, tidak aktif, sudah ditutup, atau sudah kedaluwarsa.");
                }

                if (adjustment.LeaveEntitlementPeriod?.IsLocked == true ||
                    string.Equals(
                        adjustment.LeaveEntitlementPeriod?.PeriodStatus,
                        LeaveValueConstants.PeriodStatus.Closed,
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        adjustment.LeaveEntitlementPeriod?.PeriodStatus,
                        LeaveValueConstants.PeriodStatus.Cancelled,
                        StringComparison.OrdinalIgnoreCase))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeaveAdjustmentServiceResult<LeaveAdjustmentPostingResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Leave entitlement period sedang terkunci, sudah ditutup, atau dibatalkan.");
                }

                var postedDays = adjustment.ApprovedDays ?? adjustment.RequestedDays;
                if (postedDays <= 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeaveAdjustmentServiceResult<LeaveAdjustmentPostingResponse>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Jumlah hari yang akan diposting harus lebih besar dari 0.");
                }

                var now = DateTime.UtcNow;
                var previousOpeningBalance = balance.OpeningBalanceDays;
                var previousAvailable = balance.AvailableDays;
                var previousReserved = balance.ReservedDays;
                var previousUsed = balance.UsedDays;

                var deltas = await ResolveDeltasAsync(
                    adjustment,
                    postedDays,
                    cancellationToken);

                if (!deltas.Success)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeaveAdjustmentServiceResult<LeaveAdjustmentPostingResponse>.Fail(
                        deltas.StatusCode,
                        deltas.Message);
                }

                ApplyDeltas(balance, deltas);
                RecalculateBalance(balance);

                var policy = balance.LeavePolicy;
                if (!IsNegativeBalanceAllowed(balance.AvailableDays, policy, out var negativeMessage))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeaveAdjustmentServiceResult<LeaveAdjustmentPostingResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        negativeMessage!);
                }

                var nextSequence = balance.LastTransactionSequence + 1;
                var ledger = new TrxLeaveBalanceTransaction
                {
                    Id = Guid.NewGuid(),
                    TransactionNumber = GenerateTransactionNumber(),
                    LeaveBalanceId = balance.Id,
                    WorkforceProfileId = adjustment.WorkforceProfileId,
                    LeaveTypeId = adjustment.LeaveTypeId,
                    LeaveEntitlementPeriodId = adjustment.LeaveEntitlementPeriodId,
                    LeaveAdjustmentId = adjustment.Id,
                    OriginalTransactionId = deltas.OriginalTransactionId,
                    TransactionDateTime = now,
                    EffectiveDate = adjustment.EffectiveDate,
                    TransactionSequence = nextSequence,
                    TransactionType = deltas.TransactionType,
                    Direction = adjustment.Direction,
                    TransactionDays = postedDays,
                    OpeningBalanceDelta = deltas.OpeningBalanceDelta,
                    EntitlementDelta = deltas.EntitlementDelta,
                    AccruedDelta = deltas.AccruedDelta,
                    CarryForwardDelta = deltas.CarryForwardDelta,
                    AdjustmentDelta = deltas.AdjustmentDelta,
                    CompensatoryDelta = deltas.CompensatoryDelta,
                    PendingDelta = deltas.PendingDelta,
                    ReservedDelta = deltas.ReservedDelta,
                    UsedDelta = deltas.UsedDelta,
                    RecalledDelta = deltas.RecalledDelta,
                    ExpiredDelta = deltas.ExpiredDelta,
                    EncashmentDelta = deltas.EncashmentDelta,
                    AvailableDelta = deltas.AvailableDelta,
                    PreviousOpeningBalanceDays = previousOpeningBalance,
                    PreviousAvailableDays = previousAvailable,
                    PreviousReservedDays = previousReserved,
                    NewAvailableDays = balance.AvailableDays,
                    NewReservedDays = balance.ReservedDays,
                    NewUsedDays = balance.UsedDays,
                    IdempotencyKey = NormalizeText(idempotencyKey) ??
                        $"leave-adjustment:{adjustment.Id:N}:post",
                    PostingBatchType = LeaveValueConstants.PostingBatchType.Adjustment,
                    PostingBatchId = adjustment.Id,
                    SourceType = adjustment.SourceType,
                    SourceReferenceId = adjustment.SourceReferenceId ?? adjustment.Id,
                    SourceReferenceNumber = adjustment.SourceReferenceNumber ?? adjustment.AdjustmentNumber,
                    TransactionStatus = LeaveValueConstants.TransactionStatus.Posted,
                    PostedAt = now,
                    PostedByUserId = actorUserId,
                    Remarks = NormalizeText(note) ?? adjustment.Reason,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                if (deltas.OriginalTransaction != null)
                {
                    deltas.OriginalTransaction.ReversedTransactionId = ledger.Id;
                    deltas.OriginalTransaction.ReversedAt = now;
                    deltas.OriginalTransaction.ReversedByUserId = actorUserId;
                    deltas.OriginalTransaction.TransactionStatus =
                        LeaveValueConstants.TransactionStatus.Reversed;
                    deltas.OriginalTransaction.UpdateDateTime = now;
                    deltas.OriginalTransaction.UpdateBy = actorUserId;
                }

                if (adjustment.OriginalAdjustment != null)
                {
                    adjustment.OriginalAdjustment.AdjustmentStatus =
                        LeaveValueConstants.AdjustmentStatus.Reversed;
                    adjustment.OriginalAdjustment.ReversedAt = now;
                    adjustment.OriginalAdjustment.ReversedByUserId = actorUserId;
                    adjustment.OriginalAdjustment.ReversalReason = adjustment.Reason;
                    adjustment.OriginalAdjustment.UpdateDateTime = now;
                    adjustment.OriginalAdjustment.UpdateBy = actorUserId;
                }

                balance.LastTransactionId = ledger.Id;
                balance.LastTransactionSequence = nextSequence;
                balance.BalanceVersion += 1;
                balance.LastCalculatedAt = now;
                balance.UpdateDateTime = now;
                balance.UpdateBy = actorUserId;

                adjustment.PostedDays = postedDays;
                adjustment.AdjustmentStatus = LeaveValueConstants.AdjustmentStatus.Posted;
                adjustment.PostedAt = now;
                adjustment.PostedByUserId = actorUserId;
                adjustment.PostingSnapshotJson = JsonSerializer.Serialize(new
                {
                    leaveBalanceId = balance.Id,
                    ledgerTransactionId = ledger.Id,
                    ledger.TransactionNumber,
                    transactionSequence = nextSequence,
                    postedDays,
                    adjustment.Direction,
                    deltas.TransactionType,
                    previousOpeningBalance,
                    previousAvailable,
                    previousReserved,
                    previousUsed,
                    newOpeningBalanceDays = balance.OpeningBalanceDays,
                    newAdjustmentDays = balance.AdjustmentDays,
                    newRemainingDays = balance.RemainingDays,
                    newAvailableDays = balance.AvailableDays,
                    newReservedDays = balance.ReservedDays,
                    newUsedDays = balance.UsedDays,
                    balanceVersion = balance.BalanceVersion,
                    postedAt = now,
                    postedByUserId = actorUserId
                });
                adjustment.UpdateDateTime = now;
                adjustment.UpdateBy = actorUserId;

                _dbContext.Set<TrxLeaveBalanceTransaction>().Add(ledger);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return LeaveAdjustmentServiceResult<LeaveAdjustmentPostingResponse>.Ok(
                    BuildResponse(adjustment, balance, ledger, isIdempotent: false),
                    "Leave adjustment berhasil diposting ke balance ledger.");
            }
            catch (DbUpdateConcurrencyException)
            {
                await SafeRollbackAsync(transaction, cancellationToken);
                return LeaveAdjustmentServiceResult<LeaveAdjustmentPostingResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Saldo cuti berubah bersamaan dengan proses ini. Muat ulang data lalu ulangi posting.");
            }
            catch (DbUpdateException ex)
            {
                await SafeRollbackAsync(transaction, cancellationToken);
                return LeaveAdjustmentServiceResult<LeaveAdjustmentPostingResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Posting leave adjustment gagal karena konflik data: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                await SafeRollbackAsync(transaction, cancellationToken);
                return LeaveAdjustmentServiceResult<LeaveAdjustmentPostingResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Posting leave adjustment gagal: {ex.Message}");
            }
        }

        private async Task<AdjustmentDeltas> ResolveDeltasAsync(
            TrxLeaveAdjustment adjustment,
            decimal postedDays,
            CancellationToken cancellationToken)
        {
            if (string.Equals(
                    adjustment.AdjustmentType,
                    LeaveValueConstants.AdjustmentType.Reversal,
                    StringComparison.OrdinalIgnoreCase))
            {
                if (!adjustment.OriginalAdjustmentId.HasValue)
                {
                    return AdjustmentDeltas.Fail(
                        StatusCodes.Status400BadRequest,
                        "Reversal adjustment tidak mempunyai original adjustment.");
                }

                var originalTransaction = await _dbContext.Set<TrxLeaveBalanceTransaction>()
                    .FirstOrDefaultAsync(
                        x => !x.IsDelete &&
                             x.LeaveAdjustmentId == adjustment.OriginalAdjustmentId.Value &&
                             x.TransactionStatus == LeaveValueConstants.TransactionStatus.Posted,
                        cancellationToken);

                if (originalTransaction == null)
                {
                    return AdjustmentDeltas.Fail(
                        StatusCodes.Status409Conflict,
                        "Ledger posted milik original adjustment tidak ditemukan.");
                }

                return new AdjustmentDeltas
                {
                    Success = true,
                    TransactionType = LeaveValueConstants.TransactionType.Reversal,
                    OriginalTransactionId = originalTransaction.Id,
                    OriginalTransaction = originalTransaction,
                    OpeningBalanceDelta = -originalTransaction.OpeningBalanceDelta,
                    EntitlementDelta = -originalTransaction.EntitlementDelta,
                    AccruedDelta = -originalTransaction.AccruedDelta,
                    CarryForwardDelta = -originalTransaction.CarryForwardDelta,
                    AdjustmentDelta = -originalTransaction.AdjustmentDelta,
                    CompensatoryDelta = -originalTransaction.CompensatoryDelta,
                    PendingDelta = -originalTransaction.PendingDelta,
                    ReservedDelta = -originalTransaction.ReservedDelta,
                    UsedDelta = -originalTransaction.UsedDelta,
                    RecalledDelta = -originalTransaction.RecalledDelta,
                    ExpiredDelta = -originalTransaction.ExpiredDelta,
                    EncashmentDelta = -originalTransaction.EncashmentDelta,
                    AvailableDelta = -originalTransaction.AvailableDelta
                };
            }

            var sign = string.Equals(
                adjustment.Direction,
                LeaveValueConstants.TransactionDirection.Debit,
                StringComparison.OrdinalIgnoreCase)
                ? -1m
                : 1m;
            var signedDays = postedDays * sign;

            if (string.Equals(
                    adjustment.AdjustmentType,
                    LeaveValueConstants.AdjustmentType.OpeningBalance,
                    StringComparison.OrdinalIgnoreCase))
            {
                if (sign < 0)
                {
                    return AdjustmentDeltas.Fail(
                        StatusCodes.Status400BadRequest,
                        "Opening balance hanya dapat menggunakan direction Credit.");
                }

                return new AdjustmentDeltas
                {
                    Success = true,
                    TransactionType = LeaveValueConstants.TransactionType.Opening,
                    OpeningBalanceDelta = signedDays,
                    AvailableDelta = signedDays
                };
            }

            return new AdjustmentDeltas
            {
                Success = true,
                TransactionType = LeaveValueConstants.TransactionType.ManualAdjustment,
                AdjustmentDelta = signedDays,
                AvailableDelta = signedDays
            };
        }

        private static void ApplyDeltas(WfpLeaveBalance balance, AdjustmentDeltas deltas)
        {
            balance.OpeningBalanceDays += deltas.OpeningBalanceDelta;
            balance.EntitlementDays += deltas.EntitlementDelta;
            balance.AccruedDays += deltas.AccruedDelta;
            balance.CarriedForwardDays += deltas.CarryForwardDelta;
            balance.AdjustmentDays += deltas.AdjustmentDelta;
            balance.CompensatoryDays += deltas.CompensatoryDelta;
            balance.PendingDays += deltas.PendingDelta;
            balance.ReservedDays += deltas.ReservedDelta;
            balance.UsedDays += deltas.UsedDelta;
            balance.RecalledDays += deltas.RecalledDelta;
            balance.ExpiredDays += deltas.ExpiredDelta;
            balance.EncashmentDays += deltas.EncashmentDelta;
        }

        private static void RecalculateBalance(WfpLeaveBalance balance)
        {
            balance.RemainingDays =
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

            balance.AvailableDays = balance.RemainingDays - balance.ReservedDays;
        }

        private static bool IsNegativeBalanceAllowed(
            decimal newAvailableDays,
            MstLeavePolicy? policy,
            out string? message)
        {
            message = null;
            if (newAvailableDays >= 0)
            {
                return true;
            }

            if (policy?.AllowNegativeBalance != true)
            {
                message = "Posting ditolak karena policy tidak mengizinkan saldo cuti negatif.";
                return false;
            }

            if (policy.NegativeBalanceLimitDays.HasValue &&
                newAvailableDays < -policy.NegativeBalanceLimitDays.Value)
            {
                message =
                    $"Posting ditolak karena saldo negatif melebihi batas {policy.NegativeBalanceLimitDays.Value:0.####} hari.";
                return false;
            }

            return true;
        }

        private static LeaveAdjustmentPostingResponse BuildResponse(
            TrxLeaveAdjustment adjustment,
            WfpLeaveBalance balance,
            TrxLeaveBalanceTransaction ledger,
            bool isIdempotent)
        {
            return new LeaveAdjustmentPostingResponse
            {
                LeaveAdjustmentId = adjustment.Id,
                AdjustmentNumber = adjustment.AdjustmentNumber,
                LeaveBalanceId = balance.Id,
                BalanceTransactionId = ledger.Id,
                TransactionNumber = ledger.TransactionNumber,
                TransactionSequence = ledger.TransactionSequence,
                TransactionType = ledger.TransactionType,
                Direction = ledger.Direction,
                PostedDays = adjustment.PostedDays > 0
                    ? adjustment.PostedDays
                    : ledger.TransactionDays,
                PreviousAvailableDays = ledger.PreviousAvailableDays,
                NewAvailableDays = ledger.NewAvailableDays,
                NewRemainingDays = balance.RemainingDays,
                BalanceVersion = balance.BalanceVersion,
                IsIdempotent = isIdempotent,
                PostedAt = ledger.PostedAt ?? ledger.TransactionDateTime
            };
        }

        private static string GenerateTransactionNumber()
        {
            return $"LBT-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..32].ToUpperInvariant();
        }

        private static string? NormalizeText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static async Task SafeRollbackAsync(
            Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction,
            CancellationToken cancellationToken)
        {
            try
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            catch
            {
                // Abaikan rollback failure agar error utama tetap dikembalikan.
            }
        }

        private sealed class AdjustmentDeltas
        {
            public bool Success { get; set; }
            public int StatusCode { get; set; }
            public string Message { get; set; } = string.Empty;
            public string TransactionType { get; set; } =
                LeaveValueConstants.TransactionType.ManualAdjustment;
            public Guid? OriginalTransactionId { get; set; }
            public TrxLeaveBalanceTransaction? OriginalTransaction { get; set; }
            public decimal OpeningBalanceDelta { get; set; }
            public decimal EntitlementDelta { get; set; }
            public decimal AccruedDelta { get; set; }
            public decimal CarryForwardDelta { get; set; }
            public decimal AdjustmentDelta { get; set; }
            public decimal CompensatoryDelta { get; set; }
            public decimal PendingDelta { get; set; }
            public decimal ReservedDelta { get; set; }
            public decimal UsedDelta { get; set; }
            public decimal RecalledDelta { get; set; }
            public decimal ExpiredDelta { get; set; }
            public decimal EncashmentDelta { get; set; }
            public decimal AvailableDelta { get; set; }

            public static AdjustmentDeltas Fail(int statusCode, string message)
            {
                return new AdjustmentDeltas
                {
                    Success = false,
                    StatusCode = statusCode,
                    Message = message
                };
            }
        }
    }
}
