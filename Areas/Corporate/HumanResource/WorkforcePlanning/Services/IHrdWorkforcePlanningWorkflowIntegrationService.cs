using System.Text.Json;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Kontrak layanan integrasi persetujuan S-D1 Perencanaan Tenaga Kerja (Workforce Planning)
    /// dengan mesin workflow terpadu rumah sakit.
    /// </summary>
    public interface IHrdWorkforcePlanningWorkflowIntegrationService
    {
        /// <summary>
        /// Memulai instans workflow untuk dokumen perencanaan tenaga kerja (T9 s.d. T13).
        /// </summary>
        Task<WorkflowServiceResult<WorkflowInstanceDetailResponse>> StartWorkflowAsync(
            StartWorkforcePlanningWorkflowRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Menerima dan menyinkronkan keputusan akhir workflow dari WorkflowReferenceLifecycleService.
        /// </summary>
        Task<WorkflowReferenceLifecycleSynchronizationResult> SynchronizeAsync(
            TrxWorkflowInstance workflow,
            Guid actorUserId,
            bool allowAutoApply = true,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Parameter permohonan pembuatan instans workflow dokumen Workforce Planning.
    /// </summary>
    public class StartWorkforcePlanningWorkflowRequest
    {
        public string WorkflowDefinitionCode { get; set; } = string.Empty;
        public string ReferenceType { get; set; } = string.Empty;
        public Guid ReferenceId { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string? SourceChannel { get; set; } = "Web";
        public string? RequestCorrelationId { get; set; }
        public string? IdempotencyKey { get; set; }
        public JsonElement? RequestContext { get; set; }
        public IReadOnlyCollection<Guid>? SelectedApproverUserIds { get; set; }
        public string? SubmitComment { get; set; }
        public bool AutoSubmit { get; set; } = true;
    }
}
