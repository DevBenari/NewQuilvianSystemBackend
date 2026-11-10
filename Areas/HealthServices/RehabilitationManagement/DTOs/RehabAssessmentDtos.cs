using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs
{
    // =====================================================================
    // Asesmen medis dan asesmen profesi — api-contract v1 grup Rehab Medical Assessment dan
    // Rehab Discipline Assessment (BE-RHB-009). Kedua grup memakai bentuk yang sama.
    // =====================================================================

    /// <summary>Isian draft asesmen. Dipakai <c>POST /by-episode/{episodeId}</c> dan <c>PUT /{id}</c>.</summary>
    /// <remarks>
    /// <c>AuthorUserId</c> sengaja tidak ada: penulis selalu pengguna yang membuat draft.
    /// <c>PrescribedDisciplines</c> hanya dibaca untuk asesmen medis; <c>DisciplineId</c> hanya untuk
    /// asesmen profesi dan tidak dapat diubah setelah dibuat.
    /// </remarks>
    public class SaveRehabAssessmentRequest
    {
        /// <summary>Wajib untuk <c>PUT</c>; diabaikan saat membuat.</summary>
        public int ExpectedVersion { get; set; }

        public Guid? EncounterId { get; set; }

        /// <summary>Asesmen profesi saja.</summary>
        public Guid? DisciplineId { get; set; }

        /// <summary>Waktu asesmen. Kosong = sekarang.</summary>
        public DateTime? AssessedAt { get; set; }

        public string? HistoryText { get; set; }
        public string? PhysicalFunctionalFindingsText { get; set; }
        public string? RiskAndContraindicationText { get; set; }
        public string? ProblemOrDiagnosisText { get; set; }
        public string? PrognosisText { get; set; }
        public string? ConclusionText { get; set; }
        public string? PatientGoalText { get; set; }

        /// <summary>Seluruh pengukuran draft. Daftar ini menggantikan daftar yang tersimpan.</summary>
        public List<RehabMeasurementRequest> Measurements { get; set; } = new();

        /// <summary>Asesmen medis saja. Daftar ini menggantikan resep draft yang tersimpan.</summary>
        public List<RehabPrescribedDisciplineRequest> PrescribedDisciplines { get; set; } = new();
    }

    /// <summary>
    /// Satu pengukuran. Berinstrumen: <c>InstrumentVersionId</c> + <c>Answers</c>, skor dan pita
    /// dihitung mesin instrumen. Terstruktur: nama, nilai, satuan (RHB-DEC-006/017).
    /// </summary>
    public class RehabMeasurementRequest
    {
        public RehabMeasurementSource Source { get; set; }
        public Guid? InstrumentVersionId { get; set; }

        /// <summary>Jawaban instrumen: kode isian → nilai, sesuai definisi versinya.</summary>
        public Dictionary<string, JsonElement>? Answers { get; set; }

        /// <summary>Nama pengukuran terstruktur. Untuk berinstrumen diisi dari nama instrumen.</summary>
        public string? InstrumentName { get; set; }
        public string? BodyPartOrDomain { get; set; }
        public decimal? NumericValue { get; set; }
        public string? ValueText { get; set; }
        public string? Unit { get; set; }
        public DateTime? MeasuredAt { get; set; }
    }

    public class RehabPrescribedDisciplineRequest
    {
        public Guid DisciplineId { get; set; }
        public string? MedicalPrecautionText { get; set; }
    }

    /// <summary>Body <c>POST /{id}/sign</c>.</summary>
    public class RehabSignRequest
    {
        public int ExpectedVersion { get; set; }
    }

    public class RehabAssessmentListItem
    {
        public Guid Id { get; set; }
        public Guid EpisodeId { get; set; }
        public RehabAssessmentType AssessmentType { get; set; }
        public string AssessmentTypeName { get; set; } = string.Empty;
        public Guid? DisciplineId { get; set; }
        public string? DisciplineName { get; set; }
        public Guid EncounterId { get; set; }
        public string? EncounterNumber { get; set; }
        public DateTime AssessedAt { get; set; }
        public Guid AuthorUserId { get; set; }
        public string? AuthorName { get; set; }
        public RehabRecordStatus RecordStatus { get; set; }
        public string RecordStatusName { get; set; } = string.Empty;
        public DateTime? SignedAt { get; set; }
        public int Version { get; set; }
    }

    public class RehabAssessmentDetailResponse : RehabAssessmentListItem
    {
        public string EpisodeNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public Guid? InpEpisodeId { get; set; }

        public string? HistoryText { get; set; }
        public string? PhysicalFunctionalFindingsText { get; set; }
        public string? RiskAndContraindicationText { get; set; }
        public string? ProblemOrDiagnosisText { get; set; }
        public string? PrognosisText { get; set; }
        public string? ConclusionText { get; set; }
        public string? PatientGoalText { get; set; }

        public List<RehabMeasurementItem> Measurements { get; set; } = new();
        public List<RehabPrescribedDisciplineItem> PrescribedDisciplines { get; set; } = new();

        /// <summary>Status keutuhan Rekam Medis (Draf/Ditandatangani/Terkunci/Dibatalkan).</summary>
        public ClinicalDocumentIntegrityStatus? IntegrityStatus { get; set; }
        public string? IntegrityStatusName { get; set; }

        /// <summary>
        /// Jenis dokumen Rekam Medis (<c>RehabAssessment</c> = 47) untuk addendum lewat endpoint RM
        /// setelah final.
        /// </summary>
        public ClinicalDocumentKind DocumentKind { get; set; } = ClinicalDocumentKind.RehabAssessment;

        /// <summary>
        /// Aksi menurut status dan kepenulisan: <c>update</c>, <c>sign</c>, <c>cancel</c> untuk penulis
        /// draft; <c>addendum</c> setelah final. Hak akses tetap diperiksa layar dan server.
        /// </summary>
        public List<string> AvailableActions { get; set; } = new();
    }

    public class RehabMeasurementItem
    {
        public Guid Id { get; set; }
        public RehabMeasurementSource Source { get; set; }
        public Guid? InstrumentVersionId { get; set; }
        public ClinicalInstrumentKind? InstrumentKind { get; set; }
        public string InstrumentName { get; set; } = string.Empty;
        public string? BodyPartOrDomain { get; set; }
        public JsonElement? Answers { get; set; }
        public decimal? NumericValue { get; set; }
        public string? ValueText { get; set; }
        public string? Unit { get; set; }
        public string? ScoreBandCode { get; set; }
        public DateTime MeasuredAt { get; set; }
    }

    public class RehabPrescribedDisciplineItem
    {
        public Guid DisciplineId { get; set; }
        public string DisciplineCode { get; set; } = string.Empty;
        public string DisciplineName { get; set; } = string.Empty;
        public string? MedicalPrecautionText { get; set; }
    }
}
