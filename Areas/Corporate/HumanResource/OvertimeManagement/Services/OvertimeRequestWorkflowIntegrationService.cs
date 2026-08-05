using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    /// <summary>
    /// Adapter Overtime Request ke Generic Workflow Engine.
    /// Service ini tidak menyimpan approval action pada tabel Overtime khusus.
    /// </summary>
    public class OvertimeRequestWorkflowIntegrationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly WorkflowService _workflowService;
        private readonly WorkflowReferenceLifecycleService _lifecycleService;
        private readonly OvertimePeriodGuardService _periodGuard;
        private readonly ILogger<OvertimeRequestWorkflowIntegrationService> _logger;

        public OvertimeRequestWorkflowIntegrationService(
            ApplicationDbContext dbContext,
            WorkflowService workflowService,
            WorkflowReferenceLifecycleService lifecycleService,
            OvertimePeriodGuardService periodGuard,
            ILogger<OvertimeRequestWorkflowIntegrationService> logger)
        {
            _dbContext = dbContext;
            _workflowService = workflowService;
            _lifecycleService = lifecycleService;
            _periodGuard = periodGuard;
            _logger = logger;
        }

        public async Task<OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse>>
            StartOrResubmitAsync(
                Guid overtimeRequestId,
                StartOvertimeWorkflowRequest? request,
                CancellationToken cancellationToken = default)
        {
            if (overtimeRequestId == Guid.Empty)
            {
                return Fail("Overtime request id tidak valid.");
            }

            request ??= new StartOvertimeWorkflowRequest();

            var overtimeRequest = await _dbContext.WfpOvertimeRequests
                .AsNoTracking()
                .Include(x => x.OvertimePolicy)
                .Include(x => x.OrganizationAssignment)
                .FirstOrDefaultAsync(
                    x => x.Id == overtimeRequestId && !x.IsDelete,
                    cancellationToken);

            if (overtimeRequest == null)
            {
                return OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Overtime request tidak ditemukan.");
            }

            var periodGuard = await CheckPeriodAsync(overtimeRequest, cancellationToken);
            if (!periodGuard.IsWritable)
                return Conflict(periodGuard.Message);

            if (overtimeRequest.IsCancel ||
                string.Equals(
                    overtimeRequest.OvertimeRequestStatus,
                    OvertimeValueConstants.RequestStatus.Cancelled,
                    StringComparison.OrdinalIgnoreCase))
            {
                return Conflict("Overtime request yang sudah dibatalkan tidak dapat diproses ke workflow.");
            }

            if (string.Equals(
                    overtimeRequest.OvertimeRequestStatus,
                    OvertimeValueConstants.RequestStatus.Rejected,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    overtimeRequest.OvertimeRequestStatus,
                    OvertimeValueConstants.RequestStatus.WaitingRealization,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    overtimeRequest.OvertimeRequestStatus,
                    OvertimeValueConstants.RequestStatus.WaitingVerification,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    overtimeRequest.OvertimeRequestStatus,
                    OvertimeValueConstants.RequestStatus.Realized,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    overtimeRequest.OvertimeRequestStatus,
                    OvertimeValueConstants.RequestStatus.PostedToPayroll,
                    StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(
                    $"Overtime request berstatus {overtimeRequest.OvertimeRequestStatus} tidak dapat disubmit ulang.");
            }

            if (overtimeRequest.OvertimePolicy == null)
            {
                return OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Overtime policy pada request tidak ditemukan.");
            }

            var previousRequestStatus = overtimeRequest.OvertimeRequestStatus;

            if (!overtimeRequest.OvertimePolicy.RequirePreApproval)
            {
                var autoApproved = await AutoApproveWithoutWorkflowAsync(
                    overtimeRequest.Id,
                    cancellationToken);

                return OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse>.Ok(
                    new OvertimeWorkflowIntegrationResponse
                    {
                        OvertimeRequestId = autoApproved.Id,
                        RequestNumber = autoApproved.RequestNumber,
                        PreviousRequestStatus = previousRequestStatus,
                        CurrentRequestStatus = autoApproved.OvertimeRequestStatus,
                        WorkflowDefinitionId = null,
                        WorkflowInstanceId = null,
                        WorkflowRequestNumber = null,
                        WorkflowStatus = "NotRequired",
                        CurrentStepOrder = 0,
                        WorkflowCreated = false,
                        WorkflowSubmitted = false,
                        LifecycleSynchronized = true,
                        ActionAt = DateTime.UtcNow
                    },
                    "Policy tidak mewajibkan pre-approval. Overtime request langsung berstatus ApprovedForWork.");
            }

            var workflowCode = overtimeRequest.OvertimePolicy.ApprovalWorkflowCode?.Trim();
            if (string.IsNullOrWhiteSpace(workflowCode))
            {
                return OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Overtime policy mewajibkan pre-approval tetapi belum memiliki ApprovalWorkflowCode.");
            }
            var workflowCreated = false;
            var workflowSubmitted = false;
            WorkflowInstanceDetailResponse? workflowDetail = null;

            var existingWorkflowId = overtimeRequest.WorkflowInstanceId;
            if (!existingWorkflowId.HasValue)
            {
                existingWorkflowId = await _dbContext.TrxWorkflowInstances
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.ReferenceType == OvertimeValueConstants.Workflow.ReferenceType &&
                        x.ReferenceId == overtimeRequest.Id)
                    .OrderByDescending(x => x.CreateDateTime)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (existingWorkflowId.HasValue)
            {
                var existingResult = await _workflowService.GetByIdAsync(
                    existingWorkflowId.Value,
                    cancellationToken);

                if (!existingResult.Success || existingResult.Data == null)
                {
                    return OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse>.Fail(
                        existingResult.StatusCode,
                        existingResult.Message);
                }

                workflowDetail = existingResult.Data;
            }
            else
            {
                var createKey = NormalizeIdempotencyKey(
                    request.IdempotencyKey,
                    $"OTR-WF-{overtimeRequest.Id:N}");

                var employeeContext = await _dbContext.MstEmployees
                    .AsNoTracking()
                    .Where(x =>
                        x.WorkforceProfileId == overtimeRequest.WorkforceProfileId &&
                        !x.IsDelete &&
                        !x.IsCancel)
                    .Select(x => new
                    {
                        x.EmployeeCategoryId,
                        x.EmploymentTypeId
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                var requestContext = JsonSerializer.SerializeToElement(new
                {
                    durationHours = Math.Round(overtimeRequest.RequestedMinutes / 60m, 2),
                    requestedHours = Math.Round(overtimeRequest.RequestedMinutes / 60m, 2),
                    totalHours = Math.Round(overtimeRequest.RequestedMinutes / 60m, 2),
                    requestedMinutes = overtimeRequest.RequestedMinutes,
                    employeeCategoryId = employeeContext?.EmployeeCategoryId,
                    employmentTypeId = employeeContext?.EmploymentTypeId,
                    overtimePolicyId = overtimeRequest.OvertimePolicyId,
                    workforceProfileId = overtimeRequest.WorkforceProfileId,
                    employeeId = overtimeRequest.EmployeeId,
                    organizationAssignmentId = overtimeRequest.OrganizationAssignmentId,
                    legalEntityId = overtimeRequest.OrganizationAssignment?.LegalEntityId,
                    hospitalSiteId = overtimeRequest.HospitalSiteId,
                    organizationUnitId = overtimeRequest.OrganizationUnitId,
                    departmentId = overtimeRequest.DepartmentId,
                    costCenterId = overtimeRequest.CostCenterId,
                    overtimeDate = overtimeRequest.OvertimeDate.ToString("yyyy-MM-dd"),
                    plannedStartAt = overtimeRequest.PlannedStartAt,
                    plannedEndAt = overtimeRequest.PlannedEndAt,
                    requestSource = overtimeRequest.RequestSource,
                    isUrgent = overtimeRequest.IsUrgent,
                    isRestDay = overtimeRequest.IsRestDay,
                    isHoliday = overtimeRequest.IsHoliday
                });

                var createResult = await _workflowService.CreateAsync(
                    new CreateWorkflowInstanceRequest
                    {
                        WorkflowDefinitionCode = workflowCode,
                        ReferenceType = OvertimeValueConstants.Workflow.ReferenceType,
                        ReferenceId = overtimeRequest.Id,
                        ExternalReferenceNumber = overtimeRequest.RequestNumber,
                        SourceChannel = NormalizeSourceChannel(request.SourceChannel),
                        RequestCorrelationId = $"OVERTIME:{overtimeRequest.RequestNumber}",
                        IdempotencyKey = createKey,
                        RequestContext = requestContext
                    },
                    cancellationToken);

                if (!createResult.Success || createResult.Data == null)
                {
                    return OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse>.Fail(
                        createResult.StatusCode,
                        createResult.Message);
                }

                workflowCreated = true;
                workflowDetail = createResult.Data;
            }

            if (CanSubmitWorkflow(workflowDetail.WorkflowStatus))
            {
                var submitKey = NormalizeIdempotencyKey(
                    request.IdempotencyKey,
                    $"OTR-WF-SUBMIT-{overtimeRequest.Id:N}",
                    "-SUBMIT");

                var submitResult = await _workflowService.SubmitAsync(
                    workflowDetail.Id,
                    new WorkflowSubmitRequest
                    {
                        Comment = NormalizeOptionalText(request.Comment),
                        IdempotencyKey = submitKey
                    },
                    cancellationToken);

                if (!submitResult.Success || submitResult.Data == null)
                {
                    return OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse>.Fail(
                        submitResult.StatusCode,
                        submitResult.Message);
                }

                workflowSubmitted = true;
                workflowDetail = submitResult.Data;
            }
            else if (!IsWorkflowAlreadyProcessed(workflowDetail.WorkflowStatus))
            {
                return Conflict(
                    $"Workflow berstatus {workflowDetail.WorkflowStatus} tidak dapat disubmit atau dipulihkan.");
            }

            var synchronized = await _lifecycleService.SynchronizeAsync(
                workflowDetail.Id,
                workflowDetail.Requester.UserId,
                allowAutoApply: true,
                cancellationToken: cancellationToken);

            var currentRequest = await _dbContext.WfpOvertimeRequests
                .AsNoTracking()
                .FirstAsync(x => x.Id == overtimeRequest.Id, cancellationToken);

            _logger.LogInformation(
                "Overtime request {OvertimeRequestId} terintegrasi ke workflow {WorkflowInstanceId}. Created={Created}, Submitted={Submitted}.",
                overtimeRequest.Id,
                workflowDetail.Id,
                workflowCreated,
                workflowSubmitted);

            return OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse>.Ok(
                BuildResponse(
                    currentRequest.Id,
                    currentRequest.RequestNumber,
                    previousRequestStatus,
                    currentRequest.OvertimeRequestStatus,
                    workflowDetail,
                    workflowCreated,
                    workflowSubmitted,
                    synchronized.IsHandled),
                workflowCreated
                    ? "Workflow OVERTIME_REQUEST berhasil dibuat dan disubmit."
                    : workflowSubmitted
                        ? "Workflow OVERTIME_REQUEST berhasil disubmit ulang."
                        : "Workflow OVERTIME_REQUEST sudah tersedia dan lifecycle berhasil disinkronkan.");
        }

        public async Task<OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse>>
            CancelOrWithdrawAsync(
                Guid overtimeRequestId,
                string reason,
                string? idempotencyKey,
                CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return Fail("Alasan pembatalan wajib diisi.");
            }

            var overtimeRequest = await _dbContext.WfpOvertimeRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == overtimeRequestId && !x.IsDelete,
                    cancellationToken);

            if (overtimeRequest == null)
            {
                return OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Overtime request tidak ditemukan.");
            }

            var periodGuard = await CheckPeriodAsync(overtimeRequest, cancellationToken);
            if (!periodGuard.IsWritable)
                return Conflict(periodGuard.Message);

            if (!overtimeRequest.WorkflowInstanceId.HasValue)
            {
                return Conflict("Overtime request belum memiliki workflow instance.");
            }

            var workflow = await _dbContext.TrxWorkflowInstances
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == overtimeRequest.WorkflowInstanceId.Value && !x.IsDelete,
                    cancellationToken);

            if (workflow == null)
            {
                return OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workflow instance Overtime tidak ditemukan.");
            }

            WorkflowServiceResult<WorkflowInstanceDetailResponse> actionResult;
            if (string.Equals(
                    workflow.WorkflowStatus,
                    WorkflowValueConstants.WorkflowStatus.InProgress,
                    StringComparison.OrdinalIgnoreCase))
            {
                actionResult = await _workflowService.WithdrawAsync(
                    workflow.Id,
                    new WorkflowWithdrawRequest
                    {
                        Reason = NormalizeReason(reason),
                        IdempotencyKey = NormalizeIdempotencyKey(
                            idempotencyKey,
                            $"OTR-WF-WITHDRAW-{overtimeRequest.Id:N}")
                    },
                    cancellationToken);
            }
            else if (CanCancelWorkflow(workflow.WorkflowStatus))
            {
                actionResult = await _workflowService.CancelAsync(
                    workflow.Id,
                    new WorkflowCancelRequest
                    {
                        Reason = NormalizeReason(reason),
                        IdempotencyKey = NormalizeIdempotencyKey(
                            idempotencyKey,
                            $"OTR-WF-CANCEL-{overtimeRequest.Id:N}")
                    },
                    cancellationToken);
            }
            else if (string.Equals(
                         workflow.WorkflowStatus,
                         WorkflowValueConstants.WorkflowStatus.Cancelled,
                         StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(
                         workflow.WorkflowStatus,
                         WorkflowValueConstants.WorkflowStatus.Withdrawn,
                         StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _workflowService.GetByIdAsync(workflow.Id, cancellationToken);
                if (!existing.Success || existing.Data == null)
                {
                    return OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse>.Fail(
                        existing.StatusCode,
                        existing.Message);
                }

                actionResult = WorkflowServiceResult<WorkflowInstanceDetailResponse>.Ok(
                    existing.Data,
                    "Workflow sudah dibatalkan atau ditarik.");
            }
            else
            {
                return Conflict(
                    $"Workflow berstatus {workflow.WorkflowStatus} tidak dapat dibatalkan atau ditarik.");
            }

            if (!actionResult.Success || actionResult.Data == null)
            {
                return OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse>.Fail(
                    actionResult.StatusCode,
                    actionResult.Message);
            }

            var synchronized = await _lifecycleService.SynchronizeAsync(
                actionResult.Data.Id,
                actionResult.Data.Requester.UserId,
                allowAutoApply: true,
                cancellationToken: cancellationToken);

            var currentRequest = await _dbContext.WfpOvertimeRequests
                .AsNoTracking()
                .FirstAsync(x => x.Id == overtimeRequest.Id, cancellationToken);

            return OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse>.Ok(
                BuildResponse(
                    currentRequest.Id,
                    currentRequest.RequestNumber,
                    overtimeRequest.OvertimeRequestStatus,
                    currentRequest.OvertimeRequestStatus,
                    actionResult.Data,
                    false,
                    false,
                    synchronized.IsHandled),
                actionResult.Message);
        }

        public async Task<OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse>>
            SynchronizeAsync(
                Guid overtimeRequestId,
                Guid actorUserId,
                bool allowAutoApply = true,
                CancellationToken cancellationToken = default)
        {
            var overtimeRequest = await _dbContext.WfpOvertimeRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == overtimeRequestId && !x.IsDelete,
                    cancellationToken);

            if (overtimeRequest == null)
            {
                return OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Overtime request tidak ditemukan.");
            }

            if (allowAutoApply)
            {
                var periodGuard = await CheckPeriodAsync(overtimeRequest, cancellationToken);
                if (!periodGuard.IsWritable)
                    return Conflict(periodGuard.Message);
            }

            var workflowId = overtimeRequest.WorkflowInstanceId ??
                await _dbContext.TrxWorkflowInstances
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.ReferenceType == OvertimeValueConstants.Workflow.ReferenceType &&
                        x.ReferenceId == overtimeRequest.Id)
                    .OrderByDescending(x => x.CreateDateTime)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

            if (!workflowId.HasValue)
            {
                return Conflict("Overtime request belum mempunyai workflow instance untuk disinkronkan.");
            }

            var previousStatus = overtimeRequest.OvertimeRequestStatus;
            var sync = await _lifecycleService.SynchronizeAsync(
                workflowId.Value,
                actorUserId,
                allowAutoApply,
                cancellationToken);

            var workflowResult = await _workflowService.GetByIdAsync(
                workflowId.Value,
                cancellationToken);

            if (!workflowResult.Success || workflowResult.Data == null)
            {
                return OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse>.Fail(
                    workflowResult.StatusCode,
                    workflowResult.Message);
            }

            var currentRequest = await _dbContext.WfpOvertimeRequests
                .AsNoTracking()
                .FirstAsync(x => x.Id == overtimeRequest.Id, cancellationToken);

            return OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse>.Ok(
                BuildResponse(
                    currentRequest.Id,
                    currentRequest.RequestNumber,
                    previousStatus,
                    currentRequest.OvertimeRequestStatus,
                    workflowResult.Data,
                    false,
                    false,
                    sync.IsHandled),
                sync.WarningMessage ?? "Lifecycle Overtime Request berhasil disinkronkan.");
        }

        private Task<OvertimePeriodGuardResult> CheckPeriodAsync(
            QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models.WfpOvertimeRequest request,
            CancellationToken cancellationToken) =>
            _periodGuard.CheckDateAsync(
                request.OvertimeDate,
                null,
                request.HospitalSiteId,
                request.OrganizationUnitId,
                request.DepartmentId,
                cancellationToken);

        private async Task<QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models.WfpOvertimeRequest>
            AutoApproveWithoutWorkflowAsync(
                Guid overtimeRequestId,
                CancellationToken cancellationToken)
        {
            var request = await _dbContext.WfpOvertimeRequests
                .Include(x => x.Details.Where(d => !d.IsDelete))
                .FirstAsync(x => x.Id == overtimeRequestId, cancellationToken);

            var now = DateTime.UtcNow;
            request.OvertimeRequestStatus = OvertimeValueConstants.RequestStatus.ApprovedForWork;
            request.ApprovedMinutes = request.RequestedMinutes;
            request.SubmittedAt ??= now;
            request.ApprovedAt ??= now;
            request.SubmittedByUserId ??= request.UpdateBy != Guid.Empty ? request.UpdateBy : request.CreateBy;
            request.ApprovedByUserId = request.SubmittedByUserId;
            request.CurrentApprovalStep = 0;
            request.IsCancel = false;
            request.IsActive = true;
            request.UpdateDateTime = now;

            foreach (var detail in request.Details)
            {
                detail.DetailStatus = OvertimeValueConstants.RequestStatus.ApprovedForWork;
                detail.ApprovedMinutes = detail.RequestedMinutes;
                detail.ApprovedStartAt ??= detail.PlannedStartAt;
                detail.ApprovedEndAt ??= detail.PlannedEndAt;
                detail.IsCancel = false;
                detail.IsActive = true;
                detail.UpdateDateTime = now;
                detail.UpdateBy = request.UpdateBy;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return request;
        }

        private static OvertimeWorkflowIntegrationResponse BuildResponse(
            Guid overtimeRequestId,
            string requestNumber,
            string previousRequestStatus,
            string currentRequestStatus,
            WorkflowInstanceDetailResponse workflow,
            bool workflowCreated,
            bool workflowSubmitted,
            bool lifecycleSynchronized) => new()
        {
            OvertimeRequestId = overtimeRequestId,
            RequestNumber = requestNumber,
            PreviousRequestStatus = previousRequestStatus,
            CurrentRequestStatus = currentRequestStatus,
            WorkflowDefinitionId = workflow.WorkflowDefinitionId,
            WorkflowInstanceId = workflow.Id,
            WorkflowRequestNumber = workflow.RequestNumber,
            WorkflowStatus = workflow.WorkflowStatus,
            CurrentStepOrder = workflow.CurrentStepOrder,
            CurrentStepCode = workflow.CurrentStepCode,
            WorkflowCreated = workflowCreated,
            WorkflowSubmitted = workflowSubmitted,
            LifecycleSynchronized = lifecycleSynchronized,
            ActionAt = DateTime.UtcNow
        };

        private static bool CanSubmitWorkflow(string status) =>
            string.Equals(status, WorkflowValueConstants.WorkflowStatus.Draft, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, WorkflowValueConstants.WorkflowStatus.RevisionRequested, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, WorkflowValueConstants.WorkflowStatus.Returned, StringComparison.OrdinalIgnoreCase);

        private static bool IsWorkflowAlreadyProcessed(string status) =>
            string.Equals(status, WorkflowValueConstants.WorkflowStatus.Submitted, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, WorkflowValueConstants.WorkflowStatus.InProgress, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, WorkflowValueConstants.WorkflowStatus.Completed, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, WorkflowValueConstants.WorkflowStatus.Approved, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, WorkflowValueConstants.WorkflowStatus.Rejected, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, WorkflowValueConstants.WorkflowStatus.Cancelled, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, WorkflowValueConstants.WorkflowStatus.Withdrawn, StringComparison.OrdinalIgnoreCase);

        private static bool CanCancelWorkflow(string status) =>
            string.Equals(status, WorkflowValueConstants.WorkflowStatus.Draft, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, WorkflowValueConstants.WorkflowStatus.Submitted, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, WorkflowValueConstants.WorkflowStatus.RevisionRequested, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, WorkflowValueConstants.WorkflowStatus.Returned, StringComparison.OrdinalIgnoreCase);

        private static string NormalizeSourceChannel(string? sourceChannel)
        {
            var value = sourceChannel?.Trim();
            if (string.Equals(value, WorkflowValueConstants.SourceChannel.Mobile, StringComparison.OrdinalIgnoreCase))
                return WorkflowValueConstants.SourceChannel.Mobile;
            if (string.Equals(value, WorkflowValueConstants.SourceChannel.Api, StringComparison.OrdinalIgnoreCase))
                return WorkflowValueConstants.SourceChannel.Api;
            if (string.Equals(value, WorkflowValueConstants.SourceChannel.Integration, StringComparison.OrdinalIgnoreCase))
                return WorkflowValueConstants.SourceChannel.Integration;
            return WorkflowValueConstants.SourceChannel.Web;
        }

        private static string NormalizeIdempotencyKey(
            string? requestedKey,
            string fallback,
            string suffix = "")
        {
            var value = string.IsNullOrWhiteSpace(requestedKey)
                ? fallback
                : requestedKey.Trim() + suffix;

            return value.Length <= 100 ? value : value[..100];
        }

        private static string? NormalizeOptionalText(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string NormalizeReason(string reason)
        {
            var value = reason.Trim();
            return value.Length <= 1000 ? value : value[..1000];
        }

        private static OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse> Fail(
            string message) =>
            OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse>.Fail(
                StatusCodes.Status400BadRequest,
                message);

        private static OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse> Conflict(
            string message) =>
            OvertimeWorkflowServiceResult<OvertimeWorkflowIntegrationResponse>.Fail(
                StatusCodes.Status409Conflict,
                message);
    }
}
