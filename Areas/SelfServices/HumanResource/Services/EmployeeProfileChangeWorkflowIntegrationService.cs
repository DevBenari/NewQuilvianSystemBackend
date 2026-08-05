using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Areas.SelfServices.HumanResource.DTOs;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.SelfServices.HumanResource.Services
{
    /// <summary>
    /// Adapter antara transaksi Employee Profile Change dan Workflow Engine generik.
    /// Data perubahan tetap dimiliki modul profile change. Seluruh keputusan approval
    /// tetap dimiliki WorkflowService dan Approval Inbox.
    /// </summary>
    public class EmployeeProfileChangeWorkflowIntegrationService
    {
        private static readonly HashSet<string> SupportedReferenceTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                WorkflowReferenceLifecycleService.EmployeeProfileChangeReferenceType,
                "EmployeeProfileChange",
                "TrxEmployeeProfileChangeRequest",
                "PROFILE_CHANGE"
            };

        private readonly ApplicationDbContext _dbContext;
        private readonly EmployeeProfileChangeService _employeeProfileChangeService;
        private readonly WorkflowService _workflowService;
        private readonly WorkflowReferenceLifecycleService
            _workflowReferenceLifecycleService;

        public EmployeeProfileChangeWorkflowIntegrationService(
            ApplicationDbContext dbContext,
            EmployeeProfileChangeService employeeProfileChangeService,
            WorkflowService workflowService,
            WorkflowReferenceLifecycleService workflowReferenceLifecycleService)
        {
            _dbContext = dbContext;
            _employeeProfileChangeService = employeeProfileChangeService;
            _workflowService = workflowService;
            _workflowReferenceLifecycleService =
                workflowReferenceLifecycleService;
        }

        public async Task<EmployeeProfileChangeServiceResult<
            EmployeeProfileChangeWorkflowLinkResponse>> GetWorkflowAsync(
                Guid profileChangeRequestId,
                CancellationToken cancellationToken = default)
        {
            var source = await LoadSourceRequestAsync(
                profileChangeRequestId,
                cancellationToken);

            if (source == null)
            {
                return EmployeeProfileChangeServiceResult<
                    EmployeeProfileChangeWorkflowLinkResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Employee profile change tidak ditemukan.");
            }

            var workflow = await FindLatestWorkflowAsync(
                profileChangeRequestId,
                cancellationToken);

            if (workflow == null)
            {
                return EmployeeProfileChangeServiceResult<
                    EmployeeProfileChangeWorkflowLinkResponse>.Ok(
                        new EmployeeProfileChangeWorkflowLinkResponse
                        {
                            ProfileChangeRequestId = source.Id,
                            ProfileChangeRequestNumber = source.RequestNumber,
                            ProfileChangeStatus = source.RequestStatus,
                            HasWorkflow = false,
                            IsSynchronized = string.Equals(
                                source.RequestStatus,
                                "Draft",
                                StringComparison.OrdinalIgnoreCase),
                            IsAutoApplyPending = false
                        },
                        "Employee profile change belum mempunyai workflow instance.");
            }

            var workflowResult = await _workflowService.GetByIdAsync(
                workflow.Id,
                cancellationToken);

            if (!workflowResult.Success || workflowResult.Data == null)
            {
                return EmployeeProfileChangeServiceResult<
                    EmployeeProfileChangeWorkflowLinkResponse>.Fail(
                        workflowResult.StatusCode,
                        workflowResult.Message);
            }

            return EmployeeProfileChangeServiceResult<
                EmployeeProfileChangeWorkflowLinkResponse>.Ok(
                    new EmployeeProfileChangeWorkflowLinkResponse
                    {
                        ProfileChangeRequestId = source.Id,
                        ProfileChangeRequestNumber = source.RequestNumber,
                        ProfileChangeStatus = source.RequestStatus,
                        HasWorkflow = true,
                        IsSynchronized = IsSynchronized(
                            source.RequestStatus,
                            workflowResult.Data.WorkflowStatus),
                        IsAutoApplyPending = IsAutoApplyPending(
                            source.RequestStatus,
                            workflowResult.Data.WorkflowStatus),
                        Workflow = workflowResult.Data
                    },
                    "Relasi workflow employee profile change berhasil diambil.");
        }

        public async Task<EmployeeProfileChangeServiceResult<
            EmployeeProfileChangeWorkflowResponse>> SubmitAsync(
                Guid profileChangeRequestId,
                EmployeeProfileChangeWorkflowSubmitRequest? request,
                Guid actorUserId,
                CancellationToken cancellationToken = default)
        {
            if (profileChangeRequestId == Guid.Empty || actorUserId == Guid.Empty)
            {
                return EmployeeProfileChangeServiceResult<
                    EmployeeProfileChangeWorkflowResponse>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Employee profile change id atau actor user id tidak valid.");
            }

            var source = await _dbContext.Set<TrxEmployeeProfileChangeRequest>()
                .Include(x => x.Details)
                .FirstOrDefaultAsync(
                    x => x.Id == profileChangeRequestId && !x.IsDelete,
                    cancellationToken);

            if (source == null)
            {
                return EmployeeProfileChangeServiceResult<
                    EmployeeProfileChangeWorkflowResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Employee profile change tidak ditemukan.");
            }

            if (source.RequestedByUserId != actorUserId)
            {
                return EmployeeProfileChangeServiceResult<
                    EmployeeProfileChangeWorkflowResponse>.Fail(
                        StatusCodes.Status403Forbidden,
                        "Hanya pemohon yang dapat submit employee profile change.");
            }

            var canSubmit =
                string.Equals(
                    source.RequestStatus,
                    "Draft",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    source.RequestStatus,
                    "NeedRevision",
                    StringComparison.OrdinalIgnoreCase);

            if (!canSubmit)
            {
                var runningWorkflow = await FindLatestWorkflowAsync(
                    source.Id,
                    cancellationToken);

                if (runningWorkflow != null &&
                    IsWorkflowRunning(runningWorkflow.WorkflowStatus))
                {
                    var existingResult = await _workflowService.GetByIdAsync(
                        runningWorkflow.Id,
                        cancellationToken);

                    if (existingResult.Success && existingResult.Data != null)
                    {
                        return await BuildCombinedResponseAsync(
                            source.Id,
                            existingResult.Data,
                            "Employee profile change sudah berada dalam proses workflow.",
                            cancellationToken);
                    }
                }

                return EmployeeProfileChangeServiceResult<
                    EmployeeProfileChangeWorkflowResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Employee profile change hanya dapat di-submit dari status Draft atau NeedRevision.");
            }

            if (!source.Details.Any(x => !x.IsDelete))
            {
                return EmployeeProfileChangeServiceResult<
                    EmployeeProfileChangeWorkflowResponse>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Employee profile change harus mempunyai minimal satu detail perubahan.");
            }

            var existingWorkflow = await FindLatestWorkflowAsync(
                source.Id,
                cancellationToken);

            WorkflowInstanceDetailResponse workflowDetail;
            var workflowCreatedNow = false;

            if (existingWorkflow != null)
            {
                if (string.Equals(
                        existingWorkflow.WorkflowStatus,
                        WorkflowValueConstants.WorkflowStatus.Draft,
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        existingWorkflow.WorkflowStatus,
                        WorkflowValueConstants.WorkflowStatus.RevisionRequested,
                        StringComparison.OrdinalIgnoreCase))
                {
                    var submitExistingResult = await _workflowService.SubmitAsync(
                        existingWorkflow.Id,
                        new WorkflowSubmitRequest
                        {
                            Comment = NormalizeNote(request?.Note),
                            IdempotencyKey = NormalizeOptionalText(
                                request?.IdempotencyKey)
                        },
                        cancellationToken);

                    if (!submitExistingResult.Success ||
                        submitExistingResult.Data == null)
                    {
                        return EmployeeProfileChangeServiceResult<
                            EmployeeProfileChangeWorkflowResponse>.Fail(
                                submitExistingResult.StatusCode,
                                submitExistingResult.Message);
                    }

                    workflowDetail = submitExistingResult.Data;
                }
                else if (IsWorkflowRunning(existingWorkflow.WorkflowStatus))
                {
                    var existingDetailResult = await _workflowService.GetByIdAsync(
                        existingWorkflow.Id,
                        cancellationToken);

                    if (!existingDetailResult.Success ||
                        existingDetailResult.Data == null)
                    {
                        return EmployeeProfileChangeServiceResult<
                            EmployeeProfileChangeWorkflowResponse>.Fail(
                                existingDetailResult.StatusCode,
                                existingDetailResult.Message);
                    }

                    workflowDetail = existingDetailResult.Data;
                }
                else
                {
                    return EmployeeProfileChangeServiceResult<
                        EmployeeProfileChangeWorkflowResponse>.Fail(
                            StatusCodes.Status409Conflict,
                            $"Workflow employee profile change sebelumnya sudah berstatus {existingWorkflow.WorkflowStatus} dan tidak dapat digunakan kembali.");
                }
            }
            else
            {
                var workflowCodeResult = await ResolveWorkflowCodeAsync(
                    source,
                    cancellationToken);

                if (!workflowCodeResult.Success)
                {
                    return EmployeeProfileChangeServiceResult<
                        EmployeeProfileChangeWorkflowResponse>.Fail(
                            workflowCodeResult.StatusCode,
                            workflowCodeResult.Message);
                }

                var createResult = await _workflowService.CreateAsync(
                    new CreateWorkflowInstanceRequest
                    {
                        WorkflowDefinitionCode = workflowCodeResult.WorkflowCode!,
                        ReferenceType =
                            WorkflowReferenceLifecycleService
                                .EmployeeProfileChangeReferenceType,
                        ReferenceId = source.Id,
                        ExternalReferenceNumber = source.RequestNumber,
                        SourceChannel = NormalizeSourceChannel(
                            request?.SourceChannel),
                        RequestCorrelationId = NormalizeOptionalText(
                            request?.RequestCorrelationId),
                        IdempotencyKey =
                            $"employee-profile-change:{source.Id:N}:workflow",
                        RequestContext = JsonSerializer.SerializeToElement(new
                        {
                            profileChangeRequestId = source.Id,
                            source.RequestNumber,
                            source.WorkforceProfileId,
                            source.RequestCategory,
                            source.RequestReasonId,
                            source.RequestReasonText,
                            source.Description,
                            detailCount = source.Details.Count(x => !x.IsDelete),
                            requestedByUserId = source.RequestedByUserId
                        }),
                        SelectedApproverUserIds = request?
                            .SelectedApproverUserIds?
                            .Where(x => x != Guid.Empty)
                            .Distinct()
                            .ToList() ?? new List<Guid>()
                    },
                    cancellationToken);

                if (!createResult.Success || createResult.Data == null)
                {
                    return EmployeeProfileChangeServiceResult<
                        EmployeeProfileChangeWorkflowResponse>.Fail(
                            createResult.StatusCode,
                            createResult.Message);
                }

                workflowCreatedNow = true;
                workflowDetail = createResult.Data;

                source.WorkflowDefinitionId = workflowDetail.WorkflowDefinitionId;
                source.UpdateDateTime = DateTime.UtcNow;
                source.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);

                var submitResult = await _workflowService.SubmitAsync(
                    workflowDetail.Id,
                    new WorkflowSubmitRequest
                    {
                        Comment = NormalizeNote(request?.Note),
                        IdempotencyKey = NormalizeOptionalText(
                            request?.IdempotencyKey)
                    },
                    cancellationToken);

                if (!submitResult.Success || submitResult.Data == null)
                {
                    if (workflowCreatedNow)
                    {
                        await SoftDeleteFailedDraftWorkflowAsync(
                            workflowDetail.Id,
                            actorUserId,
                            cancellationToken);
                    }

                    return EmployeeProfileChangeServiceResult<
                        EmployeeProfileChangeWorkflowResponse>.Fail(
                            submitResult.StatusCode,
                            submitResult.Message);
                }

                workflowDetail = submitResult.Data;
            }

            await _workflowReferenceLifecycleService.SynchronizeAsync(
                workflowDetail.Id,
                actorUserId,
                allowAutoApply: true,
                cancellationToken: cancellationToken);

            return await BuildCombinedResponseAsync(
                source.Id,
                workflowDetail,
                workflowCreatedNow
                    ? "Employee profile change berhasil dibuatkan workflow dan di-submit."
                    : "Employee profile change berhasil di-submit ke workflow.",
                cancellationToken);
        }

        public async Task<EmployeeProfileChangeServiceResult<
            EmployeeProfileChangeWorkflowResponse>> CancelAsync(
                Guid profileChangeRequestId,
                EmployeeProfileChangeWorkflowCancelRequest? request,
                Guid actorUserId,
                CancellationToken cancellationToken = default)
        {
            var source = await LoadSourceRequestAsync(
                profileChangeRequestId,
                cancellationToken);

            if (source == null)
            {
                return EmployeeProfileChangeServiceResult<
                    EmployeeProfileChangeWorkflowResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Employee profile change tidak ditemukan.");
            }

            if (source.RequestedByUserId != actorUserId)
            {
                return EmployeeProfileChangeServiceResult<
                    EmployeeProfileChangeWorkflowResponse>.Fail(
                        StatusCodes.Status403Forbidden,
                        "Hanya pemohon yang dapat membatalkan employee profile change.");
            }

            var workflow = await FindLatestWorkflowAsync(
                source.Id,
                cancellationToken);

            var reason = NormalizeOptionalText(request?.Reason) ??
                         "Dibatalkan oleh pemohon.";

            if (workflow == null)
            {
                var legacyResult = await _employeeProfileChangeService.CancelAsync(
                    source.Id,
                    reason,
                    actorUserId,
                    cancellationToken);

                if (!legacyResult.Success || legacyResult.Data == null)
                {
                    return EmployeeProfileChangeServiceResult<
                        EmployeeProfileChangeWorkflowResponse>.Fail(
                            legacyResult.StatusCode,
                            legacyResult.Message);
                }

                return EmployeeProfileChangeServiceResult<
                    EmployeeProfileChangeWorkflowResponse>.Ok(
                        new EmployeeProfileChangeWorkflowResponse
                        {
                            ProfileChangeRequestId = legacyResult.Data.Id,
                            ProfileChangeRequestNumber = legacyResult.Data.RequestNumber,
                            ProfileChangeStatus = legacyResult.Data.RequestStatus,
                            HasWorkflow = false,
                            IsSynchronized = true,
                            IsAutoApplyPending = false,
                            ProfileChange = legacyResult.Data,
                            Workflow = null
                        },
                        "Employee profile change draft berhasil dibatalkan sebelum workflow dibuat.");
            }

            WorkflowServiceResult<WorkflowInstanceDetailResponse> workflowResult;

            if (string.Equals(
                    workflow.WorkflowStatus,
                    WorkflowValueConstants.WorkflowStatus.InProgress,
                    StringComparison.OrdinalIgnoreCase))
            {
                workflowResult = await _workflowService.WithdrawAsync(
                    workflow.Id,
                    new WorkflowWithdrawRequest
                    {
                        Reason = reason,
                        IdempotencyKey = NormalizeOptionalText(
                            request?.IdempotencyKey)
                    },
                    cancellationToken);
            }
            else
            {
                workflowResult = await _workflowService.CancelAsync(
                    workflow.Id,
                    new WorkflowCancelRequest
                    {
                        Reason = reason,
                        IdempotencyKey = NormalizeOptionalText(
                            request?.IdempotencyKey)
                    },
                    cancellationToken);
            }

            if (!workflowResult.Success || workflowResult.Data == null)
            {
                return EmployeeProfileChangeServiceResult<
                    EmployeeProfileChangeWorkflowResponse>.Fail(
                        workflowResult.StatusCode,
                        workflowResult.Message);
            }

            await _workflowReferenceLifecycleService.SynchronizeAsync(
                workflow.Id,
                actorUserId,
                allowAutoApply: false,
                cancellationToken: cancellationToken);

            return await BuildCombinedResponseAsync(
                source.Id,
                workflowResult.Data,
                "Employee profile change dan workflow berhasil dibatalkan.",
                cancellationToken);
        }

        public async Task<EmployeeProfileChangeServiceResult<
            EmployeeProfileChangeWorkflowSynchronizationResponse>>
            SynchronizeAsync(
                Guid profileChangeRequestId,
                Guid actorUserId,
                CancellationToken cancellationToken = default)
        {
            var workflow = await FindLatestWorkflowAsync(
                profileChangeRequestId,
                cancellationToken);

            if (workflow == null)
            {
                return EmployeeProfileChangeServiceResult<
                    EmployeeProfileChangeWorkflowSynchronizationResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Workflow untuk employee profile change tidak ditemukan.");
            }

            var synchronization =
                await _workflowReferenceLifecycleService.SynchronizeAsync(
                    workflow.Id,
                    actorUserId,
                    allowAutoApply: true,
                    cancellationToken: cancellationToken);

            return EmployeeProfileChangeServiceResult<
                EmployeeProfileChangeWorkflowSynchronizationResponse>.Ok(
                    new EmployeeProfileChangeWorkflowSynchronizationResponse
                    {
                        ProfileChangeRequestId = profileChangeRequestId,
                        WorkflowInstanceId = workflow.Id,
                        PreviousProfileChangeStatus =
                            synchronization.PreviousReferenceStatus,
                        CurrentProfileChangeStatus =
                            synchronization.CurrentReferenceStatus,
                        WorkflowStatus = synchronization.WorkflowStatus,
                        StatusChanged = synchronization.StatusChanged,
                        AutoApplyAttempted =
                            synchronization.AutoApplyAttempted,
                        AutoApplySucceeded =
                            synchronization.AutoApplySucceeded,
                        WarningMessage = synchronization.WarningMessage
                    },
                    synchronization.WarningMessage ??
                    "Status employee profile change berhasil disinkronkan dengan workflow.");
        }

        private async Task<EmployeeProfileChangeServiceResult<
            EmployeeProfileChangeWorkflowResponse>> BuildCombinedResponseAsync(
                Guid profileChangeRequestId,
                WorkflowInstanceDetailResponse workflow,
                string message,
                CancellationToken cancellationToken)
        {
            var sourceResult = await _employeeProfileChangeService.GetByIdAsync(
                profileChangeRequestId,
                cancellationToken);

            if (!sourceResult.Success || sourceResult.Data == null)
            {
                return EmployeeProfileChangeServiceResult<
                    EmployeeProfileChangeWorkflowResponse>.Fail(
                        sourceResult.StatusCode,
                        sourceResult.Message);
            }

            var source = sourceResult.Data;

            return EmployeeProfileChangeServiceResult<
                EmployeeProfileChangeWorkflowResponse>.Ok(
                    new EmployeeProfileChangeWorkflowResponse
                    {
                        ProfileChangeRequestId = source.Id,
                        ProfileChangeRequestNumber = source.RequestNumber,
                        ProfileChangeStatus = source.RequestStatus,
                        HasWorkflow = true,
                        WorkflowInstanceId = workflow.Id,
                        WorkflowRequestNumber = workflow.RequestNumber,
                        WorkflowDefinitionId = workflow.WorkflowDefinitionId,
                        WorkflowCode = workflow.WorkflowCode,
                        WorkflowName = workflow.WorkflowName,
                        WorkflowStatus = workflow.WorkflowStatus,
                        CurrentStepOrder = workflow.CurrentStepOrder,
                        CurrentStepCode = workflow.CurrentStepCode,
                        IsSynchronized = IsSynchronized(
                            source.RequestStatus,
                            workflow.WorkflowStatus),
                        IsAutoApplyPending = IsAutoApplyPending(
                            source.RequestStatus,
                            workflow.WorkflowStatus),
                        ProfileChange = source,
                        Workflow = workflow
                    },
                    message);
        }

        private async Task<TrxEmployeeProfileChangeRequest?> LoadSourceRequestAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
            {
                return null;
            }

            return await _dbContext.Set<TrxEmployeeProfileChangeRequest>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDelete,
                    cancellationToken);
        }

        private async Task<TrxWorkflowInstance?> FindLatestWorkflowAsync(
            Guid profileChangeRequestId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<TrxWorkflowInstance>()
                .AsNoTracking()
                .Where(x =>
                    x.ReferenceId == profileChangeRequestId &&
                    SupportedReferenceTypes.Contains(x.ReferenceType) &&
                    !x.IsDelete)
                .OrderByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<WorkflowCodeResolutionResult> ResolveWorkflowCodeAsync(
            TrxEmployeeProfileChangeRequest source,
            CancellationToken cancellationToken)
        {
            if (!source.WorkflowDefinitionId.HasValue ||
                source.WorkflowDefinitionId.Value == Guid.Empty)
            {
                return WorkflowCodeResolutionResult.Ok("PROFILE_CHANGE");
            }

            var definition = await _dbContext.Set<MstWorkflowDefinition>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == source.WorkflowDefinitionId.Value &&
                        x.IsActive &&
                        !x.IsDelete,
                    cancellationToken);

            if (definition == null)
            {
                return WorkflowCodeResolutionResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workflow definition employee profile change tidak ditemukan atau tidak aktif.");
            }

            return WorkflowCodeResolutionResult.Ok(definition.WorkflowCode);
        }

        private async Task SoftDeleteFailedDraftWorkflowAsync(
            Guid workflowInstanceId,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var workflow = await _dbContext.Set<TrxWorkflowInstance>()
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == workflowInstanceId &&
                        x.WorkflowStatus ==
                            WorkflowValueConstants.WorkflowStatus.Draft &&
                        !x.IsDelete,
                    cancellationToken);

            if (workflow == null)
            {
                return;
            }

            var now = DateTime.UtcNow;
            workflow.IsDelete = true;
            workflow.DeleteDateTime = now;
            workflow.DeleteBy = actorUserId;
            workflow.UpdateDateTime = now;
            workflow.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private static bool IsWorkflowRunning(string workflowStatus)
        {
            return string.Equals(
                       workflowStatus,
                       WorkflowValueConstants.WorkflowStatus.Submitted,
                       StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(
                       workflowStatus,
                       WorkflowValueConstants.WorkflowStatus.InProgress,
                       StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSynchronized(
            string profileChangeStatus,
            string workflowStatus)
        {
            if (string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Completed,
                    StringComparison.OrdinalIgnoreCase))
            {
                return string.Equals(
                           profileChangeStatus,
                           "Approved",
                           StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(
                           profileChangeStatus,
                           "Applied",
                           StringComparison.OrdinalIgnoreCase);
            }

            if (string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.InProgress,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Submitted,
                    StringComparison.OrdinalIgnoreCase))
            {
                return string.Equals(
                           profileChangeStatus,
                           "Submitted",
                           StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(
                           profileChangeStatus,
                           "UnderVerification",
                           StringComparison.OrdinalIgnoreCase);
            }

            var expectedStatus =
                WorkflowReferenceLifecycleService.MapProfileChangeStatus(
                    workflowStatus);

            return string.Equals(
                profileChangeStatus,
                expectedStatus,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAutoApplyPending(
            string profileChangeStatus,
            string workflowStatus)
        {
            return string.Equals(
                       workflowStatus,
                       WorkflowValueConstants.WorkflowStatus.Completed,
                       StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(
                       profileChangeStatus,
                       "Approved",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeSourceChannel(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return WorkflowValueConstants.SourceChannel.Web;
            }

            var normalized = value.Trim();
            var allowed = new[]
            {
                WorkflowValueConstants.SourceChannel.Web,
                WorkflowValueConstants.SourceChannel.Mobile,
                WorkflowValueConstants.SourceChannel.Api,
                WorkflowValueConstants.SourceChannel.System,
                WorkflowValueConstants.SourceChannel.Integration
            };

            return allowed.FirstOrDefault(x =>
                       string.Equals(
                           x,
                           normalized,
                           StringComparison.OrdinalIgnoreCase)) ??
                   WorkflowValueConstants.SourceChannel.Web;
        }

        private static string? NormalizeNote(string? value)
        {
            var normalized = NormalizeOptionalText(value);
            if (normalized == null)
            {
                return null;
            }

            return normalized.Length <= 4000
                ? normalized
                : normalized[..4000];
        }

        private static string? NormalizeOptionalText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private sealed class WorkflowCodeResolutionResult
        {
            public bool Success { get; private set; }

            public int StatusCode { get; private set; }

            public string Message { get; private set; } = string.Empty;

            public string? WorkflowCode { get; private set; }

            public static WorkflowCodeResolutionResult Ok(string code)
            {
                return new WorkflowCodeResolutionResult
                {
                    Success = true,
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Workflow code berhasil ditentukan.",
                    WorkflowCode = code
                };
            }

            public static WorkflowCodeResolutionResult Fail(
                int statusCode,
                string message)
            {
                return new WorkflowCodeResolutionResult
                {
                    Success = false,
                    StatusCode = statusCode,
                    Message = message
                };
            }
        }
    }
}
