using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs
{
    /// <summary>
    /// DTO Umpan Balik Indikator Pemicu Kritis FPPE (INT-HR-74)
    /// </summary>
    public class FppeTriggerFeedDto
    {
        public Guid TriggerId { get; set; } = Guid.NewGuid();
        public Guid WorkforceProfileId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorCode { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;

        /// <summary>
        /// SENTINEL_EVENT, ABNORMAL_COMPLICATION, EMR_DELINQUENCY, ETHICAL_DISCIPLINARY, NEW_STAFF_ONBOARDING, NEW_PRIVILEGE_REQUEST
        /// </summary>
        public string TriggerType { get; set; } = string.Empty;

        /// <summary>
        /// FATAL, ERROR, WARNING, INFO
        /// </summary>
        public string Severity { get; set; } = "WARNING";

        public DateTime IncidentDate { get; set; } = DateTime.UtcNow;
        public string Description { get; set; } = string.Empty;
        public string MetricEvidence { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO Request Inisiasi Penugasan FPPE Terarah
    /// </summary>
    public class CreateFppeEngagementRequestDto
    {
        [Required(ErrorMessage = "WorkforceProfileId dokter target wajib diisi.")]
        public Guid WorkforceProfileId { get; set; }

        public Guid? PrivilegeCatalogId { get; set; }

        /// <summary>
        /// NEW_STAFF, NEW_PRIVILEGE_PROCTORING, SENTINEL_EVENT, ABNORMAL_COMPLICATION, EMR_DELINQUENCY, ETHICAL_DISCIPLINARY
        /// </summary>
        [Required(ErrorMessage = "Pemicu penugasan (TriggerSource) wajib diisi.")]
        [MaxLength(50)]
        public string TriggerSource { get; set; } = "NEW_STAFF";

        [Required(ErrorMessage = "Dasar justifikasi penugasan FPPE wajib diisi.")]
        [MinLength(10, ErrorMessage = "Justifikasi klinis minimal 10 karakter.")]
        public string InitialReasonJustification { get; set; } = string.Empty;

        [Required(ErrorMessage = "Dokter proctor pendamping wajib ditunjuk.")]
        public Guid AssignedProctorWorkforceId { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime? TargetEndDate { get; set; }

        public int TargetCaseCount { get; set; } = 10;
    }

    /// <summary>
    /// DTO Response Rincian Penugasan FPPE Terarah
    /// </summary>
    public class FppeEngagementResponseDto
    {
        public Guid Id { get; set; }
        public string EngagementNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string StaffCode { get; set; } = string.Empty;
        public string StaffDepartment { get; set; } = string.Empty;
        public Guid? PrivilegeCatalogId { get; set; }
        public string TriggerSource { get; set; } = string.Empty;
        public string InitialReasonJustification { get; set; } = string.Empty;
        public Guid AssignedProctorWorkforceId { get; set; }
        public string ProctorName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime TargetEndDate { get; set; }
        public int TargetCaseCount { get; set; }
        public int CompletedCaseCount { get; set; }
        public double CompletionPercentage => TargetCaseCount > 0 
            ? Math.Round(((double)CompletedCaseCount / TargetCaseCount) * 100.0, 1) 
            : 0.0;
        public string Status { get; set; } = "ACTIVE";
        public List<FppeCaseLogResponseDto> CaseLogs { get; set; } = new();
        public FppeConclusionResponseDto? Conclusion { get; set; }
    }

    /// <summary>
    /// DTO Request Entri Logbook Kasus Klinis FPPE
    /// </summary>
    public class CreateFppeCaseLogRequestDto
    {
        [Required(ErrorMessage = "Nomor Rekam Medis atau ID Episode pasien wajib diisi.")]
        [MaxLength(100)]
        public string SimrsPatientEpisodeId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama tindakan / prosedur yang dikerjakan wajib diisi.")]
        [MaxLength(200)]
        public string ProcedurePerformed { get; set; } = string.Empty;

        public DateTime ProcedureDate { get; set; } = DateTime.UtcNow;

        [Range(1, 5, ErrorMessage = "Skor keterampilan teknis wajib bernilai 1 s.d. 5.")]
        public int TechnicalSkillScore { get; set; } = 3;

        [Range(1, 5, ErrorMessage = "Skor pengambilan keputusan klinis wajib bernilai 1 s.d. 5.")]
        public int CriticalDecisionScore { get; set; } = 3;

        public bool HasComplication { get; set; } = false;

        public string? ComplicationDescription { get; set; }

        [Required(ErrorMessage = "Catatan evaluasi dan umpan balik tindakan klinis wajib diisi.")]
        public string ProctorFeedbackNotes { get; set; } = string.Empty;

        /// <summary>
        /// Level Kemandirian: 1 (Mandiri Penuh), 2 (Supervisi Tidak Langsung), 3 (Supervisi Langsung), 4 (Perlu Bantuan Penuh)
        /// </summary>
        [Range(1, 4, ErrorMessage = "Level kemandirian terobservasi wajib antara Level 1 s.d. Level 4.")]
        public int ObservedIndependenceLevel { get; set; } = 2;
    }

    /// <summary>
    /// DTO Response Logbook Kasus FPPE
    /// </summary>
    public class FppeCaseLogResponseDto
    {
        public Guid Id { get; set; }
        public Guid FppeEngagementId { get; set; }
        public string SimrsPatientEpisodeId { get; set; } = string.Empty;
        public string ProcedurePerformed { get; set; } = string.Empty;
        public DateTime ProcedureDate { get; set; }
        public int TechnicalSkillScore { get; set; }
        public int CriticalDecisionScore { get; set; }
        public bool HasComplication { get; set; }
        public string? ComplicationDescription { get; set; }
        public string ProctorFeedbackNotes { get; set; } = string.Empty;
        public int ObservedIndependenceLevel { get; set; }
        public string IndependenceLevelDescription => ObservedIndependenceLevel switch
        {
            1 => "Level 1 - Mandiri Penuh (Independent Practice)",
            2 => "Level 2 - Supervisi Tidak Langsung (Indirect Supervision)",
            3 => "Level 3 - Supervisi Langsung (Direct Supervision)",
            4 => "Level 4 - Asistensi Total (Full Assistance Required)",
            _ => "Tidak Diketahui"
        };
        public bool IsVerifiedByProctor { get; set; }
        public DateTime? ProctorVerifiedAt { get; set; }
    }

    /// <summary>
    /// DTO Request Verifikasi Logbook oleh Proctor Pendamping
    /// </summary>
    public class VerifyFppeCaseLogRequestDto
    {
        [Required(ErrorMessage = "ID Dokter Proctor yang memverifikasi wajib diisi.")]
        public Guid ProctorWorkforceId { get; set; }

        [Range(1, 5, ErrorMessage = "Skor keterampilan teknis wajib bernilai 1 s.d. 5.")]
        public int TechnicalSkillScore { get; set; } = 3;

        [Range(1, 5, ErrorMessage = "Skor pengambilan keputusan klinis wajib bernilai 1 s.d. 5.")]
        public int CriticalDecisionScore { get; set; } = 3;

        [Range(1, 4, ErrorMessage = "Level kemandirian terobservasi wajib bernilai 1 s.d. 4.")]
        public int ObservedIndependenceLevel { get; set; } = 2;

        public bool HasComplication { get; set; } = false;

        public string? ComplicationDescription { get; set; }

        [Required(ErrorMessage = "Catatan konfirmasi dan evaluasi proctor wajib diisi.")]
        public string ProctorFeedbackNotes { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO Request Pengesahan Sidang Pleno Penetapan Vonis FPPE
    /// </summary>
    public class ConcludeFppeEngagementRequestDto
    {
        /// <summary>
        /// 4 Rekomendasi Baku: GRANT_INDEPENDENT, EXTEND_SUPERVISION, RESTRICT_PRIVILEGE, REVOKE_PRIVILEGE
        /// </summary>
        [Required(ErrorMessage = "Rekomendasi vonis sidang FPPE wajib dipilih.")]
        public string RecommendedAction { get; set; } = "GRANT_INDEPENDENT";

        [Required(ErrorMessage = "Ringkasan pertimbangan sidang pleno Komite Medik wajib diisi.")]
        [MinLength(15, ErrorMessage = "Ringkasan sidang pleno minimal 15 karakter.")]
        public string PlenaryReviewSummary { get; set; } = string.Empty;

        public string? RemediationPlan { get; set; }

        [Required(ErrorMessage = "Ketua Komite Medik / Subkomite Mutu pengesah sidang wajib diisi.")]
        public Guid ApprovedByCommitteeChairmanId { get; set; }

        /// <summary>
        /// Durasi perpanjangan jika memilih EXTEND_SUPERVISION (maksimal 3 bulan)
        /// </summary>
        [Range(1, 3, ErrorMessage = "Durasi perpanjangan proctoring dibatasi 1 hingga maksimal 3 bulan.")]
        public int ExtensionMonths { get; set; } = 1;
    }

    /// <summary>
    /// DTO Response Berita Acara Kesimpulan Sidang FPPE
    /// </summary>
    public class FppeConclusionResponseDto
    {
        public Guid Id { get; set; }
        public Guid FppeEngagementId { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
        public string RecommendedActionDescription => RecommendedAction switch
        {
            "GRANT_INDEPENDENT" => "Kewenangan Klinis Mandiri Penuh Diberikan (Full Privilege Confirmed)",
            "EXTEND_SUPERVISION" => "Perpanjangan Masa Proctoring / Pendampingan (Max 3 Bulan)",
            "RESTRICT_PRIVILEGE" => "Pembatasan Kewenangan Klinis Supervisi Level 3 (Restricted Supervision)",
            "REVOKE_PRIVILEGE" => "Pencabutan Kewenangan Klinis Terkait (Privilege Revoked)",
            _ => RecommendedAction
        };
        public string PlenaryReviewSummary { get; set; } = string.Empty;
        public string? RemediationPlan { get; set; }
        public Guid ApprovedByCommitteeChairmanId { get; set; }
        public DateTime FinalizedAt { get; set; }
        public string SignatureHash { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO Request & Response Sinkronisasi Kewenangan Klinis Pasca-FPPE (INT-HR-76)
    /// </summary>
    public class SyncFppePrivilegeRequestDto
    {
        public string? Notes { get; set; }
        public Guid? InitiatedBy { get; set; }
    }

    public class SyncFppePrivilegeResponseDto
    {
        public Guid FppeEngagementId { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
        public string PrivilegeStatus { get; set; } = string.Empty;
        public string SyncStatus { get; set; } = "SUCCESS";
        public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
        public string Message { get; set; } = string.Empty;
    }
}
