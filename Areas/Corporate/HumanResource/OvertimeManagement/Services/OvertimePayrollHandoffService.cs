using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Data;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimePayrollHandoffService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly OvertimePeriodGuardService _periodGuard;

        public OvertimePayrollHandoffService(
            ApplicationDbContext dbContext,
            OvertimePeriodGuardService periodGuard)
        {
            _dbContext = dbContext;
            _periodGuard = periodGuard;
        }

        public async Task<OvertimePayrollHandoffServiceResult<OvertimePayrollHandoffPreviewResponse>> PreviewAsync(
            Guid overtimeRealizationId,
            PreviewOvertimePayrollHandoffRequest? request,
            CancellationToken cancellationToken = default)
        {
            request ??= new PreviewOvertimePayrollHandoffRequest();
            var context = await BuildContextAsync(overtimeRealizationId, request, cancellationToken);
            if (context.Realization == null)
            {
                return OvertimePayrollHandoffServiceResult<OvertimePayrollHandoffPreviewResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Overtime realization tidak ditemukan.");
            }

            return OvertimePayrollHandoffServiceResult<OvertimePayrollHandoffPreviewResponse>.Ok(
                MapPreview(context),
                context.Issues.Any(x => x.Severity == "Error")
                    ? "Preview selesai, tetapi handoff belum siap diposting."
                    : context.ExistingInput == null
                        ? "Preview overtime payroll handoff berhasil."
                        : "Overtime realization sudah memiliki payroll input.");
        }

        public async Task<OvertimePayrollHandoffServiceResult<OvertimePayrollHandoffMutationResponse>> PostAsync(
            Guid overtimeRealizationId,
            PostOvertimePayrollHandoffRequest? request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
                return FailMutation(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid.");

            request ??= new PostOvertimePayrollHandoffRequest();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
            try
            {
                await AcquireLockAsync("OTP-REALIZATION-" + overtimeRealizationId, cancellationToken);
                var context = await BuildContextAsync(overtimeRealizationId, request, cancellationToken, tracking: true);
                if (context.Realization == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return FailMutation(StatusCodes.Status404NotFound, "Overtime realization tidak ditemukan.");
                }

                if (context.ExistingInput != null)
                {
                    EnsureSourceMarkedPosted(context, actorUserId, context.ExistingInput.ImportedAt);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return OvertimePayrollHandoffServiceResult<OvertimePayrollHandoffMutationResponse>.Ok(
                        MapMutation(context, context.ExistingInput, true, false),
                        "Payroll overtime input sudah pernah dibuat untuk realization tersebut.");
                }

                var blocking = context.Issues.FirstOrDefault(x => x.Severity == "Error");
                if (blocking != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return FailMutation(StatusCodes.Status409Conflict, blocking.Message);
                }

                var input = CreatePayrollInput(context, request, actorUserId);
                _dbContext.TrxPayrollOvertimeInputs.Add(input);
                EnsureSourceMarkedPosted(context, actorUserId, input.ImportedAt);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return OvertimePayrollHandoffServiceResult<OvertimePayrollHandoffMutationResponse>.Ok(
                    MapMutation(context, input, false, false),
                    "Verified overtime berhasil diposting sebagai payroll overtime input.",
                    StatusCodes.Status201Created);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<OvertimePayrollHandoffServiceResult<OvertimePayrollHandoffReconciliationResponse>> ReconcileAsync(
            Guid overtimeRealizationId,
            ReconcileOvertimePayrollHandoffRequest? request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
            {
                return OvertimePayrollHandoffServiceResult<OvertimePayrollHandoffReconciliationResponse>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid.");
            }

            request ??= new ReconcileOvertimePayrollHandoffRequest();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
            try
            {
                await AcquireLockAsync("OTP-RECONCILE-" + overtimeRealizationId, cancellationToken);
                var realization = await LoadRealizationAsync(overtimeRealizationId, tracking: true, cancellationToken);
                if (realization == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return OvertimePayrollHandoffServiceResult<OvertimePayrollHandoffReconciliationResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Overtime realization tidak ditemukan.");
                }

                if (request.AllowRepair && realization.OvertimeRequest != null)
                {
                    var periodGuard = await CheckPeriodAsync(realization.OvertimeRequest, cancellationToken);
                    if (!periodGuard.IsWritable)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return OvertimePayrollHandoffServiceResult<OvertimePayrollHandoffReconciliationResponse>.Fail(
                            StatusCodes.Status409Conflict,
                            periodGuard.Message);
                    }
                }

                var input = await _dbContext.TrxPayrollOvertimeInputs
                    .FirstOrDefaultAsync(x =>
                        !x.IsDelete &&
                        !x.IsCancel &&
                        x.OvertimeRealizationId == realization.Id,
                        cancellationToken);

                var issues = BuildReconciliationIssues(realization, input);
                var repaired = false;

                if (request.AllowRepair && input != null && issues.Count > 0)
                {
                    EnsureSourceMarkedPosted(new HandoffContext
                    {
                        Realization = realization,
                        Request = realization.OvertimeRequest,
                        ExistingInput = input
                    }, actorUserId, input.ImportedAt);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    repaired = true;
                    issues = BuildReconciliationIssues(realization, input);
                }
                else if (request.AllowRepair && input == null && request.PayrollRunId.HasValue)
                {
                    var context = await BuildContextAsync(
                        overtimeRealizationId,
                        new PreviewOvertimePayrollHandoffRequest
                        {
                            PayrollRunId = request.PayrollRunId.Value,
                            PayrollComponentId = request.PayrollComponentId,
                            Notes = "Repair reconciliation overtime payroll handoff."
                        },
                        cancellationToken,
                        tracking: true);
                    var blocking = context.Issues.FirstOrDefault(x => x.Severity == "Error");
                    if (blocking == null)
                    {
                        input = CreatePayrollInput(context, new PostOvertimePayrollHandoffRequest
                        {
                            PayrollRunId = request.PayrollRunId.Value,
                            PayrollComponentId = request.PayrollComponentId,
                            Notes = "Repair reconciliation overtime payroll handoff.",
                            IdempotencyKey = request.IdempotencyKey
                        }, actorUserId);
                        _dbContext.TrxPayrollOvertimeInputs.Add(input);
                        EnsureSourceMarkedPosted(context, actorUserId, input.ImportedAt);
                        await _dbContext.SaveChangesAsync(cancellationToken);
                        repaired = true;
                        issues = BuildReconciliationIssues(realization, input);
                    }
                    else
                    {
                        issues.Add(blocking);
                    }
                }

                await transaction.CommitAsync(cancellationToken);
                var runEmployee = input == null
                    ? null
                    : await _dbContext.TrxPayrollRunEmployees.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == input.PayrollRunEmployeeId, cancellationToken);

                return OvertimePayrollHandoffServiceResult<OvertimePayrollHandoffReconciliationResponse>.Ok(
                    new OvertimePayrollHandoffReconciliationResponse
                    {
                        OvertimeRealizationId = realization.Id,
                        PayrollOvertimeInputId = input?.Id,
                        PayrollRunEmployeeId = input?.PayrollRunEmployeeId,
                        PayrollRunId = runEmployee?.PayrollRunId,
                        IsConsistent = issues.Count == 0,
                        WasRepaired = repaired,
                        Issues = issues
                    },
                    issues.Count == 0
                        ? repaired ? "Reconciliation berhasil dan data telah diperbaiki." : "Reconciliation berhasil, data konsisten."
                        : "Reconciliation selesai dan masih menemukan ketidaksesuaian.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<OvertimePayrollHandoffServiceResult<OvertimePayrollHandoffMutationResponse>> RollbackAsync(
            Guid overtimeRealizationId,
            RollbackOvertimePayrollHandoffRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
                return FailMutation(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid.");
            if (string.IsNullOrWhiteSpace(request.Reason))
                return FailMutation(StatusCodes.Status400BadRequest, "Alasan rollback wajib diisi.");

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
            try
            {
                await AcquireLockAsync("OTP-ROLLBACK-" + overtimeRealizationId, cancellationToken);
                var realization = await LoadRealizationAsync(overtimeRealizationId, tracking: true, cancellationToken);
                if (realization?.OvertimeRequest == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return FailMutation(StatusCodes.Status404NotFound, "Overtime realization tidak ditemukan.");
                }

                var periodGuard = await CheckPeriodAsync(realization.OvertimeRequest, cancellationToken);
                if (!periodGuard.IsWritable)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return FailMutation(StatusCodes.Status409Conflict, periodGuard.Message);
                }

                var input = await _dbContext.TrxPayrollOvertimeInputs
                    .FirstOrDefaultAsync(x =>
                        !x.IsDelete &&
                        !x.IsCancel &&
                        x.OvertimeRealizationId == overtimeRealizationId,
                        cancellationToken);
                if (input == null)
                {
                    ResetSourceToVerified(realization, actorUserId);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return OvertimePayrollHandoffServiceResult<OvertimePayrollHandoffMutationResponse>.Ok(
                        MapRollbackMutation(realization, null),
                        "Payroll input tidak ditemukan; marker Overtime telah dikembalikan ke Verified.");
                }

                var runEmployee = await _dbContext.TrxPayrollRunEmployees
                    .FirstOrDefaultAsync(x => x.Id == input.PayrollRunEmployeeId && !x.IsDelete, cancellationToken);
                var run = runEmployee == null
                    ? null
                    : await _dbContext.TrxPayrollRuns
                        .FirstOrDefaultAsync(x => x.Id == runEmployee.PayrollRunId && !x.IsDelete, cancellationToken);
                if (run == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return FailMutation(StatusCodes.Status409Conflict, "Payroll run untuk input tersebut tidak ditemukan.");
                }
                if (run.IsLocked || OvertimeValueConstants.PayrollRunStatus.Blocked.Contains(run.RunStatus, StringComparer.OrdinalIgnoreCase))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return FailMutation(StatusCodes.Status409Conflict, "Payroll run sudah dikunci atau telah melewati tahap yang mengizinkan rollback.");
                }
                if (runEmployee != null && GetBooleanProperty(runEmployee, "IsFinalized"))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return FailMutation(StatusCodes.Status409Conflict, "Payroll employee snapshot sudah final dan tidak dapat di-rollback.");
                }

                var hasGeneratedComponent = await _dbContext.TrxPayrollEmployeeComponents
                    .AsNoTracking()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        EF.Property<Guid?>(x, "SourceId") == input.Id &&
                        EF.Property<string>(x, "SourceType") == OvertimeValueConstants.PayrollHandoff.SourceType,
                        cancellationToken);
                if (hasGeneratedComponent)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return FailMutation(StatusCodes.Status409Conflict, "Payroll overtime input sudah digunakan menjadi payroll employee component.");
                }

                var now = DateTime.UtcNow;
                input.IsDelete = true;
                input.IsActive = false;
                input.DeleteDateTime = now;
                input.DeleteBy = actorUserId;
                input.UpdateDateTime = now;
                input.UpdateBy = actorUserId;
                input.Notes = AppendText(input.Notes, "Rollback: " + request.Reason.Trim(), 1000);
                ResetSourceToVerified(realization, actorUserId);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return OvertimePayrollHandoffServiceResult<OvertimePayrollHandoffMutationResponse>.Ok(
                    MapRollbackMutation(realization, input),
                    "Overtime payroll handoff berhasil di-rollback.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task<HandoffContext> BuildContextAsync(
            Guid overtimeRealizationId,
            PreviewOvertimePayrollHandoffRequest request,
            CancellationToken cancellationToken,
            bool tracking = false)
        {
            var context = new HandoffContext();
            context.Realization = await LoadRealizationAsync(overtimeRealizationId, tracking, cancellationToken);
            if (context.Realization == null) return context;
            context.Request = context.Realization.OvertimeRequest;

            if (context.Request != null)
            {
                var periodGuard = await CheckPeriodAsync(context.Request, cancellationToken);
                if (!periodGuard.IsWritable)
                    AddIssue(context, "OVERTIME_PERIOD_CLOSED", periodGuard.Message);
            }

            context.ExistingInput = await _dbContext.TrxPayrollOvertimeInputs
                .FirstOrDefaultAsync(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.OvertimeRealizationId == overtimeRealizationId,
                    cancellationToken);

            context.HasCompensatoryLeave = context.Realization.CompensatoryTimeOffs.Any(IsActiveCompensatoryCredit);
            if (context.HasCompensatoryLeave)
                AddIssue(context, "COMPENSATORY_LEAVE_EXISTS", "Realization sudah dikonversi menjadi compensatory leave dan tidak boleh dibayar tunai melalui Payroll.");
            if (!string.Equals(context.Realization.RealizationStatus, OvertimeValueConstants.RealizationStatus.Verified, StringComparison.OrdinalIgnoreCase) && context.ExistingInput == null)
                AddIssue(context, "REALIZATION_NOT_VERIFIED", "Hanya overtime realization berstatus Verified yang dapat diposting ke Payroll.");
            if (context.Realization.VerifiedMinutes <= 0)
                AddIssue(context, "VERIFIED_MINUTES_EMPTY", "Verified minutes harus lebih dari nol.");

            var finalVerification = context.Realization.Verifications
                .Where(x => x.IsActive && !x.IsDelete && !x.IsCancel)
                .OrderByDescending(x => x.ActionAt ?? x.UpdateDateTime ?? x.CreateDateTime)
                .FirstOrDefault();
            if (finalVerification == null || !string.Equals(finalVerification.VerificationStatus, OvertimeValueConstants.VerificationStatus.Approved, StringComparison.OrdinalIgnoreCase))
                AddIssue(context, "FINAL_VERIFICATION_REQUIRED", "Final verification berstatus Approved belum tersedia.");

            if (request.PayrollRunId == Guid.Empty)
            {
                AddIssue(context, "PAYROLL_RUN_REQUIRED", "PayrollRunId wajib dipilih.");
                return context;
            }

            context.PayrollRun = await _dbContext.TrxPayrollRuns
                .FirstOrDefaultAsync(x => x.Id == request.PayrollRunId && !x.IsDelete && !x.IsCancel, cancellationToken);
            if (context.PayrollRun == null)
            {
                AddIssue(context, "PAYROLL_RUN_NOT_FOUND", "Payroll run tidak ditemukan.");
                return context;
            }
            if (!context.PayrollRun.IsActive)
                AddIssue(context, "PAYROLL_RUN_INACTIVE", "Payroll run tidak aktif.");
            if (context.PayrollRun.IsLocked)
                AddIssue(context, "PAYROLL_RUN_LOCKED", "Payroll run sudah dikunci.");
            if (OvertimeValueConstants.PayrollRunStatus.Blocked.Contains(context.PayrollRun.RunStatus, StringComparer.OrdinalIgnoreCase))
                AddIssue(context, "PAYROLL_RUN_STATUS_BLOCKED", $"Payroll run berstatus {context.PayrollRun.RunStatus} tidak menerima input baru.");

            context.PayrollPeriod = await _dbContext.MstPayrollPeriods
                .FirstOrDefaultAsync(x => x.Id == context.PayrollRun.PayrollPeriodId && !x.IsDelete && !x.IsCancel, cancellationToken);
            if (context.PayrollPeriod == null)
            {
                AddIssue(context, "PAYROLL_PERIOD_NOT_FOUND", "Payroll period pada payroll run tidak ditemukan.");
                return context;
            }
            if (!context.PayrollPeriod.IsActive || context.PayrollPeriod.IsLocked)
                AddIssue(context, "PAYROLL_PERIOD_LOCKED", "Payroll period tidak aktif atau sudah dikunci.");
            if (OvertimeValueConstants.PayrollPeriodStatus.Blocked.Contains(context.PayrollPeriod.PayrollPeriodStatus, StringComparer.OrdinalIgnoreCase))
                AddIssue(context, "PAYROLL_PERIOD_STATUS_BLOCKED", $"Payroll period berstatus {context.PayrollPeriod.PayrollPeriodStatus} tidak menerima input baru.");

            var overtimeDate = context.Realization.ActualEndDate;
            var periodStart = DateOnly.FromDateTime(context.PayrollPeriod.StartDate);
            var periodEnd = DateOnly.FromDateTime(context.PayrollPeriod.EndDate);
            if (overtimeDate < periodStart || overtimeDate > periodEnd)
                AddIssue(context, "OVERTIME_OUTSIDE_PERIOD", "Tanggal overtime berada di luar payroll period.");
            if (context.PayrollPeriod.LegalEntityId.HasValue && context.Realization.OvertimeRequest?.HospitalSite?.LegalEntityId != null &&
                context.PayrollPeriod.LegalEntityId != context.Realization.OvertimeRequest.HospitalSite.LegalEntityId)
                AddIssue(context, "LEGAL_ENTITY_MISMATCH", "Legal entity payroll period tidak sesuai dengan sumber Overtime.");
            if (context.PayrollPeriod.HospitalSiteId.HasValue && context.Realization.HospitalSiteId.HasValue &&
                context.PayrollPeriod.HospitalSiteId != context.Realization.HospitalSiteId)
                AddIssue(context, "HOSPITAL_SITE_MISMATCH", "Hospital site payroll period tidak sesuai dengan sumber Overtime.");

            context.PayrollRunEmployee = await _dbContext.TrxPayrollRunEmployees
                .FirstOrDefaultAsync(x =>
                    x.PayrollRunId == context.PayrollRun.Id &&
                    x.WorkforceProfileId == context.Realization.WorkforceProfileId &&
                    !x.IsDelete &&
                    !x.IsCancel,
                    cancellationToken);
            if (context.PayrollRunEmployee == null)
                AddIssue(context, "PAYROLL_RUN_EMPLOYEE_NOT_FOUND", "Employee belum tersedia pada payroll run snapshot.");
            else
            {
                if (GetBooleanProperty(context.PayrollRunEmployee, "IsFinalized"))
                    AddIssue(context, "PAYROLL_EMPLOYEE_FINALIZED", "Payroll employee snapshot sudah final.");
                if (OvertimeValueConstants.PayrollEmployeeStatus.Blocked.Contains(context.PayrollRunEmployee.EmployeePayrollStatus, StringComparer.OrdinalIgnoreCase))
                    AddIssue(context, "PAYROLL_EMPLOYEE_STATUS_BLOCKED", $"Employee payroll berstatus {context.PayrollRunEmployee.EmployeePayrollStatus} tidak menerima input baru.");
            }

            var componentId = request.PayrollComponentId ?? context.Realization.PayrollComponentId ?? context.Request?.PayrollComponentId;
            if (componentId.HasValue && componentId != Guid.Empty)
            {
                context.PayrollComponent = await _dbContext.MstPayrollComponents
                    .FirstOrDefaultAsync(x => x.Id == componentId && !x.IsDelete && !x.IsCancel, cancellationToken);
            }
            else
            {
                var effectiveDate = context.Realization.ActualEndAt ?? DateTime.UtcNow;
                context.PayrollComponent = await _dbContext.MstPayrollComponents
                    .Where(x =>
                        !x.IsDelete &&
                        !x.IsCancel &&
                        x.IsActive &&
                        x.IsOvertimeBased &&
                        (x.EffectiveStartDate == null || x.EffectiveStartDate <= effectiveDate) &&
                        (x.EffectiveEndDate == null || x.EffectiveEndDate >= effectiveDate))
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.PayrollComponentCode)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            if (context.PayrollComponent == null)
                AddIssue(context, "PAYROLL_COMPONENT_NOT_FOUND", "Payroll component khusus Overtime tidak ditemukan.");
            else if (!context.PayrollComponent.IsActive ||
                     (!context.PayrollComponent.IsOvertimeBased && !string.Equals(context.PayrollComponent.CalculationMethod, "Overtime", StringComparison.OrdinalIgnoreCase)))
                AddIssue(context, "PAYROLL_COMPONENT_INVALID", "Payroll component yang dipilih bukan komponen Overtime aktif.");

            return context;
        }

        private async Task<TrxOvertimeRealization?> LoadRealizationAsync(
            Guid id,
            bool tracking,
            CancellationToken cancellationToken)
        {
            IQueryable<TrxOvertimeRealization> query = _dbContext.TrxOvertimeRealizations
                .Include(x => x.OvertimeRequest)!
                    .ThenInclude(x => x!.HospitalSite)
                .Include(x => x.Details)
                .Include(x => x.Verifications)
                .Include(x => x.CompensatoryTimeOffs)
                .Where(x => x.Id == id && !x.IsDelete && !x.IsCancel && x.IsActive);
            if (!tracking) query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        private TrxPayrollOvertimeInput CreatePayrollInput(
            HandoffContext context,
            PostOvertimePayrollHandoffRequest request,
            Guid actorUserId)
        {
            var realization = context.Realization!;
            var now = DateTime.UtcNow;
            var snapshots = BuildRateSnapshots(realization);
            var verifiedTotal = snapshots.Sum(x => x.VerifiedMinutes);
            var weightedMultiplier = verifiedTotal <= 0
                ? 0m
                : snapshots.Sum(x => x.RateMultiplier * x.VerifiedMinutes) / verifiedTotal;
            var weightedHourlyRate = verifiedTotal <= 0
                ? 0m
                : snapshots.Sum(x => x.HourlyRateSnapshot * x.VerifiedMinutes) / verifiedTotal;

            var calculationSnapshot = JsonSerializer.Serialize(new
            {
                Source = "OvertimeManagement",
                SourceVersion = "4H",
                request.IdempotencyKey,
                realization.Id,
                realization.RealizationNumber,
                realization.RealizationVersion,
                realization.OvertimeRequestId,
                realization.WorkforceProfileId,
                realization.ActualStartAt,
                realization.ActualEndAt,
                realization.RequestedMinutesSnapshot,
                realization.ApprovedMinutesSnapshot,
                realization.ActualMinutes,
                realization.EligibleMinutes,
                realization.VerifiedMinutes,
                WeightedRateMultiplier = weightedMultiplier,
                HourlyRateSnapshot = weightedHourlyRate,
                MonetaryCalculationOwner = "Payroll",
                RateSnapshots = snapshots
            });

            return new TrxPayrollOvertimeInput
            {
                Id = Guid.NewGuid(),
                PayrollRunEmployeeId = context.PayrollRunEmployee!.Id,
                OvertimeRealizationId = realization.Id,
                OvertimeRequestId = realization.OvertimeRequestId,
                OvertimeDate = realization.ActualEndDate,
                OvertimeStatusSnapshot = OvertimeValueConstants.RealizationStatus.Verified,
                RequestedMinutes = realization.RequestedMinutesSnapshot,
                ApprovedMinutes = realization.ApprovedMinutesSnapshot,
                ActualMinutes = realization.ActualMinutes,
                EligibleMinutes = realization.EligibleMinutes,
                VerifiedMinutes = realization.VerifiedMinutes,
                RateMultiplier = Math.Round(weightedMultiplier, 4),
                HourlyRate = Math.Round(weightedHourlyRate, 2),
                OvertimeAmount = 0m,
                CalculationSnapshotJson = calculationSnapshot,
                ImportedAt = now,
                ImportedByUserId = actorUserId,
                Notes = NormalizeNullable(request.Notes, 1000),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                UpdateBy = Guid.Empty,
                DeleteBy = Guid.Empty,
                CancelBy = Guid.Empty,
                IsDelete = false,
                IsCancel = false
            };
        }

        private static void EnsureSourceMarkedPosted(HandoffContext context, Guid actorUserId, DateTime postedAt)
        {
            var realization = context.Realization!;
            var request = context.Request ?? realization.OvertimeRequest;
            var input = context.ExistingInput;
            var now = DateTime.UtcNow;
            realization.IsPayrollPosted = true;
            realization.PayrollPeriodId = context.PayrollPeriod?.Id ?? realization.PayrollPeriodId;
            realization.PayrollComponentId = context.PayrollComponent?.Id ?? realization.PayrollComponentId;
            realization.PostedMinutes = input?.VerifiedMinutes ?? realization.VerifiedMinutes;
            realization.PostedAmount = input?.OvertimeAmount ?? 0m;
            realization.PostedToPayrollAt = postedAt;
            realization.PostedToPayrollByUserId = actorUserId;
            realization.RealizationStatus = OvertimeValueConstants.RealizationStatus.PostedToPayroll;
            realization.UpdateDateTime = now;
            realization.UpdateBy = actorUserId;

            foreach (var detail in realization.Details.Where(x => x.IsActive && !x.IsDelete && !x.IsCancel))
            {
                detail.DetailStatus = OvertimeValueConstants.RealizationDetailStatus.Posted;
                detail.UpdateDateTime = now;
                detail.UpdateBy = actorUserId;
            }

            if (request != null)
            {
                request.IsPayrollProcessed = true;
                request.PayrollPeriodId = context.PayrollPeriod?.Id ?? request.PayrollPeriodId;
                request.PayrollComponentId = context.PayrollComponent?.Id ?? request.PayrollComponentId;
                request.PayrollProcessedAt = postedAt;
                request.ProcessedAt = postedAt;
                request.ProcessedByUserId = actorUserId;
                request.OvertimeRequestStatus = OvertimeValueConstants.RequestStatus.PostedToPayroll;
                request.UpdateDateTime = now;
                request.UpdateBy = actorUserId;
            }
        }

        private static void ResetSourceToVerified(TrxOvertimeRealization realization, Guid actorUserId)
        {
            var now = DateTime.UtcNow;
            realization.IsPayrollPosted = false;
            realization.PayrollPeriodId = null;
            realization.PayrollComponentId = null;
            realization.PostedMinutes = 0;
            realization.PostedAmount = 0m;
            realization.PostedToPayrollAt = null;
            realization.PostedToPayrollByUserId = null;
            realization.RealizationStatus = OvertimeValueConstants.RealizationStatus.Verified;
            realization.UpdateDateTime = now;
            realization.UpdateBy = actorUserId;

            foreach (var detail in realization.Details.Where(x => x.IsActive && !x.IsDelete && !x.IsCancel))
            {
                detail.DetailStatus = OvertimeValueConstants.RealizationDetailStatus.Verified;
                detail.UpdateDateTime = now;
                detail.UpdateBy = actorUserId;
            }

            if (realization.OvertimeRequest != null)
            {
                realization.OvertimeRequest.IsPayrollProcessed = false;
                realization.OvertimeRequest.PayrollPeriodId = null;
                realization.OvertimeRequest.PayrollComponentId = null;
                realization.OvertimeRequest.PayrollProcessedAt = null;
                realization.OvertimeRequest.ProcessedAt = null;
                realization.OvertimeRequest.ProcessedByUserId = null;
                realization.OvertimeRequest.OvertimeRequestStatus = OvertimeValueConstants.RequestStatus.Realized;
                realization.OvertimeRequest.UpdateDateTime = now;
                realization.OvertimeRequest.UpdateBy = actorUserId;
            }
        }

        private static List<OvertimePayrollReadinessIssueResponse> BuildReconciliationIssues(
            TrxOvertimeRealization realization,
            TrxPayrollOvertimeInput? input)
        {
            var issues = new List<OvertimePayrollReadinessIssueResponse>();
            if (input == null)
            {
                issues.Add(Issue("PAYROLL_INPUT_MISSING", "Payroll overtime input tidak ditemukan."));
                if (!realization.IsPayrollPosted && realization.RealizationStatus == OvertimeValueConstants.RealizationStatus.Verified)
                    issues.Clear();
                return issues;
            }
            if (input.OvertimeRequestId != realization.OvertimeRequestId)
                issues.Add(Issue("REQUEST_REFERENCE_MISMATCH", "Overtime request reference pada payroll input tidak sesuai."));
            if (input.VerifiedMinutes != realization.VerifiedMinutes)
                issues.Add(Issue("VERIFIED_MINUTES_MISMATCH", "Verified minutes payroll input tidak sama dengan realization."));
            if (!realization.IsPayrollPosted)
                issues.Add(Issue("SOURCE_NOT_MARKED_POSTED", "Realization belum ditandai sudah diposting Payroll."));
            if (realization.PostedMinutes != input.VerifiedMinutes)
                issues.Add(Issue("POSTED_MINUTES_MISMATCH", "Posted minutes realization tidak sama dengan payroll input."));
            if (!string.Equals(realization.RealizationStatus, OvertimeValueConstants.RealizationStatus.PostedToPayroll, StringComparison.OrdinalIgnoreCase))
                issues.Add(Issue("REALIZATION_STATUS_MISMATCH", "Status realization belum PostedToPayroll."));
            if (realization.OvertimeRequest != null && !realization.OvertimeRequest.IsPayrollProcessed)
                issues.Add(Issue("REQUEST_NOT_MARKED_PROCESSED", "Overtime request belum ditandai payroll processed."));
            return issues;
        }

        private static OvertimePayrollHandoffPreviewResponse MapPreview(HandoffContext context)
        {
            var realization = context.Realization!;
            var snapshots = BuildRateSnapshots(realization);
            var verifiedTotal = snapshots.Sum(x => x.VerifiedMinutes);
            var weightedMultiplier = verifiedTotal <= 0 ? 0m : snapshots.Sum(x => x.RateMultiplier * x.VerifiedMinutes) / verifiedTotal;
            var weightedHourly = verifiedTotal <= 0 ? 0m : snapshots.Sum(x => x.HourlyRateSnapshot * x.VerifiedMinutes) / verifiedTotal;
            return new OvertimePayrollHandoffPreviewResponse
            {
                OvertimeRealizationId = realization.Id,
                RealizationNumber = realization.RealizationNumber,
                OvertimeRequestId = realization.OvertimeRequestId,
                RequestNumber = context.Request?.RequestNumber ?? string.Empty,
                WorkforceProfileId = realization.WorkforceProfileId,
                OvertimeDate = realization.ActualEndDate,
                VerifiedMinutes = realization.VerifiedMinutes,
                PayrollRunId = context.PayrollRun?.Id ?? Guid.Empty,
                PayrollRunNumber = context.PayrollRun?.RunNumber ?? string.Empty,
                PayrollRunStatus = context.PayrollRun?.RunStatus ?? string.Empty,
                PayrollRunEmployeeId = context.PayrollRunEmployee?.Id ?? Guid.Empty,
                PayrollPeriodId = context.PayrollPeriod?.Id ?? Guid.Empty,
                PayrollPeriodCode = context.PayrollPeriod?.PayrollPeriodCode ?? string.Empty,
                PayrollComponentId = context.PayrollComponent?.Id ?? Guid.Empty,
                PayrollComponentCode = context.PayrollComponent?.PayrollComponentCode ?? string.Empty,
                PayrollComponentName = context.PayrollComponent?.PayrollComponentName ?? string.Empty,
                WeightedRateMultiplier = Math.Round(weightedMultiplier, 4),
                HourlyRateSnapshot = Math.Round(weightedHourly, 2),
                HasCompensatoryLeave = context.HasCompensatoryLeave,
                HasExistingPayrollInput = context.ExistingInput != null,
                ExistingPayrollInputId = context.ExistingInput?.Id,
                CanPost = context.ExistingInput != null || !context.Issues.Any(x => x.Severity == "Error"),
                Issues = context.Issues,
                RateSnapshots = snapshots
            };
        }

        private static OvertimePayrollHandoffMutationResponse MapMutation(
            HandoffContext context,
            TrxPayrollOvertimeInput input,
            bool existing,
            bool rolledBack) => new()
        {
            OvertimeRealizationId = context.Realization!.Id,
            RealizationNumber = context.Realization.RealizationNumber,
            OvertimeRequestId = context.Realization.OvertimeRequestId,
            RequestNumber = context.Request?.RequestNumber ?? string.Empty,
            PayrollRunId = context.PayrollRun?.Id ?? Guid.Empty,
            PayrollRunEmployeeId = input.PayrollRunEmployeeId,
            PayrollPeriodId = context.PayrollPeriod?.Id ?? context.Realization.PayrollPeriodId ?? Guid.Empty,
            PayrollComponentId = context.PayrollComponent?.Id ?? context.Realization.PayrollComponentId ?? Guid.Empty,
            PayrollOvertimeInputId = input.Id,
            PostedMinutes = context.Realization.PostedMinutes,
            PostedAmount = context.Realization.PostedAmount,
            RealizationStatus = context.Realization.RealizationStatus,
            RequestStatus = context.Request?.OvertimeRequestStatus ?? string.Empty,
            IsExisting = existing,
            IsRolledBack = rolledBack,
            ProcessedAt = input.ImportedAt
        };

        private static OvertimePayrollHandoffMutationResponse MapRollbackMutation(
            TrxOvertimeRealization realization,
            TrxPayrollOvertimeInput? input) => new()
        {
            OvertimeRealizationId = realization.Id,
            RealizationNumber = realization.RealizationNumber,
            OvertimeRequestId = realization.OvertimeRequestId,
            RequestNumber = realization.OvertimeRequest?.RequestNumber ?? string.Empty,
            PayrollRunEmployeeId = input?.PayrollRunEmployeeId ?? Guid.Empty,
            PayrollOvertimeInputId = input?.Id,
            PostedMinutes = realization.PostedMinutes,
            PostedAmount = realization.PostedAmount,
            RealizationStatus = realization.RealizationStatus,
            RequestStatus = realization.OvertimeRequest?.OvertimeRequestStatus ?? string.Empty,
            IsExisting = input != null,
            IsRolledBack = true,
            ProcessedAt = DateTime.UtcNow
        };

        private static List<OvertimePayrollRateSnapshotResponse> BuildRateSnapshots(TrxOvertimeRealization realization) =>
            realization.Details
                .Where(x => x.IsActive && !x.IsDelete && !x.IsCancel && x.VerifiedMinutes > 0)
                .OrderBy(x => x.SequenceNumber)
                .Select(x => new OvertimePayrollRateSnapshotResponse
                {
                    OvertimeRateId = x.OvertimeRateId,
                    OvertimeDate = x.OvertimeDate,
                    DayType = x.DayType,
                    RateBand = x.RateBandSnapshot,
                    VerifiedMinutes = x.VerifiedMinutes,
                    RateMultiplier = x.RateMultiplierSnapshot,
                    HourlyRateSnapshot = x.BaseHourlyRateSnapshot
                }).ToList();

        private static bool IsActiveCompensatoryCredit(TrxCompensatoryTimeOff credit) =>
            credit.IsActive &&
            !credit.IsDelete &&
            !credit.IsCancel &&
            !string.Equals(credit.CompensatoryStatus, OvertimeValueConstants.CompensatoryStatus.Cancelled, StringComparison.OrdinalIgnoreCase);

        private static void AddIssue(HandoffContext context, string code, string message, string severity = "Error") =>
            context.Issues.Add(Issue(code, message, severity));

        private static OvertimePayrollReadinessIssueResponse Issue(string code, string message, string severity = "Error") => new()
        {
            Code = code,
            Message = message,
            Severity = severity
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

        private static bool GetBooleanProperty(object value, string propertyName)
        {
            var property = value.GetType().GetProperty(propertyName);
            return property?.PropertyType == typeof(bool) && property.GetValue(value) is true;
        }

        private static string? NormalizeNullable(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var normalized = value.Trim();
            return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
        }

        private static string AppendText(string? existing, string addition, int maxLength)
        {
            var combined = string.IsNullOrWhiteSpace(existing)
                ? addition.Trim()
                : existing.Trim() + Environment.NewLine + addition.Trim();
            return combined.Length <= maxLength ? combined : combined[^maxLength..];
        }

        private static OvertimePayrollHandoffServiceResult<OvertimePayrollHandoffMutationResponse> FailMutation(
            int statusCode,
            string message) => OvertimePayrollHandoffServiceResult<OvertimePayrollHandoffMutationResponse>.Fail(statusCode, message);

        private sealed class HandoffContext
        {
            public TrxOvertimeRealization? Realization { get; set; }
            public WfpOvertimeRequest? Request { get; set; }
            public TrxPayrollRun? PayrollRun { get; set; }
            public TrxPayrollRunEmployee? PayrollRunEmployee { get; set; }
            public MstPayrollPeriod? PayrollPeriod { get; set; }
            public MstPayrollComponent? PayrollComponent { get; set; }
            public TrxPayrollOvertimeInput? ExistingInput { get; set; }
            public bool HasCompensatoryLeave { get; set; }
            public List<OvertimePayrollReadinessIssueResponse> Issues { get; set; } = new();
        }
    }
}
