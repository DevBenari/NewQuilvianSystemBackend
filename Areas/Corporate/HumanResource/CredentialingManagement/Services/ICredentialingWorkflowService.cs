using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Services
{
    public interface ICredentialingWorkflowService
    {
        /// <summary>
        /// Pengajuan berkas kredensial nakes baru atau peningkatan kewenangan klinis (BE-HRD-212)
        /// Memvalidasi prasyarat mutlak verifikasi sumber primer (PSV) lisensi STR/SIP (VR-CRD-02)
        /// </summary>
        Task<CredentialingApplicationDto> CreateApplicationAsync(
            CreateCredentialingApplicationDto request,
            Guid applicantUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Penunjukan panel Mitra Bestari (Peer Reviewers) minimal 2 orang sebidang (HRD-DEC-147)
        /// Menegakkan aturan Anti-Self-Review mutlak: nakes dilarang mereview dirinya sendiri (VR-CRD-15)
        /// </summary>
        Task<CredentialingApplicationDto> AssignPeerReviewersAsync(
            AssignPeerReviewersDto request,
            Guid assignedByUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Pengisian lembar asesmen portofolio klinis oleh anggota panel Mitra Bestari (BE-HRD-213)
        /// </summary>
        Task<bool> SubmitPeerReviewAssessmentAsync(
            SubmitPeerReviewAssessmentDto request,
            Guid assessorUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Pelaksanaan dan pengesahan Berita Acara Sidang Pleno Komite Kredensial (BE-HRD-213)
        /// </summary>
        Task<PlenarySessionResultDto> ConcludePlenarySessionAsync(
            ConcludePlenarySessionDto request,
            Guid chairpersonUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil detail permohonan kredensial beserta status reviewer dan rekomendasi
        /// </summary>
        Task<CredentialingApplicationDto> GetApplicationDetailAsync(
            Guid applicationId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil daftar permohonan kredensial aktif pada komite tertentu
        /// </summary>
        Task<List<CredentialingApplicationDto>> GetApplicationsByCommitteeAsync(
            Guid committeeId,
            string? status = null,
            CancellationToken cancellationToken = default);
    }

    public class PlenarySessionResultDto
    {
        public Guid ApplicationId { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public string PlenaryBapNumber { get; set; } = string.Empty;
        public DateTime PlenaryDate { get; set; }
        public string PlenaryDecision { get; set; } = string.Empty;
        public int TotalPrivilegesEvaluated { get; set; }
        public int TotalIndependentPrivileges { get; set; }
        public int TotalSupervisedPrivileges { get; set; }
        public int TotalDeniedPrivileges { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
