using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services
{
    public interface IBusinessTravelWorkflowReconciliationService
    {
        /// <summary>
        /// Menyelaraskan ulang SPPD yang statusnya tertinggal dari workflow persetujuannya. Mengembalikan jumlah
        /// SPPD yang statusnya berubah.
        /// </summary>
        Task<int> ReconcileAsync(Guid actorUserId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Penyelarasan ulang SPPD ↔ Workflow V2 (keputusan pengguna 19 November 2026, pilihan (a); BE-HRD-376).
    /// </summary>
    /// <remarks>
    /// Workflow V2 memanggil <see cref="BusinessTravelWorkflowLifecycleHandler"/> sesudah aksi tersimpan dan hanya
    /// mencatat galatnya. Bila penangan gagal (mis. basis data terputus sesaat), SPPD dapat tertinggal: workflow sudah
    /// disetujui tetapi SPPD masih menunggu. Layanan ini mencari SPPD dalam fase persetujuan yang status workflow-nya
    /// memetakan ke status lain, lalu menjalankan penangan yang sama (idempoten). Contoh: SPPD Sari masih
    /// "Menunggu Persetujuan" padahal Kepala Ruang sudah menyetujui → pada putaran berikutnya SPPD menjadi
    /// "Disetujui" dan presensi Dinas Luar terpasang.
    /// </remarks>
    public sealed class BusinessTravelWorkflowReconciliationService : IBusinessTravelWorkflowReconciliationService
    {
        private static readonly string[] ApprovalPhase =
        {
            BusinessTravelRequestStatuses.Draft,
            BusinessTravelRequestStatuses.Submitted,
            BusinessTravelRequestStatuses.NeedRevision
        };

        private readonly ApplicationDbContext _context;
        private readonly BusinessTravelWorkflowLifecycleHandler _handler;
        private readonly ILogger<BusinessTravelWorkflowReconciliationService> _logger;

        public BusinessTravelWorkflowReconciliationService(
            ApplicationDbContext context,
            BusinessTravelWorkflowLifecycleHandler handler,
            ILogger<BusinessTravelWorkflowReconciliationService> logger)
        {
            _context = context;
            _handler = handler;
            _logger = logger;
        }

        public async Task<int> ReconcileAsync(Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var candidates = await _context.TrxBusinessTravelRequests
                .AsNoTracking()
                .Where(r => r.IsActive && !r.IsDelete && r.WorkflowInstanceId != null && ApprovalPhase.Contains(r.TravelStatus))
                .Join(_context.Set<TrxWorkflowInstance>().AsNoTracking().Where(w => !w.IsDelete),
                    r => r.WorkflowInstanceId, w => w.Id, (r, w) => new { r.TravelStatus, Workflow = w })
                .ToListAsync(cancellationToken);

            var changed = 0;
            foreach (var item in candidates)
            {
                var target = BusinessTravelWorkflowStatusMap.ToTravelStatus(item.Workflow.WorkflowStatus);
                if (target == null || target == item.TravelStatus)
                {
                    continue;
                }

                try
                {
                    var result = await _handler.SynchronizeAsync(item.Workflow, actorUserId, allowAutoApply: false, cancellationToken);
                    if (result.StatusChanged) changed++;
                }
                catch (Exception ex)
                {
                    _context.ChangeTracker.Clear();
                    _logger.LogError(ex, "[TRV-WF] Penyelarasan ulang SPPD {ReferenceId} (workflow {WorkflowInstanceId}) gagal.",
                        item.Workflow.ReferenceId, item.Workflow.Id);
                }
            }

            return changed;
        }
    }
}
