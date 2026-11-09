using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Services
{
    public interface ISpkRkkIssuanceService
    {
        /// <summary>
        /// Menerbitkan draf Surat Penugasan Klinis (SPK) dan Rincian Kewenangan Klinis (RKK)
        /// bersiklus 3 tahun sesuai ketetapan sidang pleno komite (BE-HRD-214, CRD-B3)
        /// </summary>
        Task<SpkRkkDetailDto> IssueSpkRkkDraftAsync(
            IssueSpkRkkRequestDto request,
            Guid issuedByUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Otorisasi pengesahan tunggal digital e-Sign Direktur Utama Rumah Sakit (HRD-DEC-148, VR-CRD-12)
        /// Memicu aktivasi kewenangan klinis di SIMRS dan pengarsipan abadi ke E-Dossier Sub-02 (INT-HR-68)
        /// </summary>
        Task<SpkRkkDetailDto> SignSpkRkkByDirectorGeneralAsync(
            SignSpkRkkDto request,
            Guid directorGeneralUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil rincian data SPK-RKK beserta daftar butir kewenangan klinis yang disahkan
        /// </summary>
        Task<SpkRkkDetailDto> GetSpkRkkDetailAsync(
            Guid spkRkkId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil riwayat SPK-RKK yang pernah diterbitkan untuk tenaga kerja tertentu
        /// </summary>
        Task<List<SpkRkkDetailDto>> GetSpkRkkByEmployeeAsync(
            Guid workforceProfileId,
            CancellationToken cancellationToken = default);
    }
}
