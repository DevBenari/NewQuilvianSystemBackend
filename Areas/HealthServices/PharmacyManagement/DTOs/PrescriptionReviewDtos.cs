using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs
{
    public class PrescriptionReviewResponse
    {
        public Guid Id { get; set; }
        public Guid PrescriptionId { get; set; }
        public int ReviewVersion { get; set; }
        public PrescriptionReviewStatus Status { get; set; }
        public Guid? ReviewedByPharmacistId { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool HasAdministrativeProblem { get; set; }
        public bool HasPharmaceuticalProblem { get; set; }
        public bool HasClinicalProblem { get; set; }
        public bool HasCompoundFormulaProblem { get; set; }
        public bool RequiresDoctorClarification { get; set; }
        public string? GeneralNote { get; set; }
        public List<PrescriptionReviewItemResponse> Items { get; set; } = new();
        public List<PrescriptionClarificationResponse> Clarifications { get; set; } = new();
    }

    public class PrescriptionReviewItemResponse
    {
        public Guid Id { get; set; }
        public Guid? CriterionId { get; set; }
        public PrescriptionReviewCategory Category { get; set; }
        public string CriterionCode { get; set; } = string.Empty;
        public string CriterionName { get; set; } = string.Empty;
        public PrescriptionReviewResult Result { get; set; }
        public PrescriptionIssueSeverity Severity { get; set; }
        public string? Finding { get; set; }
        public string? Recommendation { get; set; }
        public bool IsSystemDetected { get; set; }
        public string? SystemRuleCode { get; set; }
        public Guid? PrescriptionItemId { get; set; }
        public Guid? PrescriptionCompoundId { get; set; }
        public Guid? PrescriptionCompoundItemId { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int SortOrder { get; set; }
    }

    public class StartPrescriptionReviewRequest
    {
        [MaxLength(1000)] public string? GeneralNote { get; set; }
    }

    public class UpdatePrescriptionReviewItemsRequest
    {
        [Required] public List<UpdatePrescriptionReviewItemRequest> Items { get; set; } = new();
        [MaxLength(1000)] public string? GeneralNote { get; set; }
    }

    public class UpdatePrescriptionReviewItemRequest
    {
        [Required] public Guid ReviewItemId { get; set; }
        public PrescriptionReviewResult Result { get; set; }
        public PrescriptionIssueSeverity? Severity { get; set; }
        [MaxLength(1000)] public string? Finding { get; set; }
        [MaxLength(1000)] public string? Recommendation { get; set; }
    }

    public class CompletePrescriptionReviewRequest
    {
        [MaxLength(1000)] public string? GeneralNote { get; set; }
    }

    public class CreatePrescriptionClarificationRequest
    {
        public Guid? PrescriptionReviewItemId { get; set; }
        public Guid? PrescriptionItemId { get; set; }
        public Guid? PrescriptionCompoundId { get; set; }
        public Guid? PrescriptionCompoundItemId { get; set; }
        [Required, MaxLength(100)] public string ProblemCode { get; set; } = string.Empty;
        [Required, MaxLength(2000)] public string ProblemDescription { get; set; } = string.Empty;
        [MaxLength(2000)] public string? PharmacistRecommendation { get; set; }
        public PrescriptionIssueSeverity Severity { get; set; } = PrescriptionIssueSeverity.Warning;
    }

    public class DoctorClarificationResponseRequest
    {
        [Required, MaxLength(2000)] public string DoctorResponse { get; set; } = string.Empty;
        public bool PrescriptionWasRevised { get; set; }
    }

    public class ClosePrescriptionClarificationRequest
    {
        public bool Accepted { get; set; }
        [MaxLength(1000)] public string? ClosingNote { get; set; }
    }

    public class PrescriptionClarificationResponse
    {
        public Guid Id { get; set; }
        public Guid PrescriptionId { get; set; }
        public Guid PrescriptionReviewId { get; set; }
        public Guid? PrescriptionReviewItemId { get; set; }
        public string ProblemCode { get; set; } = string.Empty;
        public string ProblemDescription { get; set; } = string.Empty;
        public string? PharmacistRecommendation { get; set; }
        public PrescriptionIssueSeverity Severity { get; set; }
        public PrescriptionClarificationStatus Status { get; set; }
        public Guid RequestedByPharmacistId { get; set; }
        public DateTime RequestedAt { get; set; }
        public Guid? RespondedByDoctorId { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string? DoctorResponse { get; set; }
        public Guid? ClosedByUserId { get; set; }
        public DateTime? ClosedAt { get; set; }
    }
}
