using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Antarmuka layanan standar kebutuhan tenaga kerja rumah sakit (BE-HRD-035).
    /// Mengelola siklus hidup standar per unit, perhitungan rekomendasi, dan pengajuan ke workflow T9.
    /// </summary>
    public interface IHrdStaffingStandardService
    {
        Task<WorkforcePlanningResult<PagedResult<StaffingStandardListItemResponse>>> GetPagedListAsync(
            StaffingStandardPagedQuery query,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        Task<WorkforcePlanningResult<StaffingStandardResponse>> GetByIdAsync(
            Guid id,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil standar yang aktif berlaku untuk satu unit pada tanggal tertentu (kontrak baca Penjadwalan).
        /// </summary>
        Task<WorkforcePlanningResult<StaffingStandardResponse>> GetEffectiveAsync(
            Guid organizationUnitId,
            DateOnly date,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Membuat draf versi baru untuk satu unit. Bila sudah ada versi aktif, isinya disalin sebagai titik awal (kriteria 1).
        /// </summary>
        Task<WorkforcePlanningResult<StaffingStandardResponse>> CreateDraftAsync(
            CreateStaffingStandardRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Memperbarui draf atau versi yang dikembalikan (kriteria 2).
        /// </summary>
        Task<WorkforcePlanningResult<StaffingStandardResponse>> UpdateDraftAsync(
            Guid id,
            UpdateStaffingStandardRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Menghitung rekomendasi angka kebutuhan tenaga kerja per baris berdasarkan rumus Bagian 16.9 (kriteria 4).
        /// </summary>
        Task<WorkforcePlanningResult<StaffingRecommendationResponse>> GetRecommendationAsync(
            Guid id,
            DateOnly periodStartDate,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengajukan standar kebutuhan tenaga ke alur persetujuan workflow T9 (kriteria 3).
        /// </summary>
        Task<WorkforcePlanningResult<StaffingStandardResponse>> SubmitAsync(
            Guid id,
            SubmitStaffingStandardRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Membatalkan draf atau dokumen yang dikembalikan.
        /// </summary>
        Task<WorkforcePlanningResult<StaffingStandardResponse>> CancelAsync(
            Guid id,
            CancelStaffingStandardRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil metadata filter dan opsi pilihan enumerasi untuk layar standar kebutuhan tenaga.
        /// </summary>
        StaffingStandardFilterMetadataResponse GetFilterMetadata();
    }
}
