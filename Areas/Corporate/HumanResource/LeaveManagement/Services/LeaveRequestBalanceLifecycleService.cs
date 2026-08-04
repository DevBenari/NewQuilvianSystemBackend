using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Repositories;
using System.Data;
using System.Globalization;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    /// <summary>
    /// Menjaga hubungan workflow LEAVE_REQUEST dengan immutable leave balance ledger.
    /// Service ini tidak mengambil keputusan approval. Ia hanya menerapkan dampak saldo
    /// dari status workflow yang sudah sah.
    /// </summary>
    public class LeaveRequestBalanceLifecycleService
    {
        private const decimal Tolerance = 0.0001m;

        private readonly ApplicationDbContext _dbContext;

        public LeaveRequestBalanceLifecycleService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>>
            ApplyWorkflowStatusAsync(
                Guid leaveRequestId,
                string leaveRequestStatus,
                Guid actorUserId,
                CancellationToken cancellationToken = default)
        {
            var request = await _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .Include(x => x.LeavePolicy)
                .Include(x => x.LeaveType)
                .FirstOrDefaultAsync(
                    x => x.Id == leaveRequestId && !x.IsDelete,
                    cancellationToken);

            if (request == null)
            {
                return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan cuti tidak ditemukan.");
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

            if (request.LeavePolicy == null)
            {
                return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Leave policy pengajuan tidak ditemukan.");
            }

            if (string.Equals(
                    leaveRequestStatus,
                    LeaveRequestValueConstants.Status.NeedRevision,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    leaveRequestStatus,
                    LeaveRequestValueConstants.Status.Rejected,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    leaveRequestStatus,
                    LeaveRequestValueConstants.Status.Cancelled,
                    StringComparison.OrdinalIgnoreCase))
            {
                return await SynchronizeReservationAsync(
                    request,
                    request.LeavePolicy,
                    desiredReservedDays: 0,
                    actorUserId,
                    "ReservationRelease",
                    $"Reservasi dilepas karena pengajuan berstatus {leaveRequestStatus}.",
                    cancellationToken);
            }

            if (string.Equals(
                    leaveRequestStatus,
                    LeaveRequestValueConstants.Status.WaitingApproval,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    leaveRequestStatus,
                    LeaveRequestValueConstants.Status.Submitted,
                    StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(
                        request.LeavePolicy.ReservationTiming,
                        LeaveValueConstants.ReservationTiming.OnSubmit,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return await SynchronizeReservationAsync(
                        request,
                        request.LeavePolicy,
                        request.EstimatedBalanceDeduction,
                        actorUserId,
                        "Reservation",
                        "Reservasi saldo diselaraskan ketika workflow menunggu approval.",
                        cancellationToken);
                }

                return await GetCurrentStateAsync(
                    request,
                    "None",
                    cancellationToken);
            }

            if (string.Equals(
                    leaveRequestStatus,
                    LeaveRequestValueConstants.Status.Approved,
                    StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(
                        request.LeavePolicy.DeductionTiming,
                        LeaveValueConstants.DeductionTiming.OnApproval,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return await ConvertReservationToUsageAsync(
                        request,
                        request.LeavePolicy,
                        actorUserId,
                        cancellationToken);
                }

                if (string.Equals(
                        request.LeavePolicy.ReservationTiming,
                        LeaveValueConstants.ReservationTiming.OnApproval,
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        request.LeavePolicy.ReservationTiming,
                        LeaveValueConstants.ReservationTiming.OnSubmit,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return await SynchronizeReservationAsync(
                        request,
                        request.LeavePolicy,
                        request.EstimatedBalanceDeduction,
                        actorUserId,
                        "Reservation",
                        "Reservasi saldo diselaraskan setelah workflow disetujui.",
                        cancellationToken);
                }
            }

            return await GetCurrentStateAsync(
                request,
                "None",
                cancellationToken);
        }

        public async Task<LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>>
            RetryApprovedBalanceAsync(
                Guid leaveRequestId,
                Guid actorUserId,
                CancellationToken cancellationToken = default)
        {
            var request = await _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == leaveRequestId && !x.IsDelete,
                    cancellationToken);

            if (request == null)
            {
                return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan cuti tidak ditemukan.");
            }

            if (!string.Equals(
                    request.LeaveRequestStatus,
                    LeaveRequestValueConstants.Status.Approved,
                    StringComparison.OrdinalIgnoreCase))
            {
                return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Retry balance hanya dapat dilakukan untuk pengajuan Approved.");
            }

            return await ApplyWorkflowStatusAsync(
                leaveRequestId,
                request.LeaveRequestStatus,
                actorUserId,
                cancellationToken);
        }

        private async Task<LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>>
            SynchronizeReservationAsync(
                WfpLeaveRequest request,
                MstLeavePolicy policy,
                decimal desiredReservedDays,
                Guid actorUserId,
                string actionType,
                string remarks,
                CancellationToken cancellationToken)
        {
            await using IDbContextTransaction transaction =
                await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            try
            {
                var currentReserved = await GetCurrentReservationAsync(
                    request.Id,
                    cancellationToken);

                var beforeUsed = await GetCurrentUsedAsync(
                    request.Id,
                    cancellationToken);

                var delta = desiredReservedDays - currentReserved;

                if (Math.Abs(delta) <= Tolerance)
                {
                    await transaction.CommitAsync(cancellationToken);

                    return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Ok(
                        new LeaveRequestBalanceLifecycleResponse
                        {
                            LeaveRequestId = request.Id,
                            LeaveBalanceId = request.LeaveBalanceId,
                            ActionType = actionType,
                            IsIdempotent = true,
                            ReservationBeforeDays = currentReserved,
                            ReservationAfterDays = currentReserved,
                            UsedBeforeDays = beforeUsed,
                            UsedAfterDays = beforeUsed
                        },
                        "Reservasi saldo sudah sesuai.");
                }

                var balance = await LockBalanceAsync(
                    request.LeaveBalanceId!.Value,
                    cancellationToken);

                if (balance == null)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Leave balance tidak ditemukan.");
                }

                if (delta > 0 &&
                    (balance.IsLocked ||
                     string.Equals(
                         balance.BalanceStatus,
                         LeaveValueConstants.BalanceStatus.Locked,
                         StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(
                         balance.BalanceStatus,
                         LeaveValueConstants.BalanceStatus.Closed,
                         StringComparison.OrdinalIgnoreCase)))
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Leave balance sedang dikunci atau sudah ditutup.");
                }

                var actualDelta = delta;

                if (actualDelta < 0)
                {
                    actualDelta = -Math.Min(
                        Math.Abs(actualDelta),
                        Math.Min(currentReserved, Math.Max(0, balance.ReservedDays)));
                }

                if (actualDelta > 0)
                {
                    var afterAvailable = balance.AvailableDays - actualDelta;
                    var minimumAllowed = policy.AllowNegativeBalance
                        ? -(policy.NegativeBalanceLimitDays ?? decimal.MaxValue)
                        : 0;

                    if (afterAvailable < minimumAllowed)
                    {
                        await transaction.RollbackAsync(cancellationToken);

                        return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                            StatusCodes.Status409Conflict,
                            "Saldo cuti tidak mencukupi untuk reservasi workflow.");
                    }
                }

                if (Math.Abs(actualDelta) <= Tolerance)
                {
                    await transaction.CommitAsync(cancellationToken);

                    return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Ok(
                        new LeaveRequestBalanceLifecycleResponse
                        {
                            LeaveRequestId = request.Id,
                            LeaveBalanceId = balance.Id,
                            ActionType = actionType,
                            IsIdempotent = true,
                            ReservationBeforeDays = currentReserved,
                            ReservationAfterDays = currentReserved,
                            UsedBeforeDays = beforeUsed,
                            UsedAfterDays = beforeUsed,
                            AvailableBeforeDays = balance.AvailableDays,
                            AvailableAfterDays = balance.AvailableDays
                        },
                        "Tidak ada reservasi yang dapat diperbarui.");
                }

                var desiredKey = desiredReservedDays.ToString(
                    "0.####",
                    CultureInfo.InvariantCulture);

                var idempotencyKey = $"LEAVE-RESERVATION:{request.Id:N}:{desiredKey}";

                var existing = await _dbContext.Set<TrxLeaveBalanceTransaction>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.IdempotencyKey == idempotencyKey && !x.IsDelete,
                        cancellationToken);

                if (existing != null)
                {
                    await transaction.CommitAsync(cancellationToken);

                    return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Ok(
                        new LeaveRequestBalanceLifecycleResponse
                        {
                            LeaveRequestId = request.Id,
                            LeaveBalanceId = balance.Id,
                            ActionType = actionType,
                            IsIdempotent = true,
                            ReservationBeforeDays = currentReserved,
                            ReservationAfterDays = currentReserved,
                            UsedBeforeDays = beforeUsed,
                            UsedAfterDays = beforeUsed,
                            AvailableBeforeDays = balance.AvailableDays,
                            AvailableAfterDays = balance.AvailableDays,
                            BalanceTransactionId = existing.Id
                        },
                        "Status reservasi tersebut sudah pernah diposting.");
                }

                var availableBefore = balance.AvailableDays;
                var transactionType = actualDelta > 0
                    ? LeaveValueConstants.TransactionType.Reservation
                    : LeaveValueConstants.TransactionType.ReservationRelease;

                var ledger = CreateLedger(
                    request,
                    balance,
                    transactionType,
                    actualDelta > 0
                        ? LeaveValueConstants.TransactionDirection.Debit
                        : LeaveValueConstants.TransactionDirection.Credit,
                    Math.Abs(actualDelta),
                    actorUserId,
                    idempotencyKey,
                    remarks,
                    reservedDelta: actualDelta,
                    usedDelta: 0,
                    availableDelta: -actualDelta,
                    sourceType: actualDelta > 0
                        ? "LeaveRequestWorkflowReservation"
                        : "LeaveRequestWorkflowReservationRelease");

                balance.ReservedDays += actualDelta;
                balance.AvailableDays -= actualDelta;
                ApplyBalanceAudit(balance, ledger, actorUserId);

                ledger.NewAvailableDays = balance.AvailableDays;
                ledger.NewReservedDays = balance.ReservedDays;
                ledger.NewUsedDays = balance.UsedDays;

                _dbContext.Add(ledger);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Ok(
                    new LeaveRequestBalanceLifecycleResponse
                    {
                        LeaveRequestId = request.Id,
                        LeaveBalanceId = balance.Id,
                        ActionType = actualDelta > 0 ? "Reservation" : "ReservationRelease",
                        ActionAttempted = true,
                        ReservationBeforeDays = currentReserved,
                        ReservationAfterDays = currentReserved + actualDelta,
                        UsedBeforeDays = beforeUsed,
                        UsedAfterDays = beforeUsed,
                        AvailableBeforeDays = availableBefore,
                        AvailableAfterDays = balance.AvailableDays,
                        BalanceTransactionId = ledger.Id
                    },
                    actualDelta > 0
                        ? "Reservasi saldo workflow berhasil diposting."
                        : "Reservasi saldo workflow berhasil dilepas.");
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);

                return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Posting reservasi gagal karena konflik transaksi atau idempotency key.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Posting reservasi workflow gagal: {ex.Message}");
            }
        }

        private async Task<LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>>
            ConvertReservationToUsageAsync(
                WfpLeaveRequest request,
                MstLeavePolicy policy,
                Guid actorUserId,
                CancellationToken cancellationToken)
        {
            await using IDbContextTransaction transaction =
                await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            try
            {
                var idempotencyKey = $"LEAVE-REQUEST-DEDUCTION:{request.Id:N}:ON_APPROVAL";

                var existing = await _dbContext.Set<TrxLeaveBalanceTransaction>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.IdempotencyKey == idempotencyKey && !x.IsDelete,
                        cancellationToken);

                var currentReserved = await GetCurrentReservationAsync(
                    request.Id,
                    cancellationToken);

                var currentUsed = await GetCurrentUsedAsync(
                    request.Id,
                    cancellationToken);

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
                        "Deduction pengajuan sudah pernah diposting.");
                }

                var balance = await LockBalanceAsync(
                    request.LeaveBalanceId!.Value,
                    cancellationToken);

                if (balance == null)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Leave balance tidak ditemukan.");
                }

                if (balance.IsLocked ||
                    string.Equals(
                        balance.BalanceStatus,
                        LeaveValueConstants.BalanceStatus.Locked,
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        balance.BalanceStatus,
                        LeaveValueConstants.BalanceStatus.Closed,
                        StringComparison.OrdinalIgnoreCase))
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
                        "Deduction pengajuan sudah sesuai.");
                }

                var availableDelta = reservationToRelease - usageToPost;
                var afterAvailable = balance.AvailableDays + availableDelta;
                var minimumAllowed = policy.AllowNegativeBalance
                    ? -(policy.NegativeBalanceLimitDays ?? decimal.MaxValue)
                    : 0;

                if (afterAvailable < minimumAllowed)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Saldo cuti tidak mencukupi untuk deduction saat approval.");
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
                    "Reservasi dikonversi menjadi pemakaian setelah workflow LEAVE_REQUEST disetujui.",
                    reservedDelta: -reservationToRelease,
                    usedDelta: usageToPost,
                    availableDelta: availableDelta,
                    sourceType: "LeaveRequestApprovalDeduction");

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
                    "Deduction saldo saat approval berhasil diposting.");
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
                    $"Posting deduction workflow gagal: {ex.Message}");
            }
        }

        private async Task<LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>>
            GetCurrentStateAsync(
                WfpLeaveRequest request,
                string actionType,
                CancellationToken cancellationToken)
        {
            var reserved = await GetCurrentReservationAsync(
                request.Id,
                cancellationToken);

            var used = await GetCurrentUsedAsync(
                request.Id,
                cancellationToken);

            return LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>.Ok(
                new LeaveRequestBalanceLifecycleResponse
                {
                    LeaveRequestId = request.Id,
                    LeaveBalanceId = request.LeaveBalanceId,
                    ActionType = actionType,
                    IsIdempotent = true,
                    ReservationBeforeDays = reserved,
                    ReservationAfterDays = reserved,
                    UsedBeforeDays = used,
                    UsedAfterDays = used
                },
                "Tidak ada perubahan balance yang diperlukan.");
        }

        private async Task<decimal> GetCurrentReservationAsync(
            Guid leaveRequestId,
            CancellationToken cancellationToken)
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

        private async Task<decimal> GetCurrentUsedAsync(
            Guid leaveRequestId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<TrxLeaveBalanceTransaction>()
                .AsNoTracking()
                .Where(x =>
                    x.LeaveRequestId == leaveRequestId &&
                    x.TransactionStatus == LeaveValueConstants.TransactionStatus.Posted &&
                    !x.IsDelete)
                .SumAsync(x => (decimal?)x.UsedDelta, cancellationToken) ?? 0;
        }

        private async Task<WfpLeaveBalance?> LockBalanceAsync(
            Guid balanceId,
            CancellationToken cancellationToken)
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

        private static void ApplyBalanceAudit(
            WfpLeaveBalance balance,
            TrxLeaveBalanceTransaction ledger,
            Guid actorUserId)
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
