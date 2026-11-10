using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Antarmuka layanan buku formasi jabatan berversi (BE-HRD-032).
    /// Mengelola pembacaan formasi berlaku, riwayat versi, serta menjadi satu-satunya jalur
    /// penulis versi formasi baru di sistem di bawah advisory lock PostgreSQL.
    /// </summary>
    public interface IHrdPositionFormationService
    {
        /// <summary>
        /// Mengambil daftar formasi yang berlaku pada suatu tanggal acuan (default hari ini),
        /// dengan mendukung rule FORMATION_CARRIES_FORWARD dan pembatasan cakupan unit Kepala Unit (VR-WFP-38).
        /// </summary>
        Task<WorkforcePlanningResult<PagedResult<PositionFormationResponse>>> GetEffectiveFormationsAsync(
            PositionFormationPagedQuery query,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil seluruh riwayat versi formasi satu unit × jabatan beserta dokumen sumber, penyetuju, dan waktu persetujuan.
        /// </summary>
        Task<WorkforcePlanningResult<List<PositionFormationResponse>>> GetFormationHistoryAsync(
            Guid organizationUnitId,
            Guid positionId,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil satu formasi yang sah berlaku untuk unit × jabatan pada suatu tanggal tertentu (helper domain service).
        /// </summary>
        Task<HrdPositionFormation?> GetEffectiveFormationForPositionAsync(
            Guid organizationUnitId,
            Guid positionId,
            DateOnly date,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Satu-satunya metode untuk menulis versi formasi baru di sistem (NO_DIRECT_FORMATION_EDIT).
        /// Wajib dieksekusi di dalam transaksi aktif pemanggil (NFR-010, NFR-011) di bawah advisory lock PostgreSQL.
        /// Versi lama ditutup pada tanggal sehari sebelum effectiveStartDate (EffectiveEndDate = effectiveStartDate - 1 hari),
        /// dan versi baru dibuat dengan EffectiveEndDate = null (terbuka).
        /// </summary>
        Task<HrdPositionFormation> CreateNewVersionAsync(
            Guid hospitalSiteId,
            Guid organizationUnitId,
            Guid positionId,
            decimal formationQuantity,
            DateOnly effectiveStartDate,
            PositionFormationSourceType sourceType,
            Guid sourceDocumentId,
            string sourceDocumentNumber,
            Guid approvedByUserId,
            DateTime approvedAt,
            CancellationToken cancellationToken = default);
    }
}
