using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Layanan Tata Kelola Alur Kerja FPPE, Logbook Kasus, dan Sidang Pleno Komite Medik
    /// </summary>
    public interface IProctoringWorkflowService
    {
        /// <summary>
        /// Inisiasi penugasan FPPE terarah dengan verifikasi kualifikasi proctor dan ambang target kasus
        /// </summary>
        Task<FppeEngagementResponseDto> CreateEngagementAsync(CreateFppeEngagementRequestDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil rincian penugasan FPPE beserta logbook kasus dan kesimpulan sidang
        /// </summary>
        Task<FppeEngagementResponseDto?> GetEngagementByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Menambahkan catatan kasus tindakan klinis terproctoring dengan pseudonimisasi pasien (VR-OPP-11)
        /// </summary>
        Task<FppeCaseLogResponseDto> AddCaseLogAsync(Guid engagementId, CreateFppeCaseLogRequestDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifikasi dan evaluasi kemandirian kasus oleh Dokter Proctor pendamping (VR-OPP-12)
        /// </summary>
        Task<FppeCaseLogResponseDto> VerifyCaseLogAsync(Guid logId, VerifyFppeCaseLogRequestDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengesahkan putusan berita acara sidang pleno Komite Medik atas hasil FPPE (VR-OPP-13, VR-OPP-14)
        /// </summary>
        Task<FppeConclusionResponseDto> ConcludeEngagementAsync(Guid engagementId, ConcludeFppeEngagementRequestDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Menyinkronkan pembaruan wewenang klinis pasca-sidang pleno FPPE ke profil nakes dan SIMRS (INT-HR-76)
        /// </summary>
        Task<SyncFppePrivilegeResponseDto> SyncPrivilegeAsync(Guid engagementId, SyncFppePrivilegeRequestDto request, CancellationToken cancellationToken = default);
    }
}
