using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services
{
    /// <summary>
    /// Penerap status satu jenis dokumen Pengadaan dari status workflow (BE-PBJ-005). Didaftarkan
    /// lewat DI oleh task pemilik dokumennya (BE-PBJ-008 untuk PR, BE-PBJ-009 untuk rencana, dst.).
    /// </summary>
    /// <remarks>
    /// Penerap wajib idempoten: status dokumen diturunkan ulang dari status workflow saat ini
    /// (<c>state-transition-matrix.md</c> bagian 5), sehingga pemanggilan kedua tidak mengubah apa
    /// pun dan tidak menambah riwayat ganda.
    /// </remarks>
    public interface IProcurementWorkflowStatusApplier
    {
        string DocumentType { get; }

        Task<WorkflowReferenceLifecycleSynchronizationResult> ApplyAsync(
            TrxWorkflowInstance workflow,
            Guid actorUserId,
            CancellationToken cancellationToken);
    }

    /// <summary>
    /// Penangan lifecycle Wfl untuk seluruh dokumen Pengadaan (<c>ReferenceType</c> berawalan
    /// <c>PBJ_</c>), terdaftar sebagai <see cref="IWorkflowReferenceLifecycleHandler"/> (BE-PBJ-001/005).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Penangan meneruskan ke <see cref="IProcurementWorkflowStatusApplier"/> milik jenis dokumennya.
    /// Jenis dokumen tanpa penerap dijawab <c>IsHandled = false</c> tanpa efek, sama seperti perilaku
    /// Wfl untuk jenis yang tidak dikenal.
    /// </para>
    /// <para>
    /// <b>Pemeriksaan ulang anti-setujui-sendiri (PBJ-AC-002).</b> Sebelum status disetujui
    /// diterapkan, penangan memastikan tidak ada aksi <c>Approve</c> oleh pemohon instance. Wfl sudah
    /// menjaga ini (<c>AllowSelfApproval = false</c>); pemeriksaan ini lapisan kedua. Bila ditemukan,
    /// status dokumen tidak diubah dan kejadiannya dicatat sebagai galat.
    /// </para>
    /// </remarks>
    public sealed class ProcurementWorkflowLifecycleHandler : IWorkflowReferenceLifecycleHandler
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IReadOnlyList<IProcurementWorkflowStatusApplier> _appliers;
        private readonly ILogger<ProcurementWorkflowLifecycleHandler> _logger;

        public ProcurementWorkflowLifecycleHandler(
            ApplicationDbContext dbContext,
            IEnumerable<IProcurementWorkflowStatusApplier> appliers,
            ILogger<ProcurementWorkflowLifecycleHandler> logger)
        {
            _dbContext = dbContext;
            _appliers = appliers.ToList();
            _logger = logger;
        }

        public bool CanHandle(string referenceType) =>
            !string.IsNullOrWhiteSpace(referenceType) &&
            referenceType.Trim().StartsWith(ProcurementDocumentTypes.Prefix, StringComparison.Ordinal);

        public async Task<WorkflowReferenceLifecycleSynchronizationResult> SynchronizeAsync(
            TrxWorkflowInstance workflow,
            Guid actorUserId,
            bool allowAutoApply,
            CancellationToken cancellationToken)
        {
            var referenceType = workflow.ReferenceType.Trim();
            var applier = _appliers.FirstOrDefault(x => string.Equals(x.DocumentType, referenceType, StringComparison.Ordinal));
            if (applier == null)
            {
                return NotHandled(workflow);
            }

            if (IsApproved(workflow.WorkflowStatus))
            {
                var approvedByRequester = await _dbContext.Set<TrxApprovalAction>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.WorkflowInstanceId == workflow.Id &&
                        !x.IsDelete &&
                        x.ActionType == WorkflowValueConstants.ActionType.Approve &&
                        x.ActualActionByUserId == workflow.RequestedByUserId,
                        cancellationToken);

                if (approvedByRequester)
                {
                    _logger.LogError(
                        "Workflow Pengadaan {WorkflowInstanceId} disetujui oleh pemohonnya sendiri; status dokumen tidak diterapkan.",
                        workflow.Id);
                    return new WorkflowReferenceLifecycleSynchronizationResult
                    {
                        IsHandled = true,
                        WorkflowInstanceId = workflow.Id,
                        ReferenceId = workflow.ReferenceId,
                        WorkflowStatus = workflow.WorkflowStatus,
                        StatusChanged = false
                    };
                }
            }

            return await applier.ApplyAsync(workflow, actorUserId, cancellationToken);
        }

        private static bool IsApproved(string? status) =>
            string.Equals(status, WorkflowValueConstants.WorkflowStatus.Approved, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, WorkflowValueConstants.WorkflowStatus.Completed, StringComparison.OrdinalIgnoreCase);

        private static WorkflowReferenceLifecycleSynchronizationResult NotHandled(TrxWorkflowInstance workflow) => new()
        {
            IsHandled = false,
            WorkflowInstanceId = workflow.Id,
            ReferenceId = workflow.ReferenceId,
            WorkflowStatus = workflow.WorkflowStatus
        };
    }

    /// <summary>
    /// Pemetaan status Wfl → status dokumen Pengadaan (<c>state-transition-matrix.md</c> bagian 5).
    /// Dipakai para penerap status supaya pemetaannya satu sumber.
    /// </summary>
    public static class ProcurementWorkflowStatusMap
    {
        /// <summary>
        /// Contoh: Wfl <c>Approved</c> → PR <c>APPROVED</c>; Wfl <c>RevisionRequested</c> → PR
        /// <c>RETURNED</c>; Wfl <c>Withdrawn</c> → PR <c>CANCELLED</c>, tetapi rencana/pagu
        /// <c>RETURNED</c>. <c>null</c> berarti status Wfl tidak memindahkan dokumen.
        /// </summary>
        public static string? ToDocumentStatus(string? workflowStatus, bool isPurchaseRequest)
        {
            var status = workflowStatus?.Trim() ?? string.Empty;
            if (Eq(status, WorkflowValueConstants.WorkflowStatus.Submitted) || Eq(status, WorkflowValueConstants.WorkflowStatus.InProgress))
                return isPurchaseRequest ? ProcurementStatuses.PurchaseRequest.PendingApproval : ProcurementStatuses.Plan.PendingApproval;
            if (Eq(status, WorkflowValueConstants.WorkflowStatus.Approved) || Eq(status, WorkflowValueConstants.WorkflowStatus.Completed))
                return isPurchaseRequest ? ProcurementStatuses.PurchaseRequest.Approved : ProcurementStatuses.Plan.Approved;
            if (Eq(status, WorkflowValueConstants.WorkflowStatus.Rejected))
                return isPurchaseRequest ? ProcurementStatuses.PurchaseRequest.Rejected : ProcurementStatuses.Plan.Rejected;
            if (Eq(status, WorkflowValueConstants.WorkflowStatus.Returned) || Eq(status, WorkflowValueConstants.WorkflowStatus.RevisionRequested))
                return isPurchaseRequest ? ProcurementStatuses.PurchaseRequest.Returned : ProcurementStatuses.Plan.Returned;
            if (Eq(status, WorkflowValueConstants.WorkflowStatus.Withdrawn) || Eq(status, WorkflowValueConstants.WorkflowStatus.Cancelled))
                return isPurchaseRequest ? ProcurementStatuses.PurchaseRequest.Cancelled : ProcurementStatuses.Plan.Returned;
            return null;
        }

        private static bool Eq(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}
