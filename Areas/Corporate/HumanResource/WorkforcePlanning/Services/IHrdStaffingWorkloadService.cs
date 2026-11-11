using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Antarmuka layanan pencatatan dan pengelolaan angka beban kerja manual (BE-HRD-034).
    /// Mengelola input beban per unit, jabatan, dan periode beserta keterangan sumber resmi.
    /// </summary>
    public interface IHrdStaffingWorkloadService
    {
        /// <summary>
        /// Mengambil daftar angka beban kerja berhalaman dengan filter cakupan Kepala Unit (VR-WFP-38).
        /// </summary>
        Task<WorkforcePlanningResult<PagedResult<StaffingWorkloadResponse>>> GetPagedListAsync(
            StaffingWorkloadPagedQuery query,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil detail satu angka beban kerja berdasarkan ID.
        /// Mengembalikan 404 jika di luar cakupan Kepala Unit (VR-WFP-38).
        /// </summary>
        Task<WorkforcePlanningResult<StaffingWorkloadResponse>> GetByIdAsync(
            Guid id,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mencatat angka beban kerja baru untuk satu periode (kriteria 1-5).
        /// </summary>
        Task<WorkforcePlanningResult<StaffingWorkloadResponse>> CreateAsync(
            UpsertStaffingWorkloadRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Memperbaiki isian angka beban kerja yang sudah ada.
        /// </summary>
        Task<WorkforcePlanningResult<StaffingWorkloadResponse>> UpdateAsync(
            Guid id,
            UpsertStaffingWorkloadRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default);
    }
}
