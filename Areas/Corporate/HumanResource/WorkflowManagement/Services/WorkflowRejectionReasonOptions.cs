using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    /// <summary>
    /// BE-HRD-443 (kontrak v29 §35.6): pilihan alasan penolakan aktif untuk satu jenis permohonan, definisi, dan tahap workflow.
    /// Dipakai bersama detail penugasan Kotak Masuk lama (<c>ApprovalInboxService</c>) dan blok <c>myAction</c> Kotak Masuk
    /// Terpadu, sehingga keduanya menampilkan alasan yang sama dengan yang diterima engine saat Tolak (VR-WF-26).
    /// </summary>
    public static class WorkflowRejectionReasonOptions
    {
        public static Task<List<ApprovalInboxRejectionReasonOptionResponse>> QueryAsync(
            DbContext dbContext,
            string? requestType,
            Guid workflowDefinitionId,
            Guid? workflowStepId,
            DateTime today,
            CancellationToken cancellationToken = default)
        {
            return dbContext.Set<MstRejectionReason>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.RequestType == requestType &&
                    (!x.WorkflowDefinitionId.HasValue ||
                     x.WorkflowDefinitionId == workflowDefinitionId) &&
                    (!x.WorkflowStepId.HasValue ||
                     x.WorkflowStepId == workflowStepId) &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= today) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today))
                .OrderByDescending(x => x.WorkflowStepId.HasValue)
                .ThenByDescending(x => x.WorkflowDefinitionId.HasValue)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.ReasonName)
                .Select(x => new ApprovalInboxRejectionReasonOptionResponse
                {
                    Id = x.Id,
                    ReasonCode = x.ReasonCode,
                    ReasonName = x.ReasonName,
                    ReasonCategory = x.ReasonCategory,
                    RejectAction = x.RejectAction,
                    ReturnToStepCode = x.ReturnToStepCode,
                    IsCommentRequired = x.IsCommentRequired,
                    IsAttachmentRequired = x.IsAttachmentRequired,
                    AllowResubmit = x.AllowResubmit
                })
                .ToListAsync(cancellationToken);
        }
    }
}
