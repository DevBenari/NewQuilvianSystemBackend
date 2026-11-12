using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public interface IHrdEmploymentLifecycleWorkflowIntegrationService
    {
        Task<Guid?> StartProbationAssessmentWorkflowAsync(Guid assessmentId, Guid submitterUserId, CancellationToken cancellationToken = default);
        Task<Guid?> StartContractDecisionWorkflowAsync(Guid decisionId, Guid submitterUserId, CancellationToken cancellationToken = default);
        Task<Guid?> StartContractCorrectionWorkflowAsync(Guid correctionId, Guid submitterUserId, CancellationToken cancellationToken = default);
        Task<Guid?> StartInitialContractBatchWorkflowAsync(Guid batchId, Guid submitterUserId, CancellationToken cancellationToken = default);
        Task<bool> HandleWorkflowCompletedAsync(Guid workflowInstanceId, bool isApproved, Guid deciderUserId, string? decisionNote, CancellationToken cancellationToken = default);
    }
}
