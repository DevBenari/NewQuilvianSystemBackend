namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs
{
    /// <summary>Filter worklist kredensial (register masalah SDM MSL-16, 19 Nov 2026).</summary>
    public class CredentialingWorklistQuery
    {
        /// <summary>Cari nomor permohonan/SPK, nama, atau kode pegawai.</summary>
        public string? Search { get; set; }

        /// <summary>Hanya untuk daftar umum permohonan: Submitted, InReview, PlenarySessionConcluded.</summary>
        public string? Status { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>Filter radar rekredensial: SPK-RKK aktif yang masa berlakunya mendekati habis.</summary>
    public class RecredentialingCycleQuery
    {
        public string? Search { get; set; }

        /// <summary>Jendela hari sebelum masa berlaku habis. Bawaan 180 hari, maksimal 1095 (3 tahun).</summary>
        public int DueWithinDays { get; set; } = 180;

        /// <summary>Ikut tampilkan SPK-RKK yang sudah lewat masa berlakunya.</summary>
        public bool IncludeExpired { get; set; } = true;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>Filter registri kewenangan klinis lintas tenaga kesehatan.</summary>
    public class ClinicalPrivilegeRegistryQuery
    {
        public string? Search { get; set; }

        /// <summary>Nama nilai <c>ClinicalPrivilegeStatus</c>, misalnya Active atau Suspended.</summary>
        public string? Status { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>Baris permohonan kredensial untuk daftar umum, worklist Mitra Bestari, pleno komite, dan antrean draf SPK-RKK.</summary>
    public class CredentialingApplicationListItemDto
    {
        public Guid Id { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string ApplicantName { get; set; } = string.Empty;
        public string? ProfileCode { get; set; }
        public string ApplicationType { get; set; } = string.Empty;
        public string ApplicationStatus { get; set; } = string.Empty;
        public DateTime ApplicationDate { get; set; }
        public int RequestedPrivilegeCount { get; set; }
        /// <summary>Jumlah anggota panel Mitra Bestari yang ditugaskan (dibaca dari <c>ReviewerProfiles</c>; sejak FIN-HRD-09-BE-006 daftar ini dipertahankan di BAP pleno).</summary>
        public int AssignedReviewerCount { get; set; }
        /// <summary>Jumlah penilaian Mitra Bestari yang sudah masuk.</summary>
        public int PeerAssessmentCount { get; set; }
        /// <summary>True bila akun login sudah mengirim penilaian untuk permohonan ini.</summary>
        public bool AssessedByMe { get; set; }
        /// <summary>Recommended, ConditionallyRecommended, atau Rejected setelah sidang pleno.</summary>
        public string? PlenaryDecision { get; set; }
        public string? PlenaryBapNumber { get; set; }
    }

    /// <summary>Lembar penilaian untuk reviewer Mitra Bestari: butir kewenangan yang diajukan pemohon.</summary>
    public class CredentialingPeerReviewSheetDto
    {
        public Guid ApplicationId { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public string ApplicantName { get; set; } = string.Empty;
        public string? ProfileCode { get; set; }
        public string ApplicationType { get; set; } = string.Empty;
        public DateTime ApplicationDate { get; set; }
        public string? ApplicantNotes { get; set; }
        /// <summary>ID profil reviewer dari token login; dikirim balik apa adanya saat menyimpan penilaian.</summary>
        public Guid ReviewerWorkforceProfileId { get; set; }
        public bool AlreadyAssessed { get; set; }
        public List<ClinicalPrivilegeCatalogItemDto> RequestedPrivileges { get; set; } = new();
    }

    /// <summary>Satu penilaian Mitra Bestari pada matriks pleno.</summary>
    public class PeerAssessmentSummaryDto
    {
        public Guid AssessmentId { get; set; }
        public Guid? ReviewerWorkforceProfileId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public DateTime AssessmentDate { get; set; }
        public string? OverallRecommendation { get; set; }
        public bool RequiresSupervision { get; set; }
        public List<PrivilegeAssessmentItemDto> Items { get; set; } = new();
    }

    /// <summary>Matriks pleno: butir yang diajukan dan hasil seluruh reviewer, untuk menyelesaikan beda pendapat.</summary>
    public class CredentialingPlenaryMatrixDto
    {
        public Guid ApplicationId { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public string ApplicantName { get; set; } = string.Empty;
        public string? ProfileCode { get; set; }
        public string ApplicationStatus { get; set; } = string.Empty;
        public List<string> AssignedReviewerNames { get; set; } = new();
        public List<ClinicalPrivilegeCatalogItemDto> RequestedPrivileges { get; set; } = new();
        public List<PeerAssessmentSummaryDto> Assessments { get; set; } = new();
    }

    /// <summary>Baris SPK-RKK yang menunggu tanda tangan Direktur Utama.</summary>
    public class SpkRkkPendingSignatureItemDto
    {
        public Guid Id { get; set; }
        public Guid? CredentialingApplicationId { get; set; }
        public string SpkNumber { get; set; } = string.Empty;
        public string RkkNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string HolderName { get; set; } = string.Empty;
        public string? ProfileCode { get; set; }
        public string ClinicalPrivilegeType { get; set; } = string.Empty;
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
        public DateTime IssuedAt { get; set; }
        public string IssuanceStatus { get; set; } = string.Empty;
    }

    /// <summary>Hasil verifikasi publik SPK-RKK lewat kode QR. Hanya data yang memang tercetak di dokumen.</summary>
    public class SpkRkkPublicVerificationDto
    {
        public bool IsValid { get; set; }
        /// <summary>Valid, Expired, Suspended, Revoked, atau NotSigned.</summary>
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public string HolderName { get; set; } = string.Empty;
        public string SpkNumber { get; set; } = string.Empty;
        public string RkkNumber { get; set; } = string.Empty;
        public string ClinicalPrivilegeType { get; set; } = string.Empty;
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
        public DateTime? SignedAt { get; set; }
        public string DirectorGeneralName { get; set; } = string.Empty;
    }

    /// <summary>Baris radar rekredensial: SPK-RKK aktif, sisa hari, status berkas rekredensial, dan nilai OPPE terakhir.</summary>
    public class RecredentialingCycleListItemDto
    {
        public Guid SpkRkkIssuanceId { get; set; }
        public string SpkNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string HolderName { get; set; } = string.Empty;
        public string? ProfileCode { get; set; }
        public string ClinicalPrivilegeType { get; set; } = string.Empty;
        public DateTime ValidUntil { get; set; }
        /// <summary>Negatif bila sudah lewat.</summary>
        public int DaysUntilDue { get; set; }
        /// <summary>Status <c>TrxRecredentialingApplication</c> terakhir; null bila berkas rekredensial belum dibuat.</summary>
        public string? RecredentialingStatus { get; set; }
        public string? RecredentialingNumber { get; set; }
        public decimal? LatestOppeScore { get; set; }
        public string? LatestOppeGrade { get; set; }
        public string? LatestOppePeriodName { get; set; }
    }

    /// <summary>Baris registri kewenangan klinis lintas tenaga kesehatan.</summary>
    public class ClinicalPrivilegeRegistryItemDto
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string HolderName { get; set; } = string.Empty;
        public string? ProfileCode { get; set; }
        public string PrivilegeCode { get; set; } = string.Empty;
        public string PrivilegeName { get; set; } = string.Empty;
        public string PrivilegeType { get; set; } = string.Empty;
        public string PrivilegeStatus { get; set; } = string.Empty;
        public string? SpecialtyName { get; set; }
        public string? PracticeLocation { get; set; }
        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsTemporary { get; set; }
        public bool IsEmergencyPrivilege { get; set; }
        public bool IsSupervisionRequired { get; set; }
    }
}
