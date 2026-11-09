using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services
{
    public interface IEmployeeDocumentIssuanceService
    {
        /// <summary>
        /// Pengajuan permohonan penerbitan surat dinas resmi kepegawaian (ESS / Staf)
        /// Anti-Manual Guard diterapkan: nominal gaji ditarik otomatis dari Sub-10 Payroll bila jenis dokumen finansial.
        /// </summary>
        Task<EmployeeDocumentIssuanceDto> RequestDocumentAsync(
            Guid workforceProfileId,
            SubmitDocumentRequestDto request,
            Guid requestedByUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tier 1 Approval: Verifikasi & Persetujuan oleh Supervisor Kompensasi & Benefit (HRS-W4, VR-HRS-07)
        /// </summary>
        Task<EmployeeDocumentIssuanceDto> CbSupervisorApproveAsync(
            Guid issuanceId,
            CbSupervisorApprovalDto request,
            Guid cbSupervisorUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tier 2 Final Authorization & Issuance: Otorisasi & Penandatanganan Digital Pejabat Berwenang
        /// Disertai penegakan Anti-Self-Approval (VR-HRS-08), rendering naskah PDF (BE-HRD-202), QR Code SHA-256 & PIN BCrypt (BE-HRD-203).
        /// </summary>
        Task<EmployeeDocumentIssuanceDto> FinalAuthorizeAndIssueAsync(
            Guid issuanceId,
            FinalAuthorizeAndIssueDto request,
            Guid authorizedSignerUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil rincian data penerbitan surat naskah dinas
        /// </summary>
        Task<EmployeeDocumentIssuanceDto> GetIssuanceDetailAsync(
            Guid issuanceId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil antrean daftar penerbitan naskah dinas SDM
        /// </summary>
        Task<List<EmployeeDocumentIssuanceDto>> GetIssuancesQueueAsync(
            string? status = null,
            bool? isFinancialSensitive = null,
            Guid? workforceProfileId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifikasi publik keabsahan naskah dinas melalui QR Code / SHA-256 Hash
        /// Nilai finansial di-masking secara default (HRS-B5).
        /// </summary>
        Task<PublicDocumentVerificationDto> VerifyPublicDocumentAsync(
            string verificationHash,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Membuka masking rincian nominal gaji pada dokumen finansial dengan 4-digit PIN terverifikasi (VR-HRS-09)
        /// Dilindungi oleh Rate Limiter Anti-Brute-Force (VR-HRS-12: max 3 kali salah percobaan per 30 menit).
        /// </summary>
        Task<FinancialDetailsUnlockedDto> UnlockFinancialDetailsAsync(
            UnlockFinancialDetailsRequestDto request,
            string? clientIp = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengunduh berkas fisik PDF naskah dinas yang telah diotorisasi resmi
        /// </summary>
        Task<(byte[] FileBytes, string ContentType, string FileName)> DownloadIssuedDocumentPdfAsync(
            Guid issuanceId,
            Guid requestingUserId,
            bool isStaff,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mencabut / membatalkan surat naskah dinas yang telah terbit disertai Berita Acara Pencabutan (BAP)
        /// </summary>
        Task<EmployeeDocumentIssuanceDto> RevokeDocumentAsync(
            Guid issuanceId,
            RevokeDocumentRequestDto request,
            Guid revokingUserId,
            CancellationToken cancellationToken = default);
    }
}
