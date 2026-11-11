using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Antarmuka layanan pencatatan dan pembacaan potret gap kebutuhan tenaga kerja (BE-HRD-039).
    /// </summary>
    public interface IHrdStaffingGapSnapshotService
    {
        /// <summary>
        /// Membuat potret gap saat pengajuan dokumen (Manpower Plan atau Headcount Request).
        /// Menyimpan data di dalam transaksi yang sama dengan dokumen pemanggil (kriteria 1).
        /// </summary>
        Task<Guid> CaptureSubmissionSnapshotAsync(
            Guid hospitalSiteId,
            Guid? organizationUnitId,
            PositionFormationSourceType sourceDocumentType,
            Guid sourceDocumentId,
            DateOnly referenceDate,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil daftar potret gap berhalaman (kriteria 2).
        /// </summary>
        Task<PagedResult<StaffingGapSnapshotListItemResponse>> GetPagedListAsync(
            StaffingGapSnapshotPagedQuery query,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil detail satu potret gap beserta rincian barisnya berdasarkan ID (kriteria 2).
        /// </summary>
        Task<StaffingGapSnapshotResponse?> GetByIdAsync(
            Guid id,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Langkah scheduler harian untuk memeriksa dan menyimpan potret bulanan pada akhir bulan (BE-HRD-048).
        /// </summary>
        Task<int> StepProcessMonthlySnapshotAsync(
            DateOnly operationalDate,
            SchedulerExecutionSummary summary,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Merekam susulan potret bulanan yang terlewat dengan label ReconstructedAt (kriteria BE-HRD-048).
        /// </summary>
        Task<Guid> ReconstructMonthlySnapshotAsync(
            Guid hospitalSiteId,
            DateOnly periodMonth,
            Guid currentUserId,
            CancellationToken cancellationToken = default);
    }
}
