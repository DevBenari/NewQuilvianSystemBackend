using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveExecutionBalanceService
    {
        private const decimal Tolerance = 0.0001m;
        private readonly ApplicationDbContext _dbContext;

        public LeaveExecutionBalanceService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>>
            ApplyDeductionStageAsync(
                Guid leaveRequestId,
                string stage,
                Guid actorUserId,
                CancellationToken cancellationToken = default)
        {
            var request = await _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .Include(x => x.LeavePolicy)
                .Include(x => x.LeaveType)
                .FirstOrDefaultAsync(x => x.Id == leaveRequestId && !x.IsDelete, cancellationToken);

            if (request == null)
            {
                return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan cuti tidak ditemukan.");
            }

            if (request.LeavePolicy == null)
            {
                return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Leave policy pengajuan tidak ditemukan.");
            }

            if (!string.Equals(request.LeavePolicy.DeductionTiming, stage, StringComparison.OrdinalIgnoreCase))
            {
                return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Ok(
                    new LeaveRequestBalanceLifecycleResponse
                    {
                        LeaveRequestId = request.Id,
                        LeaveBalanceId = request.LeaveBalanceId,
                        ActionType = "None",
                        IsIdempotent = true
                    },
                    $"DeductionTiming policy bukan {stage}; tidak ada perubahan balance.");
            }

            if (!request.LeaveBalanceId.HasValue ||
                request.LeaveType?.IsBalanceDeducted != true ||
                request.EstimatedBalanceDeduction <= Tolerance)
            {
                return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Ok(
                    new LeaveRequestBalanceLifecycleResponse
                    {
                        LeaveRequestId = request.Id,
                        LeaveBalanceId = request.LeaveBalanceId,
                        ActionType = "None",
                        IsIdempotent = true
                    },
                    "Pengajuan tidak mempunyai dampak leave balance.");
            }

            await using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var idempotencyKey = $"LEAVE-REQUEST-DEDUCTION:{request.Id:N}:{stage.ToUpperInvariant()}";
                var existing = await _dbContext.Set<TrxLeaveBalanceTransaction>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey && !x.IsDelete, cancellationToken);

                var currentReserved = await GetCurrentReservationAsync(request.Id, cancellationToken);
                var currentUsed = await GetCurrentUsedAsync(request.Id, cancellationToken);

                if (existing != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Ok(
                        new LeaveRequestBalanceLifecycleResponse
                        {
                            LeaveRequestId = request.Id,
                            LeaveBalanceId = request.LeaveBalanceId,
                            ActionType = "Deduction",
                            IsIdempotent = true,
                            ReservationBeforeDays = currentReserved,
                            ReservationAfterDays = currentReserved,
                            UsedBeforeDays = currentUsed,
                            UsedAfterDays = currentUsed,
                            BalanceTransactionId = existing.Id
                        },
                        "Deduction tahap eksekusi sudah pernah diposting.");
                }

                var balance = await LockBalanceAsync(request.LeaveBalanceId.Value, cancellationToken);
                if (balance == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Leave balance tidak ditemukan.");
                }

                if (balance.IsLocked ||
                    string.Equals(balance.BalanceStatus, LeaveValueConstants.BalanceStatus.Locked, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(balance.BalanceStatus, LeaveValueConstants.BalanceStatus.Closed, StringComparison.OrdinalIgnoreCase))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Leave balance sedang dikunci atau sudah ditutup.");
                }

                var desiredUsage = request.EstimatedBalanceDeduction;
                var usageToPost = Math.Max(0, desiredUsage - currentUsed);
                var reservationToRelease = Math.Min(
                    Math.Max(0, currentReserved),
                    Math.Max(0, balance.ReservedDays));

                if (usageToPost <= Tolerance && reservationToRelease <= Tolerance)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Ok(
                        new LeaveRequestBalanceLifecycleResponse
                        {
                            LeaveRequestId = request.Id,
                            LeaveBalanceId = balance.Id,
                            ActionType = "Deduction",
                            IsIdempotent = true,
                            ReservationBeforeDays = currentReserved,
                            ReservationAfterDays = currentReserved,
                            UsedBeforeDays = currentUsed,
                            UsedAfterDays = currentUsed,
                            AvailableBeforeDays = balance.AvailableDays,
                            AvailableAfterDays = balance.AvailableDays
                        },
                        "Deduction tahap eksekusi sudah sesuai.");
                }

                var availableDelta = reservationToRelease - usageToPost;
                var afterAvailable = balance.AvailableDays + availableDelta;
                var minimumAllowed = request.LeavePolicy.AllowNegativeBalance
                    ? -(request.LeavePolicy.NegativeBalanceLimitDays ?? decimal.MaxValue)
                    : 0;

                if (afterAvailable < minimumAllowed)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Saldo cuti tidak mencukupi untuk deduction tahap eksekusi.");
                }

                var availableBefore = balance.AvailableDays;
                var usedBefore = balance.UsedDays;
                var ledger = CreateLedger(
                    request,
                    balance,
                    LeaveValueConstants.TransactionType.Deduction,
                    LeaveValueConstants.TransactionDirection.Debit,
                    usageToPost,
                    actorUserId,
                    idempotencyKey,
                    $"Reservasi dikonversi menjadi pemakaian pada tahap {stage}.",
                    reservedDelta: -reservationToRelease,
                    usedDelta: usageToPost,
                    availableDelta: availableDelta,
                    sourceType: $"LeaveRequest{stage}Deduction");

                balance.ReservedDays -= reservationToRelease;
                balance.UsedDays += usageToPost;
                balance.RemainingDays = CalculateRemaining(balance);
                balance.AvailableDays = balance.RemainingDays - balance.ReservedDays;
                ApplyBalanceAudit(balance, ledger, actorUserId);

                ledger.NewAvailableDays = balance.AvailableDays;
                ledger.NewReservedDays = balance.ReservedDays;
                ledger.NewUsedDays = balance.UsedDays;

                var trackedRequest = await _dbContext.Set<WfpLeaveRequest>()
                    .FirstAsync(x => x.Id == request.Id, cancellationToken);
                trackedRequest.ActualBalanceDeduction = currentUsed + usageToPost;
                trackedRequest.UpdateDateTime = DateTime.UtcNow;
                trackedRequest.UpdateBy = actorUserId;

                _dbContext.Add(ledger);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Ok(
                    new LeaveRequestBalanceLifecycleResponse
                    {
                        LeaveRequestId = request.Id,
                        LeaveBalanceId = balance.Id,
                        ActionType = "Deduction",
                        ActionAttempted = true,
                        ReservationBeforeDays = currentReserved,
                        ReservationAfterDays = Math.Max(0, currentReserved - reservationToRelease),
                        UsedBeforeDays = currentUsed,
                        UsedAfterDays = currentUsed + usageToPost,
                        AvailableBeforeDays = availableBefore,
                        AvailableAfterDays = balance.AvailableDays,
                        BalanceTransactionId = ledger.Id
                    },
                    $"Deduction saldo pada tahap {stage} berhasil diposting.");
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);
                return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Posting deduction gagal karena konflik transaksi atau idempotency key.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Posting deduction tahap eksekusi gagal: {ex.Message}");
            }
        }

        public async Task<LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>> RestoreAsync(
            Guid leaveRequestId,
            decimal? restoreDays,
            Guid actorUserId,
            string reason,
            string sourceSuffix,
            CancellationToken cancellationToken = default)
        {
            var request = await _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .Include(x => x.LeaveType)
                .FirstOrDefaultAsync(x => x.Id == leaveRequestId && !x.IsDelete, cancellationToken);

            if (request == null)
            {
                return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan cuti tidak ditemukan.");
            }

            if (!request.LeaveBalanceId.HasValue || request.LeaveType?.IsBalanceDeducted != true)
            {
                return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Ok(
                    new LeaveRequestBalanceLifecycleResponse
                    {
                        LeaveRequestId = request.Id,
                        LeaveBalanceId = request.LeaveBalanceId,
                        ActionType = "None",
                        IsIdempotent = true
                    },
                    "Pengajuan tidak mempunyai saldo yang perlu direstore.");
            }

            await using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var idempotencyKey = $"LEAVE-REQUEST-RESTORE:{request.Id:N}:{sourceSuffix.ToUpperInvariant()}";
                var existing = await _dbContext.Set<TrxLeaveBalanceTransaction>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey && !x.IsDelete, cancellationToken);

                var currentReserved = await GetCurrentReservationAsync(request.Id, cancellationToken);
                var currentUsed = await GetCurrentUsedAsync(request.Id, cancellationToken);

                if (existing != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Ok(
                        new LeaveRequestBalanceLifecycleResponse
                        {
                            LeaveRequestId = request.Id,
                            LeaveBalanceId = request.LeaveBalanceId,
                            ActionType = "Restore",
                            IsIdempotent = true,
                            ReservationBeforeDays = currentReserved,
                            ReservationAfterDays = currentReserved,
                            UsedBeforeDays = currentUsed,
                            UsedAfterDays = currentUsed,
                            BalanceTransactionId = existing.Id
                        },
                        "Restore saldo sudah pernah diposting.");
                }

                var balance = await LockBalanceAsync(request.LeaveBalanceId.Value, cancellationToken);
                if (balance == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Leave balance tidak ditemukan.");
                }

                if (balance.IsLocked ||
                    string.Equals(balance.BalanceStatus, LeaveValueConstants.BalanceStatus.Closed, StringComparison.OrdinalIgnoreCase))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Leave balance sedang dikunci atau sudah ditutup.");
                }

                var requestedRestore = restoreDays.HasValue && restoreDays.Value > 0
                    ? restoreDays.Value
                    : currentReserved + currentUsed;

                var reservedRestore = Math.Min(Math.Max(0, currentReserved), requestedRestore);
                var remainingRestore = Math.Max(0, requestedRestore - reservedRestore);
                var usedRestore = Math.Min(Math.Max(0, currentUsed), remainingRestore);

                if (reservedRestore <= Tolerance && usedRestore <= Tolerance)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Ok(
                        new LeaveRequestBalanceLifecycleResponse
                        {
                            LeaveRequestId = request.Id,
                            LeaveBalanceId = balance.Id,
                            ActionType = "Restore",
                            IsIdempotent = true,
                            ReservationBeforeDays = currentReserved,
                            ReservationAfterDays = currentReserved,
                            UsedBeforeDays = currentUsed,
                            UsedAfterDays = currentUsed,
                            AvailableBeforeDays = balance.AvailableDays,
                            AvailableAfterDays = balance.AvailableDays
                        },
                        "Tidak ada saldo pengajuan yang perlu direstore.");
                }

                var availableBefore = balance.AvailableDays;
                var ledger = CreateLedger(
                    request,
                    balance,
                    LeaveValueConstants.TransactionType.CancellationRestore,
                    LeaveValueConstants.TransactionDirection.Credit,
                    reservedRestore + usedRestore,
                    actorUserId,
                    idempotencyKey,
                    reason,
                    reservedDelta: -reservedRestore,
                    usedDelta: -usedRestore,
                    availableDelta: reservedRestore + usedRestore,
                    sourceType: $"LeaveRequestRestore{sourceSuffix}");

                balance.ReservedDays -= reservedRestore;
                balance.UsedDays -= usedRestore;
                balance.RemainingDays = CalculateRemaining(balance);
                balance.AvailableDays = balance.RemainingDays - balance.ReservedDays;
                ApplyBalanceAudit(balance, ledger, actorUserId);

                ledger.NewAvailableDays = balance.AvailableDays;
                ledger.NewReservedDays = balance.ReservedDays;
                ledger.NewUsedDays = balance.UsedDays;

                var trackedRequest = await _dbContext.Set<WfpLeaveRequest>()
                    .FirstAsync(x => x.Id == request.Id, cancellationToken);
                trackedRequest.ActualBalanceDeduction = Math.Max(0, currentUsed - usedRestore);
                trackedRequest.UpdateDateTime = DateTime.UtcNow;
                trackedRequest.UpdateBy = actorUserId;

                _dbContext.Add(ledger);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Ok(
                    new LeaveRequestBalanceLifecycleResponse
                    {
                        LeaveRequestId = request.Id,
                        LeaveBalanceId = balance.Id,
                        ActionType = "Restore",
                        ActionAttempted = true,
                        ReservationBeforeDays = currentReserved,
                        ReservationAfterDays = Math.Max(0, currentReserved - reservedRestore),
                        UsedBeforeDays = currentUsed,
                        UsedAfterDays = Math.Max(0, currentUsed - usedRestore),
                        AvailableBeforeDays = availableBefore,
                        AvailableAfterDays = balance.AvailableDays,
                        BalanceTransactionId = ledger.Id
                    },
                    "Saldo pengajuan cuti berhasil direstore.");
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);
                return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Restore saldo gagal karena konflik transaksi atau idempotency key.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Restore saldo gagal: {ex.Message}");
            }
        }

        private async Task<decimal> GetCurrentReservationAsync(Guid leaveRequestId, CancellationToken cancellationToken)
        {
            var value = await _dbContext.Set<TrxLeaveBalanceTransaction>()
                .AsNoTracking()
                .Where(x =>
                    x.LeaveRequestId == leaveRequestId &&
                    x.TransactionStatus == LeaveValueConstants.TransactionStatus.Posted &&
                    !x.IsDelete)
                .SumAsync(x => (decimal?)x.ReservedDelta, cancellationToken) ?? 0;
            return Math.Max(0, value);
        }

        private async Task<decimal> GetCurrentUsedAsync(Guid leaveRequestId, CancellationToken cancellationToken)
        {
            return await _dbContext.Set<TrxLeaveBalanceTransaction>()
                .AsNoTracking()
                .Where(x =>
                    x.LeaveRequestId == leaveRequestId &&
                    x.TransactionStatus == LeaveValueConstants.TransactionStatus.Posted &&
                    !x.IsDelete)
                .SumAsync(x => (decimal?)x.UsedDelta, cancellationToken) ?? 0;
        }

        private async Task<WfpLeaveBalance?> LockBalanceAsync(Guid balanceId, CancellationToken cancellationToken)
        {
            return await _dbContext.Set<WfpLeaveBalance>()
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM public.""WfpLeaveBalance""
                    WHERE ""Id"" = {balanceId}
                      AND ""IsDelete"" = false
                    FOR UPDATE")
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static TrxLeaveBalanceTransaction CreateLedger(
            WfpLeaveRequest request,
            WfpLeaveBalance balance,
            string transactionType,
            string direction,
            decimal transactionDays,
            Guid actorUserId,
            string idempotencyKey,
            string remarks,
            decimal reservedDelta,
            decimal usedDelta,
            decimal availableDelta,
            string sourceType)
        {
            return new TrxLeaveBalanceTransaction
            {
                Id = Guid.NewGuid(),
                TransactionNumber = GenerateTransactionNumber(),
                LeaveBalanceId = balance.Id,
                WorkforceProfileId = balance.WorkforceProfileId,
                LeaveTypeId = balance.LeaveTypeId,
                LeaveEntitlementPeriodId = balance.LeaveEntitlementPeriodId,
                LeaveRequestId = request.Id,
                TransactionDateTime = DateTime.UtcNow,
                EffectiveDate = request.StartDate,
                TransactionSequence = balance.LastTransactionSequence + 1,
                TransactionType = transactionType,
                Direction = direction,
                TransactionDays = transactionDays,
                ReservedDelta = reservedDelta,
                UsedDelta = usedDelta,
                AvailableDelta = availableDelta,
                PreviousOpeningBalanceDays = balance.OpeningBalanceDays,
                PreviousAvailableDays = balance.AvailableDays,
                PreviousReservedDays = balance.ReservedDays,
                NewAvailableDays = balance.AvailableDays + availableDelta,
                NewReservedDays = balance.ReservedDays + reservedDelta,
                NewUsedDays = balance.UsedDays + usedDelta,
                IdempotencyKey = idempotencyKey,
                PostingBatchType = LeaveValueConstants.PostingBatchType.LeaveRequest,
                PostingBatchId = request.Id,
                SourceType = sourceType,
                SourceReferenceId = request.Id,
                SourceReferenceNumber = request.RequestNumber,
                TransactionStatus = LeaveValueConstants.TransactionStatus.Posted,
                PostedAt = DateTime.UtcNow,
                PostedByUserId = actorUserId == Guid.Empty ? null : actorUserId,
                Remarks = remarks,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
        }

        private static void ApplyBalanceAudit(WfpLeaveBalance balance, TrxLeaveBalanceTransaction ledger, Guid actorUserId)
        {
            balance.LastTransactionId = ledger.Id;
            balance.LastTransactionSequence = ledger.TransactionSequence;
            balance.BalanceVersion += 1;
            balance.LastCalculatedAt = DateTime.UtcNow;
            balance.UpdateDateTime = DateTime.UtcNow;
            balance.UpdateBy = actorUserId;
        }

        private static decimal CalculateRemaining(WfpLeaveBalance balance)
        {
            return balance.OpeningBalanceDays +
                   balance.EntitlementDays +
                   balance.AccruedDays +
                   balance.CarriedForwardDays +
                   balance.AdjustmentDays +
                   balance.CompensatoryDays +
                   balance.RecalledDays -
                   balance.UsedDays -
                   balance.ExpiredDays -
                   balance.EncashmentDays;
        }

        private static string GenerateTransactionNumber()
        {
            return $"LBT-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        }
    }
}
