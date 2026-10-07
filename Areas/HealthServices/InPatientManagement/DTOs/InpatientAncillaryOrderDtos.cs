using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;

public enum AncillaryOrderItemType
{
    Laboratory = 1, Radiology = 2, Procedure = 3, Nutrition = 4, Blood = 5
}

public class AncillaryCoverageQuery
{
    [Required, EnumDataType(typeof(AncillaryOrderItemType))]
    public AncillaryOrderItemType ItemType { get; set; }
    public List<Guid> ItemIds { get; set; } = new();
}

public class CoverageStatusItem
{
    public Guid ItemId { get; set; }
    public bool? IsCovered { get; set; }
    public string Label { get; set; } = string.Empty;
    public string PriceStatus { get; set; } = "NOT_ESTIMABLE";
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? EstimatedUnitPrice { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PriceLabel { get; set; }
}

public class CreateInpatientNutritionConsultationRequest
{
    public Guid? RequesterDoctorId { get; set; }
    [EnumDataType(typeof(GziOrderPriority))]
    public GziOrderPriority Priority { get; set; } = GziOrderPriority.Routine;
    [Required, MaxLength(1000)] public string ReasonForReferral { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class CreateInpatientBloodOrderRequest
{
    public Guid? RequestingDoctorId { get; set; }
    [EnumDataType(typeof(BloodType))] public BloodType? RequestedBloodGroup { get; set; }
    [Required, MinLength(1)] public List<BloodOrderLineRequest> Lines { get; set; } = new();
    [MaxLength(500)] public string? ClinicalNote { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class ConfirmInpatientDuplicateBloodOrderRequest : CreateInpatientBloodOrderRequest
{
    [Required, MaxLength(500)] public string DuplicateOverrideReason { get; set; } = string.Empty;
}
