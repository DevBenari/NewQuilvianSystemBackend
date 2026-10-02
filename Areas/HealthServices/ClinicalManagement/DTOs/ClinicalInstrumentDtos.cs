using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    // =====================================================================
    // Bentuk DefinitionJson — kamus data 0.4 bagian 11.5
    // =====================================================================

    /// <summary>
    /// Definisi satu versi instrumen atau formulir klinis — <c>BE-RWI-107</c>.
    /// </summary>
    /// <remarks>
    /// <c>ReviewFlags</c> adalah perluasan kamus data: catatan tinjauan yang ditulis seeder ketika definisi
    /// lama bertabrakan. Selama masih berisi, versi itu tidak dapat disahkan — penanda wajib dibaca dan
    /// dikosongkan manusia, bukan dibereskan program.
    /// </remarks>
    public class ClinicalInstrumentDefinition
    {
        public List<ClinicalInstrumentSectionDefinition> Sections { get; set; } = new();

        public ClinicalInstrumentScoringDefinition? Scoring { get; set; }

        public List<ClinicalInstrumentBandDefinition> Bands { get; set; } = new();

        public List<string> RequiredItemCodes { get; set; } = new();

        public int? ReassessmentMinutes { get; set; }

        public List<string> ReviewFlags { get; set; } = new();
    }

    public class ClinicalInstrumentSectionDefinition
    {
        public string? Code { get; set; }
        public string? Label { get; set; }
        public List<ClinicalInstrumentItemDefinition> Items { get; set; } = new();
    }

    public class ClinicalInstrumentItemDefinition
    {
        public string? Code { get; set; }
        public string? Label { get; set; }

        /// <summary><c>boolean</c>, <c>single</c>, <c>multi</c>, <c>text</c>, <c>number</c>, <c>date</c>.</summary>
        public string? Type { get; set; }

        public List<ClinicalInstrumentOptionDefinition>? Options { get; set; }

        /// <summary>Kolom <c>TrxPatientAssessment</c> yang menyimpan isian ini.</summary>
        public string? Binding { get; set; }
    }

    public class ClinicalInstrumentOptionDefinition
    {
        public string? Code { get; set; }
        public string? Label { get; set; }
        public decimal? Score { get; set; }
    }

    public class ClinicalInstrumentScoringDefinition
    {
        public string Method { get; set; } = "sum";
    }

    public class ClinicalInstrumentBandDefinition
    {
        public string? Code { get; set; }
        public string? Label { get; set; }
        public decimal? MinInclusive { get; set; }
        public decimal? MaxExclusive { get; set; }
        public bool IsAlert { get; set; }

        /// <summary>Nama nilai <c>FallRiskStatus</c>, contoh <c>HighRisk</c>.</summary>
        public string? MappedFallRiskStatus { get; set; }
    }

    /// <summary>Hasil hitung server atas jawaban satu versi instrumen.</summary>
    public class InstrumentScoreResult
    {
        public decimal? TotalScore { get; set; }
        public string? BandCode { get; set; }
        public string? BandLabel { get; set; }
        public bool IsAlertBand { get; set; }
        public FallRiskStatus? MappedFallRiskStatus { get; set; }

        /// <summary>Label isian berskor yang belum dijawab. Selama berisi, skor tidak diterbitkan.</summary>
        public List<string> UnansweredScoredItems { get; set; } = new();

        public List<string> Errors { get; set; } = new();
    }

    // =====================================================================
    // Permintaan dan balasan grup Clinical Instrument — api-contract 0.5.0 bagian 7.2
    // =====================================================================

    public class CreateClinicalInstrumentRequest
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public ClinicalInstrumentKind InstrumentKind { get; set; }

        public int? TargetMinAgeMonths { get; set; }

        public int? TargetMaxAgeMonths { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdateClinicalInstrumentRequest
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public int? TargetMinAgeMonths { get; set; }

        public int? TargetMaxAgeMonths { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class CreateClinicalInstrumentVersionRequest
    {
        /// <summary>Definisi baru; kosong berarti disalin dari versi terakhir.</summary>
        public JsonElement? Definition { get; set; }
    }

    public class UpdateClinicalInstrumentVersionRequest
    {
        public JsonElement Definition { get; set; }

        [Required]
        [MaxLength(64)]
        public string ExpectedDefinitionHash { get; set; } = string.Empty;
    }

    public class ApproveClinicalInstrumentVersionRequest
    {
        [MaxLength(500)]
        public string? ApprovalNote { get; set; }

        [Required]
        [MaxLength(64)]
        public string ExpectedDefinitionHash { get; set; } = string.Empty;
    }

    public class RetireClinicalInstrumentVersionRequest
    {
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    public class ScorePreviewRequest
    {
        public Dictionary<string, JsonElement> Responses { get; set; } = new();
    }

    public class ClinicalInstrumentListItem
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ClinicalInstrumentKind InstrumentKind { get; set; }
        public int? TargetMinAgeMonths { get; set; }
        public int? TargetMaxAgeMonths { get; set; }
        public bool IsActive { get; set; }
        public Guid? ApprovedVersionId { get; set; }
        public int? ApprovedVersionNumber { get; set; }
        public int? LatestDraftVersionNumber { get; set; }
    }

    public class ClinicalInstrumentVersionResponse
    {
        public Guid Id { get; set; }
        public Guid InstrumentId { get; set; }
        public int VersionNumber { get; set; }
        public ClinicalInstrumentVersionStatus VersionStatus { get; set; }
        public JsonElement Definition { get; set; }
        public string DefinitionHash { get; set; } = string.Empty;
        public Guid LastModifiedByUserId { get; set; }
        public string? LastModifiedByName { get; set; }
        public DateTime LastModifiedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovalNote { get; set; }
        public DateTime? RetiredAt { get; set; }
        public string? RetireReason { get; set; }

        /// <summary>Penanda tinjauan yang masih terbuka; versi tidak dapat disahkan selama berisi.</summary>
        public List<string> ReviewFlags { get; set; } = new();

        /// <summary>Hasil validasi definisi saat dibaca; kosong berarti sah untuk disahkan.</summary>
        public List<string> ValidationErrors { get; set; } = new();
    }

    public class ClinicalInstrumentResponse : ClinicalInstrumentListItem
    {
        public string? Description { get; set; }
        public List<ClinicalInstrumentVersionResponse> Versions { get; set; } = new();
    }

    /// <summary>Versi yang berlaku bagi satu pasien dan jenis dokumen — dipanggil formulir.</summary>
    public class ResolvedInstrumentResponse
    {
        public Guid InstrumentId { get; set; }
        public string InstrumentCode { get; set; } = string.Empty;
        public string InstrumentName { get; set; } = string.Empty;
        public ClinicalInstrumentKind InstrumentKind { get; set; }
        public Guid VersionId { get; set; }
        public int VersionNumber { get; set; }
        public JsonElement Definition { get; set; }
        public string DefinitionHash { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public bool IsDraftAllowedInThisEnvironment { get; set; }
        public int? PatientAgeMonths { get; set; }
    }

    // =====================================================================
    // Jawaban instrumen pada dokumen pengkajian — api-contract 0.5.0 bagian 7.1
    // =====================================================================

    public class AssessmentInstrumentResponseRequest
    {
        public Guid InstrumentVersionId { get; set; }

        public Dictionary<string, JsonElement> Responses { get; set; } = new();
    }

    public class AssessmentInstrumentResultResponse
    {
        public Guid Id { get; set; }
        public Guid InstrumentVersionId { get; set; }
        public string InstrumentName { get; set; } = string.Empty;
        public ClinicalInstrumentKind InstrumentKind { get; set; }
        public int VersionNumber { get; set; }
        public ClinicalInstrumentVersionStatus VersionStatus { get; set; }
        public string DefinitionHashSnapshot { get; set; } = string.Empty;
        public JsonElement Responses { get; set; }
        public decimal? TotalScore { get; set; }
        public string? BandCode { get; set; }
        public string? BandLabel { get; set; }
        public bool IsAlertBand { get; set; }
        public DateTime? ComputedAt { get; set; }
    }
}
