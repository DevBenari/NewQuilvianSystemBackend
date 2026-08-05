using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimeCompensatoryLeaveService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly OvertimePeriodGuardService _periodGuard;

        public OvertimeCompensatoryLeaveService(
            ApplicationDbContext dbContext,
            OvertimePeriodGuardService periodGuard)
        {
            _dbContext = dbContext;
            _periodGuard = periodGuard;
        }

        public async Task<OvertimeCompensatoryLeaveServiceResult<OvertimeCompensatoryLeavePreviewResponse>> PreviewAsync(
            Guid overtimeRealizationId,
            PreviewOvertimeCompensatoryLeaveRequest? request,
            CancellationToken cancellationToken = default)
        {
            request ??= new PreviewOvertimeCompensatoryLeaveRequest();
            var context = await BuildPostingContextAsync(overtimeRealizationId, request, cancellationToken);
            if (!context.Success)
            {
                return OvertimeCompensatoryLeaveServiceResult<OvertimeCompensatoryLeavePreviewResponse>.Fail(
                    context.StatusCode,
                    context.Message);
            }

            return OvertimeCompensatoryLeaveServiceResult<OvertimeCompensatoryLeavePreviewResponse>.Ok(
                MapPreview(context, context.ExistingCredit != null),
                context.ExistingCredit == null
                    ? "Preview compensatory leave berhasil dihitung."
                    : "Compensatory leave untuk realization tersebut sudah tersedia.");
        }

        public async Task<OvertimeCompensatoryLeaveServiceResult<OvertimeCompensatoryLeaveMutationResponse>> PostAsync(
            Guid overtimeRealizationId,
            PostOvertimeCompensatoryLeaveRequest? request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
            {
                return FailMutation(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid.");
            }

            request ??= new PostOvertimeCompensatoryLeaveRequest();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                await AcquireLockAsync("OTC-REALIZATION-" + overtimeRealizationId, cancellationToken);
                var context = await BuildPostingContextAsync(overtimeRealizationId, request, cancellationToken);
                if (!context.Success)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return FailMutation(context.StatusCode, context.Message);
                }

                if (context.ExistingCredit != null)
                {
                    var existingBalance = await ResolveBalanceFromCreditAsync(context.ExistingCredit, cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return OvertimeCompensatoryLeaveServiceResult<OvertimeCompensatoryLeaveMutationResponse>.Ok(
                        MapMutation(context.ExistingCredit, context.Realization!, existingBalance, true, false),
                        "Compensatory leave sudah pernah diposting untuk realization tersebut.");
                }

                var realization = context.Realization!;
                var verification = context.Verification!;
                var leaveType = context.LeaveType!;
                var now = DateTime.UtcNow;
                var credit = new TrxCompensatoryTimeOff
                {
                    Id = Guid.NewGuid(),
                    CreditNumber = GenerateCreditNumber(),
                    WorkforceProfileId = realization.WorkforceProfileId,
                    EmployeeId = realization.EmployeeId,
                    OvertimeRequestId = realization.OvertimeRequestId,
                    OvertimeRealizationId = realization.Id,
                    OvertimeVerificationId = verification.Id,
                    LeaveTypeId = leaveType.Id,
                    EarnedDate = realization.ActualEndDate,
                    EffectiveStartDate = context.EffectiveStartDate,
                    ExpiryDate = context.ExpiryDate,
                    SourceOvertimeMinutes = realization.VerifiedMinutes,
                    ConversionRate = context.ConversionRate,
                    EarnedMinutes = context.EarnedMinutes,
                    RemainingMinutes = context.EarnedMinutes,
                    CompensatoryStatus = OvertimeValueConstants.CompensatoryStatus.Pending,
                    GeneratedAt = now,
                    ApprovedAt = now,
                    ApprovedByUserId = actorUserId,
                    Notes = NormalizeNullable(request.Notes, 1000),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };
                _dbContext.TrxCompensatoryTimeOffs.Add(credit);
                await _dbContext.SaveChangesAsync(cancellationToken);

                var balance = await GetOrCreateAndLockBalanceAsync(
                    realization.WorkforceProfileId,
                    leaveType.Id,
                    context.EffectiveStartDate,
                    actorUserId,
                    cancellationToken);

                var postedDays = context.EarnedDays;
                var ledger = CreateCreditLedger(
                    balance,
                    credit,
                    postedDays,
                    actorUserId,
                    request.IdempotencyKey);

                ApplyCreditToBalance(balance, ledger, actorUserId);
                credit.LeaveBalanceTransactionId = ledger.Id;
                credit.CompensatoryStatus = OvertimeValueConstants.CompensatoryStatus.Available;
                credit.UpdateDateTime = now;
                credit.UpdateBy = actorUserId;

                _dbContext.TrxLeaveBalanceTransactions.Add(ledger);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return OvertimeCompensatoryLeaveServiceResult<OvertimeCompensatoryLeaveMutationResponse>.Ok(
                    MapMutation(credit, realization, balance, false, false),
                    "Compensatory leave berhasil diposting ke leave balance.",
                    StatusCodes.Status201Created);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<OvertimeCompensatoryLeaveServiceResult<OvertimeCompensatoryLeaveMutationResponse>> ReverseAsync(
            Guid compensatoryTimeOffId,
            ReverseOvertimeCompensatoryLeaveRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
            {
                return FailMutation(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid.");
            }
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return FailMutation(StatusCodes.Status400BadRequest, "Alasan reversal wajib diisi.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
            try
            {
                await AcquireLockAsync("OTC-CREDIT-" + compensatoryTimeOffId, cancellationToken);
                var credit = await _dbContext.TrxCompensatoryTimeOffs
                    .Include(x => x.OvertimeRealization)
                    .Include(x => x.LeaveType)
                    .FirstOrDefaultAsync(x => x.Id == compensatoryTimeOffId && !x.IsDelete && !x.IsCancel, cancellationToken);
                if (credit?.OvertimeRealization == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return FailMutation(StatusCodes.Status404NotFound, "Compensatory leave tidak ditemukan.");
                }

                var periodGuard = await _periodGuard.CheckDateAsync(
                    credit.OvertimeRealization.ActualEndDate,
                    null,
                    credit.OvertimeRealization.HospitalSiteId,
                    null,
                    null,
                    cancellationToken);
                if (!periodGuard.IsWritable)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return FailMutation(StatusCodes.Status409Conflict, periodGuard.Message);
                }

                if (string.Equals(credit.CompensatoryStatus, OvertimeValueConstants.CompensatoryStatus.Cancelled, StringComparison.OrdinalIgnoreCase))
                {
                    var priorBalance = await ResolveBalanceFromCreditAsync(credit, cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return OvertimeCompensatoryLeaveServiceResult<OvertimeCompensatoryLeaveMutationResponse>.Ok(
                        MapMutation(credit, credit.OvertimeRealization, priorBalance, true, true),
                        "Compensatory leave sudah dibatalkan sebelumnya.");
                }

                if (credit.ReservedMinutes > 0 || credit.UsedMinutes > 0 || credit.ExpiredMinutes > 0 || credit.RemainingMinutes != credit.EarnedMinutes)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return FailMutation(StatusCodes.Status409Conflict, "Kredit yang sudah direservasi, digunakan, atau kedaluwarsa tidak dapat direversal dari Overtime.");
                }
                if (credit.OvertimeRealization.IsPayrollPosted ||
                    string.Equals(credit.OvertimeRealization.RealizationStatus, OvertimeValueConstants.RealizationStatus.PostedToPayroll, StringComparison.OrdinalIgnoreCase))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return FailMutation(StatusCodes.Status409Conflict, "Kredit tidak dapat direversal karena realization sudah diposting ke Payroll.");
                }
                if (!credit.LeaveBalanceTransactionId.HasValue)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return FailMutation(StatusCodes.Status409Conflict, "Kredit tidak memiliki referensi leave balance transaction.");
                }

                var sourceLedger = await _dbContext.TrxLeaveBalanceTransactions
                    .FirstOrDefaultAsync(x => x.Id == credit.LeaveBalanceTransactionId.Value && !x.IsDelete, cancellationToken);
                if (sourceLedger == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return FailMutation(StatusCodes.Status409Conflict, "Leave balance transaction sumber tidak ditemukan.");
                }

                var balance = await LockBalanceAsync(sourceLedger.LeaveBalanceId, cancellationToken);
                if (balance == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return FailMutation(StatusCodes.Status409Conflict, "Leave balance sumber tidak ditemukan.");
                }

                var reversalDays = Math.Abs(sourceLedger.TransactionDays);
                if (balance.CompensatoryDays < reversalDays || balance.AvailableDays < reversalDays)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return FailMutation(StatusCodes.Status409Conflict, "Saldo compensatory tidak mencukupi untuk reversal. Pastikan kredit belum digunakan oleh proses Leave.");
                }

                var reversal = CreateReversalLedger(
                    balance,
                    credit,
                    reversalDays,
                    actorUserId,
                    request.Reason,
                    request.IdempotencyKey);
                ApplyReversalToBalance(balance, reversal, actorUserId);

                credit.CompensatoryStatus = OvertimeValueConstants.CompensatoryStatus.Cancelled;
                credit.RemainingMinutes = 0;
                credit.IsActive = false;
                credit.Notes = AppendText(credit.Notes, "[4G Reversal] " + request.Reason, 1000);
                credit.UpdateDateTime = DateTime.UtcNow;
                credit.UpdateBy = actorUserId;

                _dbContext.TrxLeaveBalanceTransactions.Add(reversal);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return OvertimeCompensatoryLeaveServiceResult<OvertimeCompensatoryLeaveMutationResponse>.Ok(
                    MapMutation(credit, credit.OvertimeRealization, balance, false, true),
                    "Compensatory leave berhasil direversal dari leave balance.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<OvertimeCompensatoryLeaveServiceResult<OvertimeCompensatoryLeaveReconciliationResponse>> ReconcileAsync(
            Guid compensatoryTimeOffId,
            ReconcileOvertimeCompensatoryLeaveRequest? request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            request ??= new ReconcileOvertimeCompensatoryLeaveRequest();
            var credit = await _dbContext.TrxCompensatoryTimeOffs
                .Include(x => x.OvertimeRealization)
                .FirstOrDefaultAsync(x => x.Id == compensatoryTimeOffId && !x.IsDelete && !x.IsCancel, cancellationToken);
            if (credit?.OvertimeRealization == null)
            {
                return OvertimeCompensatoryLeaveServiceResult<OvertimeCompensatoryLeaveReconciliationResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Compensatory leave tidak ditemukan.");
            }

            var findings = new List<string>();
            var resolvedLedger = credit.LeaveBalanceTransactionId.HasValue
                ? await _dbContext.TrxLeaveBalanceTransactions.FirstOrDefaultAsync(x => x.Id == credit.LeaveBalanceTransactionId.Value && !x.IsDelete, cancellationToken)
                : null;

            resolvedLedger ??= await _dbContext.TrxLeaveBalanceTransactions
                .OrderByDescending(x => x.TransactionDateTime)
                .FirstOrDefaultAsync(x =>
                    x.SourceReferenceId == credit.Id &&
                    x.SourceType == OvertimeValueConstants.CompensatoryLedger.SourceTypeCredit &&
                    !x.IsDelete,
                    cancellationToken);

            var creditMatches = credit.SourceOvertimeMinutes == credit.OvertimeRealization.VerifiedMinutes;
            if (!creditMatches) findings.Add("Source overtime minutes berbeda dengan verified minutes realization.");
            var ledgerExists = resolvedLedger != null;
            if (!ledgerExists) findings.Add("Leave balance transaction kredit tidak ditemukan.");
            var sourceMatches = resolvedLedger != null &&
                                resolvedLedger.SourceReferenceId == credit.Id &&
                                string.Equals(resolvedLedger.SourceType, OvertimeValueConstants.CompensatoryLedger.SourceTypeCredit, StringComparison.OrdinalIgnoreCase);
            if (ledgerExists && !sourceMatches) findings.Add("Source reference leave balance transaction tidak sesuai.");
            var expectedStatus = resolvedLedger == null
                ? OvertimeValueConstants.CompensatoryStatus.Pending
                : OvertimeValueConstants.CompensatoryStatus.Available;
            var statusMatches = string.Equals(credit.CompensatoryStatus, OvertimeValueConstants.CompensatoryStatus.Cancelled, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(credit.CompensatoryStatus, expectedStatus, StringComparison.OrdinalIgnoreCase) ||
                                credit.UsedMinutes > 0 || credit.ReservedMinutes > 0 || credit.ExpiredMinutes > 0;
            if (!statusMatches) findings.Add("Status compensatory tidak sesuai dengan kondisi ledger.");

            var repaired = false;
            if (request.AllowRepair && actorUserId != Guid.Empty && resolvedLedger != null && !credit.LeaveBalanceTransactionId.HasValue)
            {
                credit.LeaveBalanceTransactionId = resolvedLedger.Id;
                credit.CompensatoryStatus = OvertimeValueConstants.CompensatoryStatus.Available;
                credit.UpdateDateTime = DateTime.UtcNow;
                credit.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);
                repaired = true;
                findings.Add("Link leave balance transaction berhasil diperbaiki.");
            }

            var response = new OvertimeCompensatoryLeaveReconciliationResponse
            {
                CompensatoryTimeOffId = credit.Id,
                CreditNumber = credit.CreditNumber,
                OvertimeRealizationId = credit.OvertimeRealization.Id,
                LeaveBalanceTransactionId = credit.LeaveBalanceTransactionId,
                ResolvedLeaveBalanceTransactionId = resolvedLedger?.Id,
                CreditMatchesRealization = creditMatches,
                LedgerExists = ledgerExists,
                LedgerSourceMatches = sourceMatches,
                StatusMatches = statusMatches,
                IsConsistent = creditMatches && ledgerExists && sourceMatches && statusMatches,
                WasRepaired = repaired,
                Findings = findings
            };

            return OvertimeCompensatoryLeaveServiceResult<OvertimeCompensatoryLeaveReconciliationResponse>.Ok(
                response,
                response.IsConsistent ? "Reconciliation compensatory leave konsisten." : "Reconciliation menemukan perbedaan data.");
        }

        private async Task<PostingContext> BuildPostingContextAsync(
            Guid overtimeRealizationId,
            PreviewOvertimeCompensatoryLeaveRequest request,
            CancellationToken cancellationToken)
        {
            if (request.LeaveTypeId == Guid.Empty) return PostingContext.Fail(StatusCodes.Status400BadRequest, "LeaveTypeId wajib diisi.");
            if (request.ConversionRate <= 0 || request.ConversionRate > 10) return PostingContext.Fail(StatusCodes.Status400BadRequest, "ConversionRate harus lebih dari 0 dan maksimal 10.");
            if (request.MinutesPerDay <= 0 || request.MinutesPerDay > 1440) return PostingContext.Fail(StatusCodes.Status400BadRequest, "MinutesPerDay harus antara 1 sampai 1440.");
            if (request.ExpiryDate.HasValue && request.EffectiveStartDate.HasValue && request.ExpiryDate.Value < request.EffectiveStartDate.Value)
                return PostingContext.Fail(StatusCodes.Status400BadRequest, "ExpiryDate tidak boleh lebih awal dari EffectiveStartDate.");

            var realization = await _dbContext.TrxOvertimeRealizations
                .AsNoTracking()
                .Include(x => x.OvertimeRequest)
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Verifications)
                .FirstOrDefaultAsync(x => x.Id == overtimeRealizationId && !x.IsDelete && !x.IsCancel && x.IsActive, cancellationToken);
            if (realization?.OvertimeRequest == null) return PostingContext.Fail(StatusCodes.Status404NotFound, "Overtime realization tidak ditemukan.");
            var periodGuard = await CheckPeriodAsync(realization.OvertimeRequest, cancellationToken);
            if (!periodGuard.IsWritable) return PostingContext.Fail(StatusCodes.Status409Conflict, periodGuard.Message);
            if (!string.Equals(realization.RealizationStatus, OvertimeValueConstants.RealizationStatus.Verified, StringComparison.OrdinalIgnoreCase) || realization.VerifiedMinutes <= 0)
                return PostingContext.Fail(StatusCodes.Status409Conflict, "Hanya overtime realization final Verified dengan verified minutes lebih dari 0 yang dapat dikonversi.");
            if (realization.IsPayrollPosted || realization.OvertimeRequest.IsPayrollProcessed)
                return PostingContext.Fail(StatusCodes.Status409Conflict, "Overtime realization sudah diproses Payroll.");

            var verification = realization.Verifications
                .Where(x => !x.IsDelete && !x.IsCancel && x.IsActive && x.IsFinalVerification && x.VerificationStatus == OvertimeValueConstants.VerificationStatus.Approved)
                .OrderByDescending(x => x.VerificationOrder)
                .ThenByDescending(x => x.ActionAt)
                .FirstOrDefault();
            if (verification == null) return PostingContext.Fail(StatusCodes.Status409Conflict, "Final approved overtime verification tidak ditemukan.");

            var leaveType = await _dbContext.MstLeaveTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.LeaveTypeId && x.IsActive && !x.IsDelete && !x.IsCancel, cancellationToken);
            if (leaveType == null) return PostingContext.Fail(StatusCodes.Status404NotFound, "Leave type tidak ditemukan atau tidak aktif.");
            if (!string.Equals(leaveType.LeaveCategory, OvertimeValueConstants.CompensatoryLedger.LeaveCategory, StringComparison.OrdinalIgnoreCase))
                return PostingContext.Fail(StatusCodes.Status400BadRequest, "Leave type harus menggunakan LeaveCategory Compensatory.");

            var existing = await _dbContext.TrxCompensatoryTimeOffs
                .AsNoTracking()
                .Include(x => x.LeaveType)
                .FirstOrDefaultAsync(x =>
                    x.OvertimeRealizationId == realization.Id &&
                    x.CompensatoryStatus != OvertimeValueConstants.CompensatoryStatus.Cancelled &&
                    !x.IsDelete && !x.IsCancel,
                    cancellationToken);

            var earnedMinutes = (int)Math.Round(realization.VerifiedMinutes * request.ConversionRate, 0, MidpointRounding.AwayFromZero);
            if (earnedMinutes <= 0) return PostingContext.Fail(StatusCodes.Status400BadRequest, "Hasil konversi compensatory minutes harus lebih dari 0.");
            var earnedDays = Math.Round((decimal)earnedMinutes / request.MinutesPerDay, 2, MidpointRounding.AwayFromZero);
            if (earnedDays <= 0) return PostingContext.Fail(StatusCodes.Status400BadRequest, "Hasil konversi leave days harus lebih dari 0.01 hari.");

            return PostingContext.Ok(
                realization,
                verification,
                leaveType,
                existing,
                request.ConversionRate,
                earnedMinutes,
                request.MinutesPerDay,
                earnedDays,
                request.EffectiveStartDate ?? realization.ActualEndDate,
                request.ExpiryDate);
        }

        private async Task<WfpLeaveBalance> GetOrCreateAndLockBalanceAsync(
            Guid workforceProfileId,
            Guid leaveTypeId,
            DateOnly effectiveDate,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var existing = await _dbContext.WfpLeaveBalances
                .FirstOrDefaultAsync(x => x.WorkforceProfileId == workforceProfileId && x.LeaveTypeId == leaveTypeId && x.Year == effectiveDate.Year && !x.IsDelete, cancellationToken);
            if (existing == null)
            {
                existing = new WfpLeaveBalance
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    LeaveTypeId = leaveTypeId,
                    Year = effectiveDate.Year,
                    PeriodStartDate = new DateOnly(effectiveDate.Year, 1, 1),
                    PeriodEndDate = new DateOnly(effectiveDate.Year, 12, 31),
                    BalanceStatus = OvertimeValueConstants.CompensatoryLedger.Active,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };
                _dbContext.WfpLeaveBalances.Add(existing);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            return await LockBalanceAsync(existing.Id, cancellationToken) ?? existing;
        }

        private async Task<WfpLeaveBalance?> LockBalanceAsync(Guid balanceId, CancellationToken cancellationToken) =>
            await _dbContext.WfpLeaveBalances
                .FromSqlInterpolated($@"SELECT * FROM public.""WfpLeaveBalance"" WHERE ""Id"" = {balanceId} AND ""IsDelete"" = false FOR UPDATE")
                .FirstOrDefaultAsync(cancellationToken);

        private TrxLeaveBalanceTransaction CreateCreditLedger(
            WfpLeaveBalance balance,
            TrxCompensatoryTimeOff credit,
            decimal days,
            Guid actorUserId,
            string? requestedKey) => new()
        {
            Id = Guid.NewGuid(),
            TransactionNumber = GenerateLedgerNumber("CR"),
            LeaveBalanceId = balance.Id,
            WorkforceProfileId = balance.WorkforceProfileId,
            LeaveTypeId = balance.LeaveTypeId,
            LeaveEntitlementPeriodId = balance.LeaveEntitlementPeriodId,
            TransactionDateTime = DateTime.UtcNow,
            EffectiveDate = credit.EffectiveStartDate,
            TransactionSequence = balance.LastTransactionSequence + 1,
            TransactionType = OvertimeValueConstants.CompensatoryLedger.TransactionTypeCredit,
            Direction = OvertimeValueConstants.CompensatoryLedger.DirectionCredit,
            TransactionDays = days,
            AvailableDelta = days,
            PreviousOpeningBalanceDays = balance.OpeningBalanceDays,
            PreviousAvailableDays = balance.AvailableDays,
            PreviousReservedDays = balance.ReservedDays,
            NewAvailableDays = balance.AvailableDays + days,
            NewReservedDays = balance.ReservedDays,
            NewUsedDays = balance.UsedDays,
            IdempotencyKey = NormalizeNullable(requestedKey, 120) ?? "OTC-CREDIT-" + credit.Id,
            PostingBatchType = OvertimeValueConstants.CompensatoryLedger.PostingBatchType,
            PostingBatchId = credit.Id,
            SourceType = OvertimeValueConstants.CompensatoryLedger.SourceTypeCredit,
            SourceReferenceId = credit.Id,
            SourceReferenceNumber = credit.CreditNumber,
            TransactionStatus = OvertimeValueConstants.CompensatoryLedger.Posted,
            PostedAt = DateTime.UtcNow,
            PostedByUserId = actorUserId,
            Remarks = "Compensatory leave dari verified overtime realization.",
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId,
            IsDelete = false,
            IsCancel = false
        };

        private TrxLeaveBalanceTransaction CreateReversalLedger(
            WfpLeaveBalance balance,
            TrxCompensatoryTimeOff credit,
            decimal days,
            Guid actorUserId,
            string reason,
            string? requestedKey) => new()
        {
            Id = Guid.NewGuid(),
            TransactionNumber = GenerateLedgerNumber("RV"),
            LeaveBalanceId = balance.Id,
            WorkforceProfileId = balance.WorkforceProfileId,
            LeaveTypeId = balance.LeaveTypeId,
            LeaveEntitlementPeriodId = balance.LeaveEntitlementPeriodId,
            TransactionDateTime = DateTime.UtcNow,
            EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            TransactionSequence = balance.LastTransactionSequence + 1,
            TransactionType = OvertimeValueConstants.CompensatoryLedger.TransactionTypeReversal,
            Direction = OvertimeValueConstants.CompensatoryLedger.DirectionDebit,
            TransactionDays = -days,
            AvailableDelta = -days,
            PreviousOpeningBalanceDays = balance.OpeningBalanceDays,
            PreviousAvailableDays = balance.AvailableDays,
            PreviousReservedDays = balance.ReservedDays,
            NewAvailableDays = balance.AvailableDays - days,
            NewReservedDays = balance.ReservedDays,
            NewUsedDays = balance.UsedDays,
            IdempotencyKey = NormalizeNullable(requestedKey, 120) ?? "OTC-REVERSAL-" + credit.Id,
            PostingBatchType = OvertimeValueConstants.CompensatoryLedger.PostingBatchType,
            PostingBatchId = credit.Id,
            SourceType = OvertimeValueConstants.CompensatoryLedger.SourceTypeReversal,
            SourceReferenceId = credit.Id,
            SourceReferenceNumber = credit.CreditNumber,
            TransactionStatus = OvertimeValueConstants.CompensatoryLedger.Posted,
            PostedAt = DateTime.UtcNow,
            PostedByUserId = actorUserId,
            Remarks = reason.Trim(),
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId,
            IsDelete = false,
            IsCancel = false
        };

        private static void ApplyCreditToBalance(WfpLeaveBalance balance, TrxLeaveBalanceTransaction ledger, Guid actorUserId)
        {
            balance.CompensatoryDays += ledger.TransactionDays;
            RefreshBalance(balance, ledger, actorUserId);
        }

        private static void ApplyReversalToBalance(WfpLeaveBalance balance, TrxLeaveBalanceTransaction ledger, Guid actorUserId)
        {
            balance.CompensatoryDays = Math.Max(0, balance.CompensatoryDays + ledger.TransactionDays);
            RefreshBalance(balance, ledger, actorUserId);
        }

        private static void RefreshBalance(WfpLeaveBalance balance, TrxLeaveBalanceTransaction ledger, Guid actorUserId)
        {
            balance.RemainingDays = CalculateRemaining(balance);
            balance.AvailableDays = balance.RemainingDays - balance.ReservedDays;
            balance.LastTransactionId = ledger.Id;
            balance.LastTransactionSequence = ledger.TransactionSequence;
            balance.BalanceVersion += 1;
            balance.LastCalculatedAt = DateTime.UtcNow;
            balance.BalanceStatus = OvertimeValueConstants.CompensatoryLedger.Active;
            balance.IsActive = true;
            balance.UpdateDateTime = DateTime.UtcNow;
            balance.UpdateBy = actorUserId;
            ledger.NewAvailableDays = balance.AvailableDays;
            ledger.NewReservedDays = balance.ReservedDays;
            ledger.NewUsedDays = balance.UsedDays;
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

        private async Task<WfpLeaveBalance?> ResolveBalanceFromCreditAsync(
            TrxCompensatoryTimeOff credit,
            CancellationToken cancellationToken)
        {
            if (!credit.LeaveBalanceTransactionId.HasValue) return null;
            var ledger = await _dbContext.TrxLeaveBalanceTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == credit.LeaveBalanceTransactionId.Value && !x.IsDelete, cancellationToken);
            return ledger == null
                ? null
                : await _dbContext.WfpLeaveBalances.AsNoTracking().FirstOrDefaultAsync(x => x.Id == ledger.LeaveBalanceId && !x.IsDelete, cancellationToken);
        }

        private static OvertimeCompensatoryLeavePreviewResponse MapPreview(PostingContext context, bool idempotent) => new()
        {
            OvertimeRealizationId = context.Realization!.Id,
            RealizationNumber = context.Realization.RealizationNumber,
            RealizationVersion = context.Realization.RealizationVersion,
            OvertimeRequestId = context.Realization.OvertimeRequestId,
            RequestNumber = context.Realization.OvertimeRequest?.RequestNumber ?? string.Empty,
            WorkforceProfileId = context.Realization.WorkforceProfileId,
            WorkforceProfileCode = context.Realization.WorkforceProfile?.ProfileCode ?? string.Empty,
            WorkforceDisplayName = context.Realization.WorkforceProfile?.DisplayName ?? string.Empty,
            OvertimeVerificationId = context.Verification?.Id,
            VerifiedMinutes = context.Realization.VerifiedMinutes,
            LeaveTypeId = context.LeaveType!.Id,
            LeaveTypeCode = context.LeaveType.LeaveTypeCode,
            LeaveTypeName = context.LeaveType.LeaveTypeName,
            ConversionRate = context.ConversionRate,
            EarnedMinutes = context.EarnedMinutes,
            MinutesPerDay = context.MinutesPerDay,
            EarnedDays = context.EarnedDays,
            EarnedDate = context.Realization.ActualEndDate,
            EffectiveStartDate = context.EffectiveStartDate,
            ExpiryDate = context.ExpiryDate,
            ExistingCompensatoryTimeOffId = context.ExistingCredit?.Id,
            ExistingCreditNumber = context.ExistingCredit?.CreditNumber,
            ExistingStatus = context.ExistingCredit?.CompensatoryStatus,
            IsIdempotentResult = idempotent,
            CanPost = context.ExistingCredit == null,
            ValidationMessages = context.ExistingCredit == null ? Array.Empty<string>() : new[] { "Realization sudah memiliki compensatory leave aktif." }
        };

        private static OvertimeCompensatoryLeaveMutationResponse MapMutation(
            TrxCompensatoryTimeOff credit,
            TrxOvertimeRealization realization,
            WfpLeaveBalance? balance,
            bool idempotent,
            bool reversed) => new()
        {
            CompensatoryTimeOffId = credit.Id,
            CreditNumber = credit.CreditNumber,
            OvertimeRealizationId = realization.Id,
            RealizationNumber = realization.RealizationNumber,
            OvertimeRequestId = realization.OvertimeRequestId,
            RequestNumber = realization.OvertimeRequest?.RequestNumber ?? string.Empty,
            WorkforceProfileId = credit.WorkforceProfileId,
            LeaveTypeId = credit.LeaveTypeId ?? Guid.Empty,
            LeaveTypeName = credit.LeaveType?.LeaveTypeName ?? string.Empty,
            LeaveBalanceId = balance?.Id,
            LeaveBalanceTransactionId = credit.LeaveBalanceTransactionId,
            CompensatoryStatus = credit.CompensatoryStatus,
            SourceOvertimeMinutes = credit.SourceOvertimeMinutes,
            ConversionRate = credit.ConversionRate,
            EarnedMinutes = credit.EarnedMinutes,
            PostedDays = balance == null ? 0 : Math.Round((decimal)credit.EarnedMinutes / 480m, 2, MidpointRounding.AwayFromZero),
            BalanceCompensatoryDays = balance?.CompensatoryDays ?? 0,
            BalanceAvailableDays = balance?.AvailableDays ?? 0,
            EffectiveStartDate = credit.EffectiveStartDate,
            ExpiryDate = credit.ExpiryDate,
            IsIdempotentResult = idempotent,
            IsReversed = reversed,
            ActionAt = DateTime.UtcNow
        };

        private Task<OvertimePeriodGuardResult> CheckPeriodAsync(
            WfpOvertimeRequest request,
            CancellationToken cancellationToken) =>
            _periodGuard.CheckDateAsync(
                request.OvertimeDate,
                null,
                request.HospitalSiteId,
                request.OrganizationUnitId,
                request.DepartmentId,
                cancellationToken);

        private async Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtext({key}))", cancellationToken);

        private static string GenerateCreditNumber() =>
            $"CTO-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..35].ToUpperInvariant();

        private static string GenerateLedgerNumber(string suffix) =>
            $"LBT-OTC-{suffix}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..48].ToUpperInvariant();

        private static string? NormalizeNullable(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var normalized = value.Trim();
            return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
        }

        private static string? AppendText(string? existing, string addition, int maxLength)
        {
            var combined = string.IsNullOrWhiteSpace(existing) ? addition.Trim() : existing.Trim() + Environment.NewLine + addition.Trim();
            return combined.Length <= maxLength ? combined : combined[^maxLength..];
        }

        private static OvertimeCompensatoryLeaveServiceResult<OvertimeCompensatoryLeaveMutationResponse> FailMutation(int status, string message) =>
            OvertimeCompensatoryLeaveServiceResult<OvertimeCompensatoryLeaveMutationResponse>.Fail(status, message);

        private sealed class PostingContext
        {
            public bool Success { get; private set; }
            public int StatusCode { get; private set; }
            public string Message { get; private set; } = string.Empty;
            public TrxOvertimeRealization? Realization { get; private set; }
            public TrxOvertimeVerification? Verification { get; private set; }
            public MstLeaveType? LeaveType { get; private set; }
            public TrxCompensatoryTimeOff? ExistingCredit { get; private set; }
            public decimal ConversionRate { get; private set; }
            public int EarnedMinutes { get; private set; }
            public int MinutesPerDay { get; private set; }
            public decimal EarnedDays { get; private set; }
            public DateOnly EffectiveStartDate { get; private set; }
            public DateOnly? ExpiryDate { get; private set; }

            public static PostingContext Fail(int statusCode, string message) => new() { Success = false, StatusCode = statusCode, Message = message };
            public static PostingContext Ok(
                TrxOvertimeRealization realization,
                TrxOvertimeVerification verification,
                MstLeaveType leaveType,
                TrxCompensatoryTimeOff? existingCredit,
                decimal conversionRate,
                int earnedMinutes,
                int minutesPerDay,
                decimal earnedDays,
                DateOnly effectiveStartDate,
                DateOnly? expiryDate) => new()
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Realization = realization,
                Verification = verification,
                LeaveType = leaveType,
                ExistingCredit = existingCredit,
                ConversionRate = conversionRate,
                EarnedMinutes = earnedMinutes,
                MinutesPerDay = minutesPerDay,
                EarnedDays = earnedDays,
                EffectiveStartDate = effectiveStartDate,
                ExpiryDate = expiryDate
            };
        }
    }
}
