using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Repositories;
using System.Data;
using System.Globalization;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveRequestReservationService
    {
        private const decimal Tolerance = 0.0001m;
        private readonly ApplicationDbContext _dbContext;

        public LeaveRequestReservationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<LeaveRequestServiceResult<TrxLeaveBalanceTransaction?>> ReserveAsync(
            WfpLeaveRequest leaveRequest,
            MstLeavePolicy policy,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var desiredReserved =
                leaveRequest.LeaveBalanceId.HasValue &&
                leaveRequest.EstimatedBalanceDeduction > 0 &&
                policy.ReservationTiming == LeaveValueConstants.ReservationTiming.OnSubmit
                    ? leaveRequest.EstimatedBalanceDeduction
                    : 0;

            return SynchronizeReservationAsync(
                leaveRequest,
                policy,
                desiredReserved,
                actorUserId,
                "Sinkronisasi reservasi saldo saat pengajuan disubmit.",
                cancellationToken);
        }

        public Task<LeaveRequestServiceResult<TrxLeaveBalanceTransaction?>> ReleaseAsync(
            WfpLeaveRequest leaveRequest,
            Guid actorUserId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            return SynchronizeReservationAsync(
                leaveRequest,
                leaveRequest.LeavePolicy ?? new MstLeavePolicy(),
                0,
                actorUserId,
                reason,
                cancellationToken);
        }

        public async Task<bool> HasActiveReservationAsync(
            Guid leaveRequestId,
            CancellationToken cancellationToken = default)
        {
            var total = await _dbContext.Set<TrxLeaveBalanceTransaction>()
                .AsNoTracking()
                .Where(x =>
                    x.LeaveRequestId == leaveRequestId &&
                    x.TransactionStatus == LeaveValueConstants.TransactionStatus.Posted &&
                    !x.IsDelete)
                .SumAsync(x => (decimal?)x.ReservedDelta, cancellationToken) ?? 0;

            return total > Tolerance;
        }

        private async Task<LeaveRequestServiceResult<TrxLeaveBalanceTransaction?>> SynchronizeReservationAsync(
            WfpLeaveRequest leaveRequest,
            MstLeavePolicy policy,
            decimal desiredReserved,
            Guid actorUserId,
            string reason,
            CancellationToken cancellationToken)
        {
            if (!leaveRequest.LeaveBalanceId.HasValue)
            {
                return LeaveRequestServiceResult<TrxLeaveBalanceTransaction?>.Ok(
                    null,
                    "Pengajuan tidak mempunyai saldo yang perlu disinkronkan.");
            }

            await using IDbContextTransaction transaction =
                await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            try
            {
                var currentReserved = await _dbContext.Set<TrxLeaveBalanceTransaction>()
                    .AsNoTracking()
                    .Where(x =>
                        x.LeaveRequestId == leaveRequest.Id &&
                        x.TransactionStatus == LeaveValueConstants.TransactionStatus.Posted &&
                        !x.IsDelete)
                    .SumAsync(x => (decimal?)x.ReservedDelta, cancellationToken) ?? 0;

                var delta = desiredReserved - currentReserved;
                if (Math.Abs(delta) <= Tolerance)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return LeaveRequestServiceResult<TrxLeaveBalanceTransaction?>.Ok(
                        null,
                        "Reservasi saldo sudah sesuai dengan kebutuhan pengajuan.");
                }

                var balance = await LockBalanceAsync(
                    leaveRequest.LeaveBalanceId.Value,
                    cancellationToken);

                if (balance == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeaveRequestServiceResult<TrxLeaveBalanceTransaction?>.Fail(
                        StatusCodes.Status404NotFound,
                        "Leave balance tidak ditemukan.");
                }

                if (delta > 0 &&
                    (balance.IsLocked ||
                     balance.BalanceStatus == LeaveValueConstants.BalanceStatus.Locked ||
                     balance.BalanceStatus == LeaveValueConstants.BalanceStatus.Closed))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeaveRequestServiceResult<TrxLeaveBalanceTransaction?>.Fail(
                        StatusCodes.Status409Conflict,
                        "Leave balance sedang dikunci atau sudah ditutup.");
                }

                if (delta > 0)
                {
                    var minimumAllowed = policy.AllowNegativeBalance
                        ? -(policy.NegativeBalanceLimitDays ?? decimal.MaxValue)
                        : 0;

                    if (balance.AvailableDays - delta < minimumAllowed)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return LeaveRequestServiceResult<TrxLeaveBalanceTransaction?>.Fail(
                            StatusCodes.Status409Conflict,
                            "Saldo tidak mencukupi untuk reservasi pengajuan cuti.");
                    }
                }

                var actualDelta = delta;
                if (actualDelta < 0)
                {
                    actualDelta = -Math.Min(
                        Math.Abs(actualDelta),
                        Math.Max(0, balance.ReservedDays));
                }

                if (Math.Abs(actualDelta) <= Tolerance)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return LeaveRequestServiceResult<TrxLeaveBalanceTransaction?>.Ok(
                        null,
                        "Tidak ada reservasi yang perlu diperbarui.");
                }

                var desiredKey = desiredReserved.ToString("0.####", CultureInfo.InvariantCulture);
                var idempotencyKey = $"LEAVE-RESERVATION:{leaveRequest.Id:N}:{desiredKey}";
                var duplicate = await _dbContext.Set<TrxLeaveBalanceTransaction>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.IdempotencyKey == idempotencyKey &&
                        !x.IsDelete,
                        cancellationToken);

                if (duplicate != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return LeaveRequestServiceResult<TrxLeaveBalanceTransaction?>.Ok(
                        duplicate,
                        "Perubahan reservasi sudah pernah diproses.");
                }

                var now = DateTime.UtcNow;
                var isReservation = actualDelta > 0;
                var ledger = new TrxLeaveBalanceTransaction
                {
                    Id = Guid.NewGuid(),
                    TransactionNumber = GenerateTransactionNumber(),
                    LeaveBalanceId = balance.Id,
                    WorkforceProfileId = balance.WorkforceProfileId,
                    LeaveTypeId = balance.LeaveTypeId,
                    LeaveEntitlementPeriodId = balance.LeaveEntitlementPeriodId,
                    LeaveRequestId = leaveRequest.Id,
                    TransactionDateTime = now,
                    EffectiveDate = leaveRequest.StartDate,
                    TransactionSequence = balance.LastTransactionSequence + 1,
                    TransactionType = isReservation
                        ? LeaveValueConstants.TransactionType.Reservation
                        : LeaveValueConstants.TransactionType.ReservationRelease,
                    Direction = isReservation
                        ? LeaveValueConstants.TransactionDirection.Debit
                        : LeaveValueConstants.TransactionDirection.Credit,
                    TransactionDays = Math.Abs(actualDelta),
                    ReservedDelta = actualDelta,
                    AvailableDelta = -actualDelta,
                    PreviousOpeningBalanceDays = balance.OpeningBalanceDays,
                    PreviousAvailableDays = balance.AvailableDays,
                    PreviousReservedDays = balance.ReservedDays,
                    NewAvailableDays = balance.AvailableDays - actualDelta,
                    NewReservedDays = balance.ReservedDays + actualDelta,
                    NewUsedDays = balance.UsedDays,
                    IdempotencyKey = idempotencyKey,
                    PostingBatchType = LeaveValueConstants.PostingBatchType.LeaveRequest,
                    PostingBatchId = leaveRequest.Id,
                    SourceType = isReservation
                        ? "LeaveRequestSubmit"
                        : "LeaveRequestReservationRelease",
                    SourceReferenceId = leaveRequest.Id,
                    SourceReferenceNumber = leaveRequest.RequestNumber,
                    TransactionStatus = LeaveValueConstants.TransactionStatus.Posted,
                    PostedAt = now,
                    PostedByUserId = actorUserId == Guid.Empty ? null : actorUserId,
                    Remarks = reason,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };

                balance.ReservedDays += actualDelta;
                balance.AvailableDays -= actualDelta;
                balance.LastTransactionId = ledger.Id;
                balance.LastTransactionSequence = ledger.TransactionSequence;
                balance.BalanceVersion += 1;
                balance.LastCalculatedAt = now;
                balance.UpdateDateTime = now;
                balance.UpdateBy = actorUserId;

                _dbContext.Add(ledger);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return LeaveRequestServiceResult<TrxLeaveBalanceTransaction?>.Ok(
                    ledger,
                    isReservation
                        ? "Saldo pengajuan cuti berhasil direservasi."
                        : "Reservasi saldo berhasil dilepas.");
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);
                return LeaveRequestServiceResult<TrxLeaveBalanceTransaction?>.Fail(
                    StatusCodes.Status409Conflict,
                    "Sinkronisasi reservasi gagal karena konflik transaksi atau request sudah diproses.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return LeaveRequestServiceResult<TrxLeaveBalanceTransaction?>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Sinkronisasi reservasi saldo gagal: {ex.Message}");
            }
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

        private static string GenerateTransactionNumber() =>
            $"LBT-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }
}
