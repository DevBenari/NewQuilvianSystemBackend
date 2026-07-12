using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    public class ConsultationFinalizationIssueResponse
    {
        public string IssueKey { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public ConsultationValidationSeverity Severity { get; set; }
        public string Section { get; set; } = string.Empty;
        public string TabKey { get; set; } = string.Empty;
        public string? Field { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public Guid? EntityId { get; set; }
    }

    public class ConsultationFinalizationSectionResponse
    {
        public string Section { get; set; } = string.Empty;
        public string TabKey { get; set; } = string.Empty;
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int InformationCount { get; set; }
        public List<ConsultationFinalizationIssueResponse> Issues { get; set; } = new();
    }

    public class ConsultationFinalizationValidationResponse
    {
        public Guid ConsultationId { get; set; }
        public bool CanFinalize { get; set; }
        public bool RequiresWarningAcknowledgement { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int InformationCount { get; set; }
        public List<ConsultationFinalizationSectionResponse> Sections { get; set; } = new();
    }

    public class FinalizeDoctorConsultationRequest : CompleteDoctorConsultationRequest
    {
        public DateTime? ExpectedUpdatedAt { get; set; }

        public List<string> AcknowledgedWarningKeys { get; set; } = new();

        [MaxLength(1000)]
        public string? FinalizationNote { get; set; }
    }

    public class ConsultationFinalizationResponse
    {
        public Guid ConsultationId { get; set; }
        public DateTime CompletedAt { get; set; }
        public Guid CompletedByUserId { get; set; }
        public int FinalizedPrescriptionCount { get; set; }
        public int FinalizedProcedureCount { get; set; }
        public ConsultationFinalizationValidationResponse Validation { get; set; } = new();
    }
}
