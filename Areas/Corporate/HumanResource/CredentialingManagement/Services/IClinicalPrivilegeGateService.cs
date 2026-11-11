using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Services
{
    public interface IClinicalPrivilegeGateService
    {
        /// <summary>
        /// Validasi gerbang wewenang klinis real-time SIMRS EMR (BE-HRD-216, INT-HR-69, HRD-DEC-005)
        /// Mengecek keabsahan lisensi SIP faskes, masa berlaku SPK 3 tahun, dan derajat supervisi Level 1 s.d. Level 4
        /// Target performa latensi tinggi (p99 < 50ms)
        /// </summary>
        Task<ClinicalPrivilegeGateValidationResultDto> ValidateClinicalGateAsync(
            ClinicalPrivilegeGateValidationRequestDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Persetujuan digital (Co-Signature) oleh dokter supervisor penanggung jawab dalam 24 jam (BE-HRD-217, VR-CRD-07)
        /// </summary>
        Task<CoSignatureResultDto> ProcessCoSignatureAsync(
            CoSignatureActionDto request,
            Guid supervisorUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Perekaman aksi bypass darurat penyelamatan nyawa (Life-Saving Emergency Override) (BE-HRD-218, INT-HR-70)
        /// Menyimpan justifikasi klinis cito dan mengirim notifikasi audit seketika ke Komite Mutu (KMKP)
        /// </summary>
        Task<EmergencyOverrideLogDto> RecordLifeSavingOverrideAsync(
            ClinicalEmergencyOverrideRequestDto request,
            Guid doctorUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Audit dan penelaahan tindak lanjut oleh Komite Mutu & Keselamatan Pasien (KMKP) dalam 24 jam
        /// </summary>
        Task<EmergencyOverrideLogDto> ReviewEmergencyOverrideByKmkpAsync(
            ReviewEmergencyOverrideDto request,
            Guid kmkpAuditorUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil daftar log audit bypass darurat penyelamatan nyawa untuk keperluan rapat komite mutu
        /// </summary>
        Task<List<EmergencyOverrideLogDto>> GetEmergencyOverrideLogsAsync(
            bool? pendingOnly = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Pemberian kewenangan klinis sementara (Temporary Privilege) dokter tamu / darurat bencana (BE-HRD-219, HRD-DEC-150)
        /// </summary>
        Task<TemporaryPrivilegeResultDto> GrantTemporaryPrivilegeAsync(
            GrantTemporaryPrivilegeDto request,
            Guid authorizedByUserId,
            CancellationToken cancellationToken = default);
    }
}
