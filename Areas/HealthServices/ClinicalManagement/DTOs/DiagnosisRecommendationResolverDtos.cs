using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    public class ResolveDiagnosisRecommendationRequest
    {
        [Required]
        public List<Guid> DiagnosisIds { get; set; } = new();
    }

    public class ResolveDiagnosisRecommendationResponse
    {
        public bool HasRecommendation { get; set; }
        public List<ResolvedDiagnosisDrugRecommendationResponse> DrugRecommendations { get; set; } = new();
        public List<ResolvedDiagnosisProcedureRecommendationResponse> ProcedureRecommendations { get; set; } = new();
        public List<ResolvedDiagnosisEducationRecommendationResponse> EducationRecommendations { get; set; } = new();
    }

    public class ResolvedDiagnosisDrugRecommendationResponse
    {
        public Guid Id { get; set; }
        public Guid DiagnosisId { get; set; }
        public string DiagnosisCode { get; set; } = string.Empty;
        public string DiagnosisName { get; set; } = string.Empty;
        public Guid DrugId { get; set; }
        public string RecommendationType { get; set; } = string.Empty;
        public string RecommendationTypeName { get; set; } = string.Empty;
        public string? IndicationText { get; set; }
        public string? DoseText { get; set; }
        public string? Route { get; set; }
        public string? Frequency { get; set; }
        public string? DurationText { get; set; }
        public string? CautionNote { get; set; }
        public string? SourceType { get; set; }
        public string? SourceTitle { get; set; }
        public string? SourceYear { get; set; }
    }

    public class ResolvedDiagnosisProcedureRecommendationResponse
    {
        public Guid Id { get; set; }
        public Guid DiagnosisId { get; set; }
        public string DiagnosisCode { get; set; } = string.Empty;
        public string DiagnosisName { get; set; } = string.Empty;
        public Guid? ProcedureId { get; set; }
        public string RecommendationType { get; set; } = string.Empty;
        public string RecommendationTypeName { get; set; } = string.Empty;
        public string RecommendationName { get; set; } = string.Empty;
        public string? InstructionText { get; set; }
        public string? SourceType { get; set; }
        public string? SourceTitle { get; set; }
        public string? SourceYear { get; set; }
    }

    public class ResolvedDiagnosisEducationRecommendationResponse
    {
        public Guid Id { get; set; }
        public Guid DiagnosisId { get; set; }
        public string DiagnosisCode { get; set; } = string.Empty;
        public string DiagnosisName { get; set; } = string.Empty;
        public string EducationType { get; set; } = string.Empty;
        public string EducationTypeName { get; set; } = string.Empty;
        public string EducationTitle { get; set; } = string.Empty;
        public string EducationText { get; set; } = string.Empty;
        public string? SourceType { get; set; }
        public string? SourceTitle { get; set; }
        public string? SourceYear { get; set; }
    }
}
