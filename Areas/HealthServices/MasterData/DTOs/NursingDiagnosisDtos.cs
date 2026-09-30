using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class NursingDiagnosisSummaryResponse
    {
        public int TotalDiagnosis { get; set; }
        public int ActiveDiagnosis { get; set; }
        public int InactiveDiagnosis { get; set; }
        public int TotalGroups { get; set; }
        public int TotalInterventions { get; set; }
        public int TotalOutcomes { get; set; }
    }

    public class NursingDiagnosisListItemResponse
    {
        public Guid Id { get; set; }
        public Guid? GroupId { get; set; }
        public string? GroupCode { get; set; }
        public string? GroupName { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? SubCategory { get; set; }
        public string? Definition { get; set; }
        public string TerminologySystem { get; set; } = "SDKI";
        public bool IsActive { get; set; }
        public int EtiologyCount { get; set; }
        public int OutcomeCount { get; set; }
        public int InterventionCount { get; set; }
    }

    public class NursingDiagnosisDetailResponse : NursingDiagnosisListItemResponse
    {
        public List<NursingDiagnosisEtiologyResponse> Etiologies { get; set; } = new();
        public List<NursingDiagnosisOutcomeResponse> Outcomes { get; set; } = new();
        public List<NursingDiagnosisInterventionResponse> Interventions { get; set; } = new();
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class NursingDiagnosisOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? GroupName { get; set; }
        public string Label => $"{Code} - {Name}";
        public string FormattedAssessmentText => $"{Name} ({Code})";
    }

    public class NursingDiagnosisEtiologyResponse
    {
        public Guid Id { get; set; }
        public Guid NursingDiagnosisId { get; set; }
        public string EtiologyName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class NursingDiagnosisOutcomeResponse
    {
        public Guid Id { get; set; }
        public Guid NursingDiagnosisId { get; set; }
        public string OutcomeCode { get; set; } = string.Empty;
        public string OutcomeName { get; set; } = string.Empty;
        public string? Expectation { get; set; }
        public string TerminologySystem { get; set; } = "SLKI";
        public bool IsActive { get; set; }
    }

    public class NursingDiagnosisInterventionResponse
    {
        public Guid Id { get; set; }
        public Guid NursingDiagnosisId { get; set; }
        public int PillarType { get; set; } // 1: Observasi, 2: Terapeutik, 3: Edukasi, 4: Kolaborasi
        public string PillarTypeName => PillarType switch
        {
            1 => "Observasi",
            2 => "Terapeutik",
            3 => "Edukasi",
            4 => "Kolaborasi",
            _ => "Lainnya"
        };
        public string InterventionCode { get; set; } = string.Empty;
        public string InterventionName { get; set; } = string.Empty;
        public string ActionDescription { get; set; } = string.Empty;
        public string TerminologySystem { get; set; } = "SIKI";
        public bool IsDefaultRecommendation { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Bundel lengkap 3S (SDKI + Luaran SLKI + Intervensi 4 Pilar SIKI)
    /// untuk pengisian instan 1-klik pada form SOAP Keperawatan dan Rencana Asuhan.
    /// </summary>
    public class NursingDiagnosisBundleResponse
    {
        public Guid DiagnosisId { get; set; }
        public string DiagnosisCode { get; set; } = string.Empty;
        public string DiagnosisName { get; set; } = string.Empty;
        public string? SubCategory { get; set; }
        public List<string> RecommendedEtiologies { get; set; } = new();
        public List<NursingDiagnosisOutcomeResponse> Outcomes { get; set; } = new();
        public List<NursingDiagnosisInterventionResponse> Observations { get; set; } = new();
        public List<NursingDiagnosisInterventionResponse> Therapeutics { get; set; } = new();
        public List<NursingDiagnosisInterventionResponse> Educations { get; set; } = new();
        public List<NursingDiagnosisInterventionResponse> Collaborations { get; set; } = new();
        public string FormattedSoapAssessment { get; set; } = string.Empty;
        public string FormattedSoapPlanTemplate { get; set; } = string.Empty;
    }

    public class CreateNursingDiagnosisRequest
    {
        public Guid? GroupId { get; set; }

        [Required(ErrorMessage = "Kode diagnosis SDKI wajib diisi.")]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama diagnosis keperawatan wajib diisi.")]
        [MaxLength(300)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? SubCategory { get; set; }

        public string? Definition { get; set; }

        [MaxLength(50)]
        public string TerminologySystem { get; set; } = "SDKI";

        public bool IsActive { get; set; } = true;
    }

    public class UpdateNursingDiagnosisRequest : CreateNursingDiagnosisRequest
    {
    }

    public class CreateNursingDiagnosisInterventionRequest
    {
        [Required]
        public Guid NursingDiagnosisId { get; set; }

        [Range(1, 4, ErrorMessage = "PillarType harus antara 1 (Observasi) hingga 4 (Kolaborasi).")]
        public int PillarType { get; set; }

        [Required]
        [MaxLength(50)]
        public string InterventionCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string InterventionName { get; set; } = string.Empty;

        [Required]
        public string ActionDescription { get; set; } = string.Empty;

        [MaxLength(50)]
        public string TerminologySystem { get; set; } = "SIKI";

        public bool IsDefaultRecommendation { get; set; } = true;

        public bool IsActive { get; set; } = true;
    }

    public class CreateNursingDiagnosisOutcomeRequest
    {
        [Required]
        public Guid NursingDiagnosisId { get; set; }

        [Required]
        [MaxLength(50)]
        public string OutcomeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string OutcomeName { get; set; } = string.Empty;

        public string? Expectation { get; set; }

        [MaxLength(50)]
        public string TerminologySystem { get; set; } = "SLKI";

        public bool IsActive { get; set; } = true;
    }

    public class CreateNursingDiagnosisEtiologyRequest
    {
        [Required]
        public Guid NursingDiagnosisId { get; set; }

        [Required]
        [MaxLength(300)]
        public string EtiologyName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
