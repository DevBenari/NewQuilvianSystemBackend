using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Layanan integrasi persetujuan S-D1 Perencanaan Tenaga Kerja (Workforce Planning)
    /// yang mengelola lima jenis rujukan workflow, penerusan keputusan akhir ke domain secara idempoten,
    /// serta perlindungan privasi data sensitif pada logger.
    /// </summary>
    public class HrdWorkforcePlanningWorkflowIntegrationService : IHrdWorkforcePlanningWorkflowIntegrationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<IWorkforcePlanningDecisionApplier> _decisionAppliers;
        private readonly ILogger<HrdWorkforcePlanningWorkflowIntegrationService> _logger;

        public HrdWorkforcePlanningWorkflowIntegrationService(
            ApplicationDbContext dbContext,
            IServiceProvider serviceProvider,
            IEnumerable<IWorkforcePlanningDecisionApplier> decisionAppliers,
            ILogger<HrdWorkforcePlanningWorkflowIntegrationService> logger)
        {
            _dbContext = dbContext;
            _serviceProvider = serviceProvider;
            _decisionAppliers = decisionAppliers;
            _logger = logger;
        }

        public async Task<WorkflowServiceResult<WorkflowInstanceDetailResponse>> StartWorkflowAsync(
            StartWorkforcePlanningWorkflowRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!IsRecognizedReferenceType(request.ReferenceType))
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Jenis rujukan workflow {request.ReferenceType} tidak dikenali untuk modul Workforce Planning.");
            }

            var workflowService = _serviceProvider.GetRequiredService<WorkflowService>();

            var createResult = await workflowService.CreateAsync(
                new CreateWorkflowInstanceRequest
                {
                    WorkflowDefinitionCode = request.WorkflowDefinitionCode,
                    ReferenceType = request.ReferenceType,
                    ReferenceId = request.ReferenceId,
                    ExternalReferenceNumber = request.DocumentNumber,
                    SourceChannel = string.IsNullOrWhiteSpace(request.SourceChannel) ? "Web" : request.SourceChannel,
                    RequestCorrelationId = request.RequestCorrelationId,
                    IdempotencyKey = request.IdempotencyKey,
                    RequestContext = request.RequestContext,
                    SelectedApproverUserIds = request.SelectedApproverUserIds?.ToList() ?? new List<Guid>()
                },
                cancellationToken);

            if (!createResult.Success || createResult.Data == null)
            {
                return createResult;
            }

            if (!request.AutoSubmit)
            {
                return createResult;
            }

            var submitResult = await workflowService.SubmitAsync(
                createResult.Data.Id,
                new WorkflowSubmitRequest
                {
                    Comment = request.SubmitComment,
                    IdempotencyKey = string.IsNullOrWhiteSpace(request.IdempotencyKey)
                        ? null
                        : $"{request.IdempotencyKey}:submit"
                },
                cancellationToken);

            return submitResult;
        }

        public async Task<WorkflowReferenceLifecycleSynchronizationResult> SynchronizeAsync(
            TrxWorkflowInstance workflow,
            Guid actorUserId,
            bool allowAutoApply = true,
            CancellationToken cancellationToken = default)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException(nameof(workflow));
            }

            if (!IsRecognizedReferenceType(workflow.ReferenceType))
            {
                return new WorkflowReferenceLifecycleSynchronizationResult
                {
                    IsHandled = false,
                    WorkflowInstanceId = workflow.Id,
                    ReferenceId = workflow.ReferenceId,
                    WorkflowStatus = workflow.WorkflowStatus,
                    WarningMessage = $"Jenis rujukan {workflow.ReferenceType} tidak dikenali oleh Workforce Planning."
                };
            }

            // 1. Pemeriksaan Idempotensi: jika status dokumen sumber sudah final, abaikan keputusan berulang
            var isAlreadyFinal = await CheckIfDocumentAlreadyFinalAsync(workflow.ReferenceType, workflow.ReferenceId, cancellationToken);
            if (isAlreadyFinal.AlreadyFinal)
            {
                _logger.LogInformation(
                    "Keputusan workflow {WorkflowInstanceId} untuk rujukan {ReferenceType} {ReferenceId} diabaikan karena status dokumen sumber sudah final: {CurrentStatus}.",
                    workflow.Id,
                    workflow.ReferenceType,
                    workflow.ReferenceId,
                    isAlreadyFinal.CurrentStatus);

                return new WorkflowReferenceLifecycleSynchronizationResult
                {
                    IsHandled = true,
                    WorkflowInstanceId = workflow.Id,
                    ReferenceId = workflow.ReferenceId,
                    PreviousReferenceStatus = isAlreadyFinal.CurrentStatus,
                    CurrentReferenceStatus = isAlreadyFinal.CurrentStatus,
                    WorkflowStatus = workflow.WorkflowStatus,
                    StatusChanged = false,
                    AutoApplyAttempted = false,
                    AutoApplySucceeded = false
                };
            }

            // 2. Cari penerap keputusan domain yang terdaftar
            var applier = _decisionAppliers.FirstOrDefault(x =>
                string.Equals(x.ReferenceType, workflow.ReferenceType, StringComparison.OrdinalIgnoreCase));

            if (applier == null)
            {
                // Acceptance criteria 2: Keputusan untuk jenis rujukan yang belum punya penerap dicatat sebagai galat dan dokumen tidak berubah — bukan crash.
                _logger.LogError(
                    "Penerap keputusan domain belum terdaftar untuk jenis rujukan {ReferenceType} (WorkflowInstanceId: {WorkflowInstanceId}, ReferenceId: {ReferenceId}). Dokumen tidak berubah.",
                    workflow.ReferenceType,
                    workflow.Id,
                    workflow.ReferenceId);

                return new WorkflowReferenceLifecycleSynchronizationResult
                {
                    IsHandled = false,
                    WorkflowInstanceId = workflow.Id,
                    ReferenceId = workflow.ReferenceId,
                    WorkflowStatus = workflow.WorkflowStatus,
                    WarningMessage = $"Penerap keputusan domain belum terdaftar untuk jenis rujukan {workflow.ReferenceType}."
                };
            }

            // 3. Delegasikan ke penerap keputusan domain
            var context = new WorkforcePlanningDecisionContext
            {
                WorkflowInstanceId = workflow.Id,
                ReferenceId = workflow.ReferenceId,
                ReferenceType = workflow.ReferenceType,
                WorkflowStatus = workflow.WorkflowStatus,
                ActorUserId = actorUserId,
                AllowAutoApply = allowAutoApply
            };

            var applyResult = await applier.ApplyDecisionAsync(context, cancellationToken);

            return new WorkflowReferenceLifecycleSynchronizationResult
            {
                IsHandled = applyResult.IsHandled,
                WorkflowInstanceId = workflow.Id,
                ReferenceId = workflow.ReferenceId,
                PreviousReferenceStatus = applyResult.PreviousStatus,
                CurrentReferenceStatus = applyResult.CurrentStatus,
                WorkflowStatus = workflow.WorkflowStatus,
                StatusChanged = applyResult.StatusChanged,
                AutoApplyAttempted = applyResult.AutoApplyAttempted,
                AutoApplySucceeded = applyResult.AutoApplySucceeded,
                WarningMessage = applyResult.WarningMessage
            };
        }

        private static bool IsRecognizedReferenceType(string? referenceType)
        {
            if (string.IsNullOrWhiteSpace(referenceType))
            {
                return false;
            }

            var trimmed = referenceType.Trim();
            return string.Equals(trimmed, WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceStaffingStandard, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(trimmed, WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceInitialFormationBatch, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(trimmed, WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceManpowerPlan, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(trimmed, WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceHeadcountRequest, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(trimmed, WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceHeadcountCancellation, StringComparison.OrdinalIgnoreCase);
        }

        private async Task<(bool AlreadyFinal, string CurrentStatus)> CheckIfDocumentAlreadyFinalAsync(
            string referenceType,
            Guid referenceId,
            CancellationToken cancellationToken)
        {
            var trimmed = referenceType.Trim();

            if (string.Equals(trimmed, WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceStaffingStandard, StringComparison.OrdinalIgnoreCase))
            {
                var doc = await _dbContext.HrdStaffingStandards
                    .AsNoTracking()
                    .Where(x => x.Id == referenceId && !x.IsDelete)
                    .Select(x => new { x.StandardStatus })
                    .FirstOrDefaultAsync(cancellationToken);

                if (doc != null)
                {
                    var isFinal = doc.StandardStatus is StaffingStandardStatus.Active
                                                     or StaffingStandardStatus.Scheduled
                                                     or StaffingStandardStatus.Superseded
                                                     or StaffingStandardStatus.Rejected
                                                     or StaffingStandardStatus.Cancelled;
                    return (isFinal, doc.StandardStatus.ToString());
                }
            }
            else if (string.Equals(trimmed, WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceInitialFormationBatch, StringComparison.OrdinalIgnoreCase))
            {
                var doc = await _dbContext.HrdInitialFormationBatches
                    .AsNoTracking()
                    .Where(x => x.Id == referenceId && !x.IsDelete)
                    .Select(x => new { x.BatchStatus })
                    .FirstOrDefaultAsync(cancellationToken);

                if (doc != null)
                {
                    var isFinal = doc.BatchStatus is InitialFormationBatchStatus.Applied
                                                  or InitialFormationBatchStatus.Rejected
                                                  or InitialFormationBatchStatus.Cancelled;
                    return (isFinal, doc.BatchStatus.ToString());
                }
            }
            else if (string.Equals(trimmed, WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceManpowerPlan, StringComparison.OrdinalIgnoreCase))
            {
                var doc = await _dbContext.HrdManpowerPlans
                    .AsNoTracking()
                    .Where(x => x.Id == referenceId && !x.IsDelete)
                    .Select(x => new { x.PlanStatus })
                    .FirstOrDefaultAsync(cancellationToken);

                if (doc != null)
                {
                    var isFinal = doc.PlanStatus is ManpowerPlanStatus.Approved
                                                 or ManpowerPlanStatus.Rejected
                                                 or ManpowerPlanStatus.Cancelled;
                    return (isFinal, doc.PlanStatus.ToString());
                }
            }
            else if (string.Equals(trimmed, WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceHeadcountRequest, StringComparison.OrdinalIgnoreCase))
            {
                var doc = await _dbContext.HrdHeadcountRequests
                    .AsNoTracking()
                    .Where(x => x.Id == referenceId && !x.IsDelete)
                    .Select(x => new { x.RequestStatus })
                    .FirstOrDefaultAsync(cancellationToken);

                if (doc != null)
                {
                    var isFinal = doc.RequestStatus is HeadcountRequestStatus.ReadyForRecruitment
                                                    or HeadcountRequestStatus.PartiallyFulfilled
                                                    or HeadcountRequestStatus.Fulfilled
                                                    or HeadcountRequestStatus.Closed
                                                    or HeadcountRequestStatus.Rejected
                                                    or HeadcountRequestStatus.Withdrawn;
                    return (isFinal, doc.RequestStatus.ToString());
                }
            }
            else if (string.Equals(trimmed, WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceHeadcountCancellation, StringComparison.OrdinalIgnoreCase))
            {
                var doc = await _dbContext.HrdHeadcountRequestCancellations
                    .AsNoTracking()
                    .Where(x => x.Id == referenceId && !x.IsDelete)
                    .Select(x => new { x.CancellationStatus })
                    .FirstOrDefaultAsync(cancellationToken);

                if (doc != null)
                {
                    var isFinal = doc.CancellationStatus is HeadcountCancellationStatus.Approved
                                                          or HeadcountCancellationStatus.Rejected;
                    return (isFinal, doc.CancellationStatus.ToString());
                }
            }

            return (false, string.Empty);
        }
    }
}
