using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Layanan Transmisi Sinkronisasi Wewenang Klinis ke Modul SIMRS EMR (INT-HR-76, VR-OPP-21, VR-OPP-22, VR-OPP-25)
    /// </summary>
    public interface ISimrsClinicalPrivilegeDispatcher
    {
        /// <summary>
        /// Mengirim instruksi pembaruan wewenang Level 1-4 ke SIMRS EMR dengan penanganan Hard-Stop jika status Revoked
        /// </summary>
        Task<SimrsPrivilegeSyncAuditLogDto> DispatchToSimrsAsync(Guid verdictId, DispatchSimrsPrivilegeRequestDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil log riwayat pengiriman transmisi sinkronisasi ke SIMRS EMR
        /// </summary>
        Task<List<SimrsPrivilegeSyncAuditLogDto>> GetSyncAuditLogsAsync(CancellationToken cancellationToken = default);
    }
}
