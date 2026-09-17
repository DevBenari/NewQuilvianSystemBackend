namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    /// <summary>
    /// Progres Pengkajian Pasien lima bagian — <c>BE-RWI-112</c>, api-contract 0.5.0 bagian 7.1,
    /// <c>RWI-DEC-119</c>, <c>RWI-DEC-120</c>.
    /// </summary>
    public class NursingAssessmentProgressResponse
    {
        public Guid EpisodeId { get; set; }

        /// <summary>Lima baris dengan urutan tetap: GENERAL, FALL_RISK, PAIN, EDUCATION, DISCHARGE_PLANNING.</summary>
        public List<NursingAssessmentProgressSection> Sections { get; set; } = new();

        public int CompletedCount { get; set; }

        public int TotalCount { get; set; } = 5;

        /// <summary><c>CompletedCount × 20</c> — selalu kelipatan 20.</summary>
        public int ProgressPercent { get; set; }

        /// <summary>Pengawasan Harian: tampil, <b>tidak</b> dihitung ke persen.</summary>
        public DateTime? DailyMonitoringLastRecordedAt { get; set; }

        /// <summary>Evaluasi Awal MPP: tampil, <b>tidak</b> dihitung ke persen.</summary>
        public string CaseManagementEvaluationStatus { get; set; } = string.Empty;

        /// <summary>
        /// Temuan berisiko untuk kepala konteks — terpisah dari <see cref="Sections"/>, tidak pernah
        /// mengubah keadaan bagian maupun persen.
        /// </summary>
        public List<NursingClinicalAlert> Alerts { get; set; } = new();

        public DateTime CalculatedAt { get; set; }
    }

    public class NursingAssessmentProgressSection
    {
        public string SectionCode { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;

        /// <summary><c>Completed</c> (✓), <c>NeedsAttention</c> (!), atau <c>NotFilled</c> (○).</summary>
        public string State { get; set; } = string.Empty;

        public string StateLabel { get; set; } = string.Empty;
        public Guid? LastDocumentId { get; set; }
        public DateTime? LastClinicalDateTime { get; set; }
        public string? LastAuthorName { get; set; }

        /// <summary>Hanya bagian PAIN: waktu kajian ulang nyeri sudah lewat tanpa dokumen nyeri sesudahnya.</summary>
        public bool ReassessmentOverdue { get; set; }
    }

    public class NursingClinicalAlert
    {
        public string SectionCode { get; set; } = string.Empty;
        public Guid AssessmentId { get; set; }
        public string InstrumentName { get; set; } = string.Empty;
        public string? BandCode { get; set; }
        public string? BandLabel { get; set; }
        public decimal? TotalScore { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
