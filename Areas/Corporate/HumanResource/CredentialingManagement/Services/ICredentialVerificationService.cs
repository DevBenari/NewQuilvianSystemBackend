using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Services
{
    public interface ICredentialVerificationService
    {
        /// <summary>
        /// Mendaftarkan lisensi profesi nakes (STR Seumur Hidup atau SIP 5 Tahun)
        /// Memvalidasi aturan registrasi lisensi faskes (VR-CRD-01)
        /// </summary>
        Task<ProfessionalLicenseDto> RegisterLicenseAsync(
            RegisterProfessionalLicenseDto request,
            Guid registeredByUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Eksekusi audit Verifikasi Sumber Primer (PSV - Primary Source Verification)
        /// ke portal Konsil Kedokteran Indonesia (KKI) atau SatuSehat SDMK Kemenkes (HRD-DEC-146, VR-CRD-02)
        /// </summary>
        Task<PsvVerificationResultDto> PerformPrimarySourceVerificationAsync(
            PsvVerificationRequestDto request,
            Guid auditorUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil rincian data lisensi profesional beserta status radar kedaluwarsa
        /// </summary>
        Task<ProfessionalLicenseDto> GetLicenseDetailAsync(
            Guid licenseId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil seluruh lisensi profesi milik tenaga kerja spesifik
        /// </summary>
        Task<List<ProfessionalLicenseDto>> GetLicensesByEmployeeAsync(
            Guid workforceProfileId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengajukan perpanjangan lisensi SIP 5 tahun baru oleh nakes/analis kredensial
        /// </summary>
        Task<ProfessionalLicenseDto> RenewLicenseAsync(
            Guid licenseId,
            RenewProfessionalLicenseDto request,
            Guid requestedByUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil data radar pemantauan lisensi yang mendekati kedaluwarsa (H-90, H-60, H-30, H-7, Expired H-0)
        /// </summary>
        Task<List<LicenseRadarAlertDto>> GetLicenseExpiringRadarListAsync(
            string? radarLevel = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Eksekusi aksi Fail-Closed otomatis saat lisensi SIP nakes telah kedaluwarsa (H-0)
        /// Menonaktifkan order EMR SIMRS (INT-HR-69) dan memblokir jadwal dinas Sub-06 (INT-HR-71, CRD-B2)
        /// </summary>
        Task<int> ExecuteFailClosedForExpiredLicensesAsync(
            CancellationToken cancellationToken = default);
    }
}
