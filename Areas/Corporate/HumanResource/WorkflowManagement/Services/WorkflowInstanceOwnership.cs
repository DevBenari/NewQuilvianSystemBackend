using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    /// <summary>
    /// BE-HRD-447 (kontrak v29 VR-WF-29, HRD-DEC-229/235): membedakan permohonan milik modul (instance engine
    /// <see cref="WorkflowService"/>) dari instance engine V2 lama (endpoint generik <c>POST /api/v1/hrd/workflow/instances</c>).
    /// Instance engine lama dibuat tanpa <c>WorkflowDefinitionId</c> dan penugasannya mengisi <c>ApproverEmployeeId</c>;
    /// engine tidak pernah mengisi <c>ApproverEmployeeId</c> (HRD-TF-081).
    /// </summary>
    public static class WorkflowInstanceOwnership
    {
        public const string CreateFromModuleMessage = "Permohonan dibuat dari layar modul (Cuti, Lembur, SPPD, dan seterusnya).";
        public const string CancelFromModuleMessage = "Batalkan permohonan dari layar modulnya.";
        public const string ResubmitFromModuleMessage = "Perbaiki dan ajukan ulang dari layar modulnya.";

        /// <summary>Instance milik modul: punya definisi workflow dan tidak punya penugasan engine lama.</summary>
        public static readonly Expression<Func<TrxWorkflowInstance, bool>> IsModuleManagedExpression = w =>
            w.WorkflowDefinitionId != Guid.Empty &&
            !w.StepInstances.Any(s => s.ApproverAssignments.Any(a => a.ApproverEmployeeId != null));

        public static Task<bool> IsModuleManagedAsync(
            DbContext dbContext,
            Guid workflowInstanceId,
            CancellationToken cancellationToken = default)
        {
            return dbContext.Set<TrxWorkflowInstance>()
                .AsNoTracking()
                .Where(x => x.Id == workflowInstanceId)
                .Where(IsModuleManagedExpression)
                .AnyAsync(cancellationToken);
        }
    }

    /// <summary>Permohonan milik modul diubah lewat endpoint generik; controller menjawab <c>409 Conflict</c>.</summary>
    public sealed class WorkflowModuleManagedException(string message) : Exception(message);
}
