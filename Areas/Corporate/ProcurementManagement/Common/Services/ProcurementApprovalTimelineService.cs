using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services
{
    /// <summary>
    /// Linimasa persetujuan <c>Wfl</c> satu dokumen Pengadaan (tab Persetujuan, <c>03-frontend-architecture.md</c>
    /// §4.2: "<c>ApprovalInbox : Read</c> atau baca PR").
    /// </summary>
    /// <remarks>
    /// Cakupan dokumen diperiksa lewat <see cref="IProcurementDocumentAccessProvider"/> yang sama dengan lampiran:
    /// dokumen di luar cakupan dijawab 404. Instance dibaca lewat <see cref="WorkflowService.GetByIdAsync"/>
    /// (API milik modul <c>Wfl</c>), bukan langsung dari tabel <c>Wfl</c>.
    /// </remarks>
    public sealed class ProcurementApprovalTimelineService
    {
        private readonly IReadOnlyList<IProcurementDocumentAccessProvider> _accessProviders;
        private readonly WorkflowService _workflowService;
        private readonly ApplicationDbContext _dbContext;

        public ProcurementApprovalTimelineService(
            IEnumerable<IProcurementDocumentAccessProvider> accessProviders,
            WorkflowService workflowService,
            ApplicationDbContext dbContext)
        {
            _accessProviders = accessProviders.ToList();
            _workflowService = workflowService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// <c>null</c> bila dokumen belum pernah masuk persetujuan. Dokumen tidak ada atau di luar cakupan
        /// melempar <see cref="KeyNotFoundException"/> (404).
        /// </summary>
        public async Task<ProcurementApprovalTimelineResponse?> GetAsync(
            ClaimsPrincipal user, string documentType, Guid documentId, CancellationToken cancellationToken)
        {
            var provider = _accessProviders.FirstOrDefault(x => x.DocumentType == documentType)
                ?? throw new KeyNotFoundException("Dokumen tidak ditemukan.");
            var access = await provider.GetAccessAsync(user, documentId, cancellationToken);
            if (access == ProcurementDocumentAccessLevel.None)
                throw new KeyNotFoundException("Dokumen tidak ditemukan atau bukan milik unit Anda.");

            var workflowInstanceId = await GetWorkflowInstanceIdAsync(documentType, documentId, cancellationToken);
            if (!workflowInstanceId.HasValue) return null;

            var result = await _workflowService.GetByIdAsync(workflowInstanceId.Value, cancellationToken);
            if (!result.Success || result.Data == null) return null;

            var instance = result.Data;
            var actionsByStep = instance.ApprovalActions
                .Where(x => x.WorkflowStepInstanceId.HasValue)
                .GroupBy(x => x.WorkflowStepInstanceId!.Value)
                .ToDictionary(x => x.Key, x => x.OrderBy(a => a.ActionAt).ToList());

            return new ProcurementApprovalTimelineResponse
            {
                WorkflowInstanceId = instance.Id,
                RequestNumber = instance.RequestNumber,
                WorkflowName = instance.WorkflowName,
                WorkflowStatus = instance.WorkflowStatus,
                SubmittedAt = instance.SubmittedAt,
                CompletedAt = instance.CompletedAt,
                Steps = instance.Steps
                    .OrderBy(x => x.StepOrder)
                    .Select(step => new ProcurementApprovalStepResponse
                    {
                        Id = step.Id,
                        StepOrder = step.StepOrder,
                        StepName = step.StepName,
                        StepStatus = step.StepStatus,
                        DueAt = step.DueAt,
                        CompletedAt = step.CompletedAt,
                        Actions = (actionsByStep.TryGetValue(step.Id, out var actions) ? actions : new())
                            .Select(action => new ProcurementApprovalActionResponse
                            {
                                Id = action.Id,
                                ActionType = action.ActionType,
                                ActionAt = action.ActionAt,
                                ActorName = action.ActualActionByName,
                                IsSystemAction = action.IsSystemAction,
                                Comment = action.Comment,
                                ReasonName = action.ActionReasonName
                            })
                            .ToList()
                    })
                    .ToList()
            };
        }

        private Task<Guid?> GetWorkflowInstanceIdAsync(string documentType, Guid documentId, CancellationToken cancellationToken) =>
            documentType switch
            {
                ProcurementDocumentTypes.PurchaseRequest => _dbContext.PrcPurchaseRequests.AsNoTracking()
                    .Where(x => x.Id == documentId && !x.IsDelete).Select(x => x.WorkflowInstanceId).FirstOrDefaultAsync(cancellationToken),
                ProcurementDocumentTypes.NeedPlan => _dbContext.PrcNeedPlans.AsNoTracking()
                    .Where(x => x.Id == documentId && !x.IsDelete).Select(x => x.WorkflowInstanceId).FirstOrDefaultAsync(cancellationToken),
                ProcurementDocumentTypes.Budget => _dbContext.PrcBudgets.AsNoTracking()
                    .Where(x => x.Id == documentId && !x.IsDelete).Select(x => x.WorkflowInstanceId).FirstOrDefaultAsync(cancellationToken),
                ProcurementDocumentTypes.PackagePlan => _dbContext.PrcPackagePlans.AsNoTracking()
                    .Where(x => x.Id == documentId && !x.IsDelete).Select(x => x.WorkflowInstanceId).FirstOrDefaultAsync(cancellationToken),
                // BE-PBJ-VD-007: usulan blokir/buka blokir vendor.
                ProcurementDocumentTypes.VendorStatusRequest => _dbContext.PrcVendorStatusRequests.AsNoTracking()
                    .Where(x => x.Id == documentId && !x.IsDelete).Select(x => x.WorkflowInstanceId).FirstOrDefaultAsync(cancellationToken),
                // BE-PBJ-PS-008: persiapan paket (persetujuan penetapan memakai PBJ_PACKAGE_AWARD, BE-PBJ-PS-013).
                ProcurementDocumentTypes.Package => _dbContext.PrcPackages.AsNoTracking()
                    .Where(x => x.Id == documentId && !x.IsDelete).Select(x => x.PreparationWorkflowInstanceId).FirstOrDefaultAsync(cancellationToken),
                _ => Task.FromResult<Guid?>(null)
            };
    }
}
