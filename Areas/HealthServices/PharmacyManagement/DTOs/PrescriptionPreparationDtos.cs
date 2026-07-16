using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs
{
    public class PrescriptionPreparationResponse
    {
        public Guid Id { get; set; }
        public Guid PrescriptionId { get; set; }
        public PrescriptionPreparationStatus Status { get; set; }
        public Guid? PreparedByUserId { get; set; }
        public DateTime? PreparationStartedAt { get; set; }
        public DateTime? PreparationCompletedAt { get; set; }
        public string? PreparationNote { get; set; }
        public List<PrescriptionPreparationItemResponse> Items { get; set; } = new();
    }

    public class PrescriptionPreparationItemResponse
    {
        public Guid Id { get; set; }
        public Guid? PrescriptionItemId { get; set; }
        public Guid? PrescriptionCompoundItemId { get; set; }
        public Guid DrugId { get; set; }
        public decimal TheoreticalQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
        public decimal WasteQuantity { get; set; }
        public Guid? MeasurementId { get; set; }
        public string? MeasurementName { get; set; }
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Note { get; set; }
        public int SortOrder { get; set; }
    }

    public class StartPrescriptionPreparationRequest
    {
        [MaxLength(1000)] public string? PreparationNote { get; set; }
    }

    public class CompletePrescriptionPreparationRequest
    {
        [Required] public List<SavePrescriptionPreparationItemRequest> Items { get; set; } = new();
        [MaxLength(1000)] public string? PreparationNote { get; set; }
    }

    public class SavePrescriptionPreparationItemRequest
    {
        public Guid? PrescriptionItemId { get; set; }
        public Guid? PrescriptionCompoundItemId { get; set; }
        [Required] public Guid DrugId { get; set; }
        [Range(typeof(decimal), "0", "999999999")] public decimal TheoreticalQuantity { get; set; }
        [Range(typeof(decimal), "0.0001", "999999999")] public decimal ActualQuantity { get; set; }
        [Range(typeof(decimal), "0", "999999999")] public decimal WasteQuantity { get; set; }
        public Guid? MeasurementId { get; set; }
        [MaxLength(100)] public string? MeasurementName { get; set; }
        [MaxLength(100)] public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        [MaxLength(500)] public string? Note { get; set; }
        public int SortOrder { get; set; }
    }

    public class PrescriptionFinalCheckResponse
    {
        public Guid Id { get; set; }
        public Guid PrescriptionId { get; set; }
        public PrescriptionFinalCheckStatus Status { get; set; }
        public Guid? CheckedByUserId { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? CheckNote { get; set; }
        public List<PrescriptionFinalCheckItemResponse> Items { get; set; } = new();
    }

    public class PrescriptionFinalCheckItemResponse
    {
        public Guid Id { get; set; }
        public string CriterionCode { get; set; } = string.Empty;
        public string CriterionName { get; set; } = string.Empty;
        public PrescriptionReviewResult Result { get; set; }
        public string? Finding { get; set; }
        public int SortOrder { get; set; }
    }

    public class CompletePrescriptionFinalCheckRequest
    {
        [Required] public List<CompletePrescriptionFinalCheckItemRequest> Items { get; set; } = new();
        [MaxLength(1000)] public string? CheckNote { get; set; }
    }

    public class CompletePrescriptionFinalCheckItemRequest
    {
        [Required, MaxLength(80)] public string CriterionCode { get; set; } = string.Empty;
        [Required, MaxLength(200)] public string CriterionName { get; set; } = string.Empty;
        public PrescriptionReviewResult Result { get; set; }
        [MaxLength(1000)] public string? Finding { get; set; }
        public int SortOrder { get; set; }
    }
}
