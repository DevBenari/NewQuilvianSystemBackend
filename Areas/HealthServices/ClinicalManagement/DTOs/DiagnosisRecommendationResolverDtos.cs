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

        /// <summary>
        /// Nama obat dari <c>MstDrug</c> — <c>BE-RWI-142</c>. Tanpa ini rekomendasi hanya menyebut
        /// indikasi dan dosis, dan dokter tidak tahu obat apa yang dimaksud.
        /// </summary>
        public string DrugName { get; set; } = string.Empty;

        /// <summary>Kekuatan sediaan, mis. "500 mg"; kosong bila master tidak mengisinya.</summary>
        public string? DrugStrength { get; set; }

        /// <summary>Bentuk sediaan, mis. "Tablet", "Injeksi"; kosong bila master tidak mengisinya.</summary>
        public string? DrugForm { get; set; }

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

        /// <summary>Kode tindakan dari <c>MstProcedure</c> bila rekomendasi menunjuk master — <c>BE-RWI-142</c>.</summary>
        public string? ProcedureCode { get; set; }

        /// <summary>Nama tindakan dari <c>MstProcedure</c> bila rekomendasi menunjuk master — <c>BE-RWI-142</c>.</summary>
        public string? ProcedureName { get; set; }

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
