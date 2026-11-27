using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    /// <summary>
    /// BE-HRD-441 (kontrak v29, HRD-DEC-234, VR-WF-30, INT-HR-113): penentu delegasi (Plh) aktif yang dipakai bersama oleh engine
    /// <see cref="WorkflowService"/> (otorisasi aksi dan pengalihan saat penugasan dibuat) dan adapter Kotak Masuk Terpadu.
    /// Delegasi aktif = status <c>Active</c>/<c>Approved</c>, aktif, tidak dihapus/dibatalkan, dalam rentang
    /// <c>EffectiveStartAt</c>–<c>EffectiveEndAt</c>, dan cakupannya (semua workflow, definisi, tahap) cocok.
    /// </summary>
    public static class WorkflowDelegationResolver
    {
        /// <summary>Delegasi berstatus aktif pada waktu <paramref name="now"/>.</summary>
        public static IQueryable<TrxApprovalDelegation> WhereActiveAt(
            this IQueryable<TrxApprovalDelegation> query,
            DateTime now)
        {
            return query.Where(x =>
                !x.IsDelete &&
                !x.IsCancel &&
                x.IsActive &&
                (x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Active ||
                 x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Approved) &&
                x.EffectiveStartAt <= now &&
                x.EffectiveEndAt >= now);
        }

        /// <summary>Delegasi yang cakupannya meliputi definisi dan tahap workflow tertentu.</summary>
        public static IQueryable<TrxApprovalDelegation> WhereCovers(
            this IQueryable<TrxApprovalDelegation> query,
            Guid workflowDefinitionId,
            Guid workflowStepId)
        {
            return query.Where(x =>
                (x.AppliesToAllWorkflows ||
                 !x.WorkflowDefinitionId.HasValue ||
                 x.WorkflowDefinitionId == workflowDefinitionId) &&
                (!x.WorkflowStepId.HasValue || x.WorkflowStepId == workflowStepId));
        }

        /// <summary>Urutan pilihan bila lebih dari satu delegasi cocok: paling spesifik, lalu paling baru.</summary>
        public static IOrderedQueryable<TrxApprovalDelegation> OrderBySpecificity(
            this IQueryable<TrxApprovalDelegation> query)
        {
            return query
                .OrderByDescending(x => x.WorkflowStepId.HasValue)
                .ThenByDescending(x => x.WorkflowDefinitionId.HasValue)
                .ThenByDescending(x => x.EffectiveStartAt);
        }

        /// <summary>
        /// Delegasi aktif yang membuat <paramref name="delegateUserId"/> boleh bertindak atas nama
        /// <paramref name="principalUserId"/> untuk definisi dan tahap tersebut, atau <c>null</c>.
        /// </summary>
        public static Task<TrxApprovalDelegation?> FindActingDelegationAsync(
            DbContext dbContext,
            Guid principalUserId,
            Guid delegateUserId,
            Guid workflowDefinitionId,
            Guid workflowStepId,
            DateTime now,
            CancellationToken cancellationToken = default)
        {
            if (principalUserId == Guid.Empty || delegateUserId == Guid.Empty || principalUserId == delegateUserId)
            {
                return Task.FromResult<TrxApprovalDelegation?>(null);
            }

            return dbContext.Set<TrxApprovalDelegation>()
                .AsNoTracking()
                .WhereActiveAt(now)
                .WhereCovers(workflowDefinitionId, workflowStepId)
                .Where(x => x.DelegatorUserId == principalUserId && x.DelegateUserId == delegateUserId)
                .OrderBySpecificity()
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
