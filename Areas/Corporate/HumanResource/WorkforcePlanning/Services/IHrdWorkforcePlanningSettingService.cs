using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Antarmuka layanan pengaturan kebijakan perencanaan tenaga kerja rumah sakit.
    /// Sesuai kontrak BE-HRD-031, HRD-DEC-061, HRD-DEC-071, VR-WFP-40, VR-WFP-41.
    /// </summary>
    public interface IHrdWorkforcePlanningSettingService
    {
        /// <summary>
        /// Mengambil pengaturan satu rumah sakit.
        /// Rumah sakit yang belum memiliki baris konfigurasi akan mengembalikan kedua nilai kosong (null), bukan angka bawaan.
        /// </summary>
        Task<WorkforcePlanningResult<WorkforcePlanningSettingResponse>> GetByHospitalSiteIdAsync(Guid hospitalSiteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengubah atau membuat pengaturan satu rumah sakit dengan validasi alasan, batas hari wajar, dan optimistic concurrency token.
        /// </summary>
        Task<WorkforcePlanningResult<WorkforcePlanningSettingResponse>> UpdateAsync(UpdateWorkforcePlanningSettingRequest request, Guid currentUserId, CancellationToken cancellationToken = default);
    }
}
