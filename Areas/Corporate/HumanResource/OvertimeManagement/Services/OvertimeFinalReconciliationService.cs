using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimeFinalReconciliationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly WorkflowReferenceLifecycleService _workflowLifecycleService;
        private readonly OvertimeCompensatoryLeaveService _compensatoryService;
        private readonly OvertimePayrollHandoffService _payrollService;
        private readonly OvertimeSchedulerOptions _options;
        private readonly ILogger<OvertimeFinalReconciliationService> _logger;

        public OvertimeFinalReconciliationService(
            ApplicationDbContext dbContext,
            WorkflowReferenceLifecycleService workflowLifecycleService,
            OvertimeCompensatoryLeaveService compensatoryService,
            OvertimePayrollHandoffService payrollService,
            IOptions<OvertimeSchedulerOptions> options,
            ILogger<OvertimeFinalReconciliationService> logger)
        {
            _dbContext = dbContext;
            _workflowLifecycleService = workflowLifecycleService;
            _compensatoryService = compensatoryService;
            _payrollService = payrollService;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<OvertimeFinalReconciliationResponse> ReconcileAsync(
            OvertimeReconciliationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var first = await EvaluateAsync(request, cancellationToken);
            if (!request.AllowRepair || actorUserId == Guid.Empty)
            {
                return first;
            }

            var attempted = 0;
            var succeeded = 0;
            foreach (var finding in first.Findings.Where(x => x.IsRepairable && x.ReferenceId.HasValue).ToList())
            {
                attempted++;
                try
                {
                    var repaired = await RepairFindingAsync(finding, actorUserId, cancellationToken);
                    finding.WasRepaired = repaired;
                    if (repaired) succeeded++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Safe repair overtime reconciliation gagal. Code={Code}, ReferenceId={ReferenceId}",
                        finding.Code,
                        finding.ReferenceId);
                }
            }

            var final = await EvaluateAsync(request, cancellationToken);
            final.RepairAttempted = attempted;
            final.RepairSucceeded = succeeded;
            return final;
        }

        public string SerializeSnapshot(OvertimeFinalReconciliationResponse response) =>
            JsonSerializer.Serialize(response);

        private async Task<OvertimeFinalReconciliationResponse> EvaluateAsync(
            OvertimeReconciliationRequest request,
            CancellationToken cancellationToken)
        {
            var period = request.OvertimePeriodId.HasValue && request.OvertimePeriodId != Guid.Empty
                ? await _dbContext.Set<TrxOvertimePeriod>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.OvertimePeriodId && !x.IsDelete, cancellationToken)
                : null;

            if (request.OvertimePeriodId.HasValue && request.OvertimePeriodId != Guid.Empty && period == null)
            {
                return new OvertimeFinalReconciliationResponse
                {
                    OvertimePeriodId = request.OvertimePeriodId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    BlockingCount = 1,
                    IsCloseReady = false,
                    EvaluatedAt = DateTime.UtcNow,
                    Findings = new List<OvertimeReconciliationFindingResponse>
                    {
                        new()
                        {
                            Code = "OVERTIME_PERIOD_NOT_FOUND",
                            Severity = "Error",
                            Message = "Overtime period tidak ditemukan.",
                            ReferenceType = "OvertimePeriod",
                            ReferenceId = request.OvertimePeriodId,
                            IsBlocking = true,
                            IsRepairable = false
                        }
                    }
                };
            }

            if (period != null)
            {
                request.StartDate = period.StartDate;
                request.EndDate = period.EndDate;
                request.LegalEntityId = period.LegalEntityId;
                request.HospitalSiteId = period.HospitalSiteId;
                request.OrganizationUnitId = period.OrganizationUnitId;
                request.DepartmentId = period.DepartmentId;
            }

            var requireAttendanceFinal = period?.RequireAttendanceFinal ?? true;
            var requireVerificationComplete = period?.RequireVerificationComplete ?? true;
            var requireSettlementComplete = period?.RequireSettlementComplete ?? true;
            var utcNow = DateTime.UtcNow;
            var localToday = DateOnly.FromDateTime(
                TimeZoneInfo.ConvertTimeFromUtc(utcNow, ResolveTimeZone(_options.TimeZoneId)));
            var overdueThreshold = utcNow.AddHours(-Math.Clamp(request.VerificationOverdueHours, 1, 720));

            var query = _dbContext.WfpOvertimeRequests
                .AsNoTracking()
                .Include(x => x.Realizations.Where(r => !r.IsDelete && !r.IsCancel && r.IsActive))
                    .ThenInclude(x => x.Verifications.Where(v => !v.IsDelete && !v.IsCancel && v.IsActive))
                .Include(x => x.Realizations.Where(r => !r.IsDelete && !r.IsCancel && r.IsActive))
                    .ThenInclude(x => x.CompensatoryTimeOffs.Where(c => !c.IsDelete && !c.IsCancel))
                .Where(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.OvertimeDate >= request.StartDate &&
                    x.OvertimeDate <= request.EndDate);

            query = ApplyScope(query, request);
            var requests = await query.ToListAsync(cancellationToken);
            var requestIds = requests.Select(x => x.Id).ToList();
            var realizationIds = requests.SelectMany(x => x.Realizations).Select(x => x.Id).Distinct().ToList();
            var workflowIds = requests.Where(x => x.WorkflowInstanceId.HasValue).Select(x => x.WorkflowInstanceId!.Value).Distinct().ToList();
            var creditIds = requests.SelectMany(x => x.Realizations).SelectMany(x => x.CompensatoryTimeOffs).Select(x => x.Id).Distinct().ToList();

            var workflows = workflowIds.Count == 0
                ? new Dictionary<Guid, TrxWorkflowInstance>()
                : await _dbContext.TrxWorkflowInstances
                    .AsNoTracking()
                    .Where(x => workflowIds.Contains(x.Id) && !x.IsDelete)
                    .ToDictionaryAsync(x => x.Id, cancellationToken);

            var payrollInputs = realizationIds.Count == 0
                ? new List<PayrollInputRow>()
                : await _dbContext.TrxPayrollOvertimeInputs
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && !x.IsCancel &&
                        x.OvertimeRealizationId.HasValue &&
                        realizationIds.Contains(x.OvertimeRealizationId.Value))
                    .Select(x => new PayrollInputRow
                    {
                        Id = x.Id,
                        OvertimeRealizationId = x.OvertimeRealizationId!.Value,
                        VerifiedMinutes = x.VerifiedMinutes
                    })
                    .ToListAsync(cancellationToken);

            var ledgers = creditIds.Count == 0
                ? new List<CompensatoryLedgerRow>()
                : await _dbContext.TrxLeaveBalanceTransactions
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.SourceReferenceId.HasValue && creditIds.Contains(x.SourceReferenceId.Value))
                    .Select(x => new CompensatoryLedgerRow
                    {
                        Id = x.Id,
                        SourceReferenceId = x.SourceReferenceId!.Value,
                        SourceType = x.SourceType,
                        TransactionStatus = x.TransactionStatus
                    })
                    .ToListAsync(cancellationToken);

            var attendanceRows = requireAttendanceFinal && requestIds.Count > 0
                ? await _dbContext.TrxAttendanceDailies
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.IsActive &&
                        x.WorkforceProfileId.HasValue &&
                        x.AttendanceDate >= request.StartDate &&
                        x.AttendanceDate <= request.EndDate)
                    .Select(x => new AttendanceRow
                    {
                        WorkforceProfileId = x.WorkforceProfileId!.Value,
                        AttendanceDate = x.AttendanceDate,
                        ProcessingStatus = x.ProcessingStatus,
                        ProcessingVersion = x.ProcessingVersion
                    })
                    .ToListAsync(cancellationToken)
                : new List<AttendanceRow>();

            var openPlansQuery = _dbContext.TrxOvertimePlans
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.PlanStartDate <= request.EndDate &&
                    x.PlanEndDate >= request.StartDate &&
                    x.PlanStatus != OvertimeValueConstants.PlanStatus.Closed &&
                    x.PlanStatus != OvertimeValueConstants.PlanStatus.Cancelled &&
                    x.PlanStatus != OvertimeValueConstants.PlanStatus.Converted);
            if (request.LegalEntityId.HasValue && request.LegalEntityId != Guid.Empty) openPlansQuery = openPlansQuery.Where(x => x.LegalEntityId == request.LegalEntityId);
            if (request.HospitalSiteId.HasValue && request.HospitalSiteId != Guid.Empty) openPlansQuery = openPlansQuery.Where(x => x.HospitalSiteId == request.HospitalSiteId);
            if (request.OrganizationUnitId.HasValue && request.OrganizationUnitId != Guid.Empty) openPlansQuery = openPlansQuery.Where(x => x.OrganizationUnitId == request.OrganizationUnitId);
            if (request.DepartmentId.HasValue && request.DepartmentId != Guid.Empty) openPlansQuery = openPlansQuery.Where(x => x.DepartmentId == request.DepartmentId);
            var openPlans = await openPlansQuery
                .Select(x => new { x.Id, x.PlanNumber, x.PlanStatus })
                .ToListAsync(cancellationToken);

            var response = new OvertimeFinalReconciliationResponse
            {
                OvertimePeriodId = period?.Id,
                PeriodCode = period?.PeriodCode,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalRequest = requests.Count,
                OpenPlan = openPlans.Count,
                EvaluatedAt = DateTime.UtcNow
            };

            foreach (var plan in openPlans)
            {
                AddFinding(response, "OPEN_OVERTIME_PLAN", "Error",
                    $"Overtime plan masih berstatus {plan.PlanStatus} dan belum diselesaikan.",
                    "OvertimePlan", plan.Id, plan.PlanNumber, true, false);
            }

            foreach (var overtimeRequest in requests)
            {
                if (overtimeRequest.OvertimeRequestStatus == OvertimeValueConstants.RequestStatus.Draft)
                {
                    response.DraftRequest++;
                    AddFinding(response, "DRAFT_REQUEST", "Error",
                        "Overtime request masih Draft dan belum disubmit atau dibatalkan.",
                        "OvertimeRequest", overtimeRequest.Id, overtimeRequest.RequestNumber, true, false);
                }

                EvaluateWorkflow(overtimeRequest, workflows, response);
                var activeRealization = overtimeRequest.Realizations
                    .OrderByDescending(x => x.RealizationVersion)
                    .FirstOrDefault();

                if (IsPendingRealization(overtimeRequest.OvertimeRequestStatus) && activeRealization == null)
                {
                    response.ApprovedPendingRealization++;
                    AddFinding(response, "REALIZATION_PENDING", "Error",
                        "Overtime request sudah disetujui tetapi belum mempunyai realization aktif.",
                        "OvertimeRequest", overtimeRequest.Id, overtimeRequest.RequestNumber, true, false);

                    if (requireAttendanceFinal)
                    {
                        var attendance = attendanceRows
                            .Where(x => x.WorkforceProfileId == overtimeRequest.WorkforceProfileId && x.AttendanceDate == overtimeRequest.OvertimeDate)
                            .OrderByDescending(x => x.ProcessingVersion)
                            .FirstOrDefault();
                        if (attendance == null || !string.Equals(attendance.ProcessingStatus, AttendanceValueConstants.AttendanceProcessingStatus.Processed, StringComparison.OrdinalIgnoreCase))
                        {
                            response.AttendanceNotFinal++;
                            AddFinding(response, "ATTENDANCE_NOT_FINAL", "Error",
                                "Attendance daily belum final untuk request yang menunggu realization.",
                                "OvertimeRequest", overtimeRequest.Id, overtimeRequest.RequestNumber, true, false);
                        }
                    }
                }

                if (activeRealization == null) continue;

                if (activeRealization.RealizationStatus == OvertimeValueConstants.RealizationStatus.WaitingVerification)
                {
                    response.WaitingVerification++;
                    var pendingVerification = activeRealization.Verifications
                        .Where(x => x.VerificationStatus == OvertimeValueConstants.VerificationStatus.Pending)
                        .OrderByDescending(x => x.CreateDateTime)
                        .FirstOrDefault();
                    if ((pendingVerification?.CreateDateTime ?? activeRealization.SubmittedAt ?? activeRealization.CreateDateTime) <= overdueThreshold)
                    {
                        response.VerificationOverdue++;
                        AddFinding(response, "VERIFICATION_OVERDUE", "Warning",
                            "Overtime realization menunggu verifikasi melebihi batas monitoring.",
                            "OvertimeRealization", activeRealization.Id, activeRealization.RealizationNumber, false, false);
                    }
                    if (requireVerificationComplete)
                    {
                        AddFinding(response, "VERIFICATION_PENDING", "Error",
                            "Overtime realization masih menunggu final verification.",
                            "OvertimeRealization", activeRealization.Id, activeRealization.RealizationNumber, true, false);
                    }
                }

                if (activeRealization.RealizationStatus == OvertimeValueConstants.RealizationStatus.NeedRevision)
                {
                    response.NeedRevision++;
                    AddFinding(response, "REALIZATION_NEED_REVISION", "Error",
                        "Overtime realization masih memerlukan revisi atau recalculation.",
                        "OvertimeRealization", activeRealization.Id, activeRealization.RealizationNumber, true, false);
                }

                EvaluateSettlement(
                    overtimeRequest,
                    activeRealization,
                    payrollInputs,
                    ledgers,
                    requireSettlementComplete,
                    localToday,
                    response);
            }

            response.OpenWorkflowRequest = response.Findings.Count(x => x.Code == "WORKFLOW_OPEN" || x.Code == "WORKFLOW_MISSING");
            response.BlockingCount = response.Findings.Count(x => x.IsBlocking);
            response.WarningCount = response.Findings.Count(x => !x.IsBlocking);
            response.IsCloseReady = response.BlockingCount == 0;
            return response;
        }

        private async Task<bool> RepairFindingAsync(
            OvertimeReconciliationFindingResponse finding,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            if (!finding.ReferenceId.HasValue) return false;

            if (finding.Code == "WORKFLOW_STATUS_MISMATCH")
            {
                var request = await _dbContext.WfpOvertimeRequests
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == finding.ReferenceId.Value && x.WorkflowInstanceId.HasValue, cancellationToken);
                if (request?.WorkflowInstanceId == null) return false;
                var result = await _workflowLifecycleService.SynchronizeAsync(
                    request.WorkflowInstanceId.Value,
                    actorUserId,
                    true,
                    cancellationToken);
                return result.IsHandled;
            }

            if (finding.Code == "PAYROLL_MARKER_MISMATCH")
            {
                var result = await _payrollService.ReconcileAsync(
                    finding.ReferenceId.Value,
                    new ReconcileOvertimePayrollHandoffRequest { AllowRepair = true },
                    actorUserId,
                    cancellationToken);
                return result.Success && result.Data?.IsConsistent == true;
            }

            if (finding.Code == "COMPENSATORY_LEDGER_LINK_MISMATCH")
            {
                var result = await _compensatoryService.ReconcileAsync(
                    finding.ReferenceId.Value,
                    new ReconcileOvertimeCompensatoryLeaveRequest { AllowRepair = true },
                    actorUserId,
                    cancellationToken);
                return result.Success && result.Data?.IsConsistent == true;
            }

            return false;
        }

        private static void EvaluateWorkflow(
            WfpOvertimeRequest request,
            IReadOnlyDictionary<Guid, TrxWorkflowInstance> workflows,
            OvertimeFinalReconciliationResponse response)
        {
            if (!request.WorkflowInstanceId.HasValue)
            {
                if (request.OvertimeRequestStatus == OvertimeValueConstants.RequestStatus.Submitted ||
                    request.OvertimeRequestStatus == OvertimeValueConstants.RequestStatus.NeedRevision)
                {
                    AddFinding(response, "WORKFLOW_MISSING", "Error",
                        "Overtime request membutuhkan workflow tetapi WorkflowInstanceId belum tersedia.",
                        "OvertimeRequest", request.Id, request.RequestNumber, true, false);
                }
                return;
            }

            if (!workflows.TryGetValue(request.WorkflowInstanceId.Value, out var workflow))
            {
                AddFinding(response, "WORKFLOW_NOT_FOUND", "Error",
                    "Workflow instance pada overtime request tidak ditemukan.",
                    "OvertimeRequest", request.Id, request.RequestNumber, true, false);
                return;
            }

            if (IsOpenWorkflow(workflow.WorkflowStatus))
            {
                AddFinding(response, "WORKFLOW_OPEN", "Error",
                    $"Workflow overtime masih berstatus {workflow.WorkflowStatus}.",
                    "OvertimeRequest", request.Id, request.RequestNumber, true, false);
            }

            if (!IsWorkflowAligned(workflow.WorkflowStatus, request.OvertimeRequestStatus))
            {
                response.WorkflowLifecycleIssue++;
                AddFinding(response, "WORKFLOW_STATUS_MISMATCH", "Warning",
                    $"Status workflow {workflow.WorkflowStatus} belum selaras dengan status request {request.OvertimeRequestStatus}.",
                    "OvertimeRequest", request.Id, request.RequestNumber, false, true);
            }
        }

        private static void EvaluateSettlement(
            WfpOvertimeRequest request,
            TrxOvertimeRealization realization,
            IReadOnlyCollection<PayrollInputRow> payrollInputs,
            IReadOnlyCollection<CompensatoryLedgerRow> ledgers,
            bool requireSettlementComplete,
            DateOnly localToday,
            OvertimeFinalReconciliationResponse response)
        {
            var payrollInput = payrollInputs.FirstOrDefault(x => x.OvertimeRealizationId == realization.Id);
            var activeCredits = realization.CompensatoryTimeOffs
                .Where(x => x.IsActive && !x.IsDelete && !x.IsCancel &&
                    x.CompensatoryStatus != OvertimeValueConstants.CompensatoryStatus.Cancelled)
                .ToList();
            var activeCredit = activeCredits.FirstOrDefault();

            if (payrollInput != null && activeCredit != null)
            {
                AddFinding(response, "DOUBLE_BENEFIT", "Error",
                    "Satu realization mempunyai payroll input dan compensatory leave aktif sekaligus.",
                    "OvertimeRealization", realization.Id, realization.RealizationNumber, true, false);
            }

            if (payrollInput != null)
            {
                response.PostedToPayroll++;
                var markerMatches = realization.IsPayrollPosted &&
                    realization.RealizationStatus == OvertimeValueConstants.RealizationStatus.PostedToPayroll &&
                    realization.PostedMinutes == payrollInput.VerifiedMinutes &&
                    request.IsPayrollProcessed;
                if (!markerMatches)
                {
                    response.PayrollReconciliationIssue++;
                    AddFinding(response, "PAYROLL_MARKER_MISMATCH", "Warning",
                        "Payroll input tersedia tetapi marker source Overtime belum konsisten.",
                        "OvertimeRealization", realization.Id, realization.RealizationNumber, false, true);
                }
            }
            else if (realization.IsPayrollPosted || realization.RealizationStatus == OvertimeValueConstants.RealizationStatus.PostedToPayroll)
            {
                response.PayrollReconciliationIssue++;
                AddFinding(response, "PAYROLL_INPUT_MISSING", "Error",
                    "Realization ditandai PostedToPayroll tetapi payroll overtime input tidak ditemukan.",
                    "OvertimeRealization", realization.Id, realization.RealizationNumber, true, false);
            }

            if (activeCredit != null)
            {
                response.CompensatoryLeave++;
                var ledger = ledgers.FirstOrDefault(x => x.SourceReferenceId == activeCredit.Id &&
                    x.SourceType == OvertimeValueConstants.CompensatoryLedger.SourceTypeCredit);
                if (activeCredit.LeaveBalanceTransactionId == null || ledger == null || ledger.Id != activeCredit.LeaveBalanceTransactionId)
                {
                    response.CompensatoryReconciliationIssue++;
                    AddFinding(response, "COMPENSATORY_LEDGER_LINK_MISMATCH", "Warning",
                        "Compensatory credit belum mempunyai link ledger yang konsisten.",
                        "CompensatoryTimeOff", activeCredit.Id, activeCredit.CreditNumber, false, true);
                }

                if (activeCredit.ExpiryDate.HasValue &&
                    activeCredit.ExpiryDate.Value < localToday &&
                    activeCredit.RemainingMinutes > 0 &&
                    activeCredit.CompensatoryStatus != OvertimeValueConstants.CompensatoryStatus.Expired)
                {
                    response.ExpiredCompensatoryPending++;
                    AddFinding(response, "COMPENSATORY_EXPIRY_PENDING", "Error",
                        "Compensatory credit sudah melewati expiry date tetapi remaining minutes belum dieksekusi expiry.",
                        "CompensatoryTimeOff", activeCredit.Id, activeCredit.CreditNumber, true, false);
                }
            }

            var settled = payrollInput != null || activeCredit != null;
            if (realization.RealizationStatus == OvertimeValueConstants.RealizationStatus.Verified && !settled)
            {
                response.VerifiedAwaitingSettlement++;
                AddFinding(response, "SETTLEMENT_PENDING", requireSettlementComplete ? "Error" : "Warning",
                    "Verified overtime belum diarahkan ke Payroll atau Compensatory Leave.",
                    "OvertimeRealization", realization.Id, realization.RealizationNumber, requireSettlementComplete, false);
            }
        }

        private static IQueryable<WfpOvertimeRequest> ApplyScope(
            IQueryable<WfpOvertimeRequest> query,
            OvertimeReconciliationRequest request)
        {
            if (request.HospitalSiteId.HasValue && request.HospitalSiteId != Guid.Empty) query = query.Where(x => x.HospitalSiteId == request.HospitalSiteId);
            if (request.OrganizationUnitId.HasValue && request.OrganizationUnitId != Guid.Empty) query = query.Where(x => x.OrganizationUnitId == request.OrganizationUnitId);
            if (request.DepartmentId.HasValue && request.DepartmentId != Guid.Empty) query = query.Where(x => x.DepartmentId == request.DepartmentId);
            if (request.LegalEntityId.HasValue && request.LegalEntityId != Guid.Empty)
            {
                query = query.Where(x => x.HospitalSite != null && x.HospitalSite.LegalEntityId == request.LegalEntityId);
            }
            return query;
        }

        private static bool IsPendingRealization(string status) =>
            status == OvertimeValueConstants.RequestStatus.ApprovedForWork ||
            status == OvertimeValueConstants.RequestStatus.InProgress ||
            status == OvertimeValueConstants.RequestStatus.WaitingRealization;

        private static bool IsOpenWorkflow(string status) =>
            status == WorkflowValueConstants.WorkflowStatus.Draft ||
            status == WorkflowValueConstants.WorkflowStatus.Submitted ||
            status == WorkflowValueConstants.WorkflowStatus.InProgress ||
            status == WorkflowValueConstants.WorkflowStatus.RevisionRequested ||
            status == WorkflowValueConstants.WorkflowStatus.Returned;

        private static bool IsWorkflowAligned(string workflowStatus, string requestStatus)
        {
            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Approved ||
                workflowStatus == WorkflowValueConstants.WorkflowStatus.Completed)
            {
                return requestStatus == OvertimeValueConstants.RequestStatus.ApprovedForWork ||
                       requestStatus == OvertimeValueConstants.RequestStatus.InProgress ||
                       requestStatus == OvertimeValueConstants.RequestStatus.WaitingRealization ||
                       requestStatus == OvertimeValueConstants.RequestStatus.WaitingVerification ||
                       requestStatus == OvertimeValueConstants.RequestStatus.Realized ||
                       requestStatus == OvertimeValueConstants.RequestStatus.PostedToPayroll;
            }
            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Rejected)
                return requestStatus == OvertimeValueConstants.RequestStatus.Rejected;
            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Cancelled || workflowStatus == WorkflowValueConstants.WorkflowStatus.Withdrawn)
                return requestStatus == OvertimeValueConstants.RequestStatus.Cancelled;
            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.RevisionRequested || workflowStatus == WorkflowValueConstants.WorkflowStatus.Returned)
                return requestStatus == OvertimeValueConstants.RequestStatus.NeedRevision;
            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Submitted || workflowStatus == WorkflowValueConstants.WorkflowStatus.InProgress)
                return requestStatus == OvertimeValueConstants.RequestStatus.Submitted;
            return true;
        }

        private static TimeZoneInfo ResolveTimeZone(string? id)
        {
            var candidates = new List<string>();
            if (!string.IsNullOrWhiteSpace(id)) candidates.Add(id.Trim());
            candidates.Add("Asia/Jakarta");
            candidates.Add("SE Asia Standard Time");
            foreach (var candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(candidate); }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }
            return TimeZoneInfo.Utc;
        }

        private static void AddFinding(
            OvertimeFinalReconciliationResponse response,
            string code,
            string severity,
            string message,
            string referenceType,
            Guid referenceId,
            string referenceNumber,
            bool blocking,
            bool repairable) =>
            response.Findings.Add(new OvertimeReconciliationFindingResponse
            {
                Code = code,
                Severity = severity,
                Message = message,
                ReferenceType = referenceType,
                ReferenceId = referenceId,
                ReferenceNumber = referenceNumber,
                IsBlocking = blocking,
                IsRepairable = repairable
            });

        private sealed class PayrollInputRow
        {
            public Guid Id { get; set; }
            public Guid OvertimeRealizationId { get; set; }
            public int VerifiedMinutes { get; set; }
        }

        private sealed class CompensatoryLedgerRow
        {
            public Guid Id { get; set; }
            public Guid SourceReferenceId { get; set; }
            public string SourceType { get; set; } = string.Empty;
            public string TransactionStatus { get; set; } = string.Empty;
        }

        private sealed class AttendanceRow
        {
            public Guid WorkforceProfileId { get; set; }
            public DateOnly AttendanceDate { get; set; }
            public string ProcessingStatus { get; set; } = string.Empty;
            public int ProcessingVersion { get; set; }
        }
    }
}
