using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;

// ===================================================================== permintaan

public class CreateGzOrderRequest
{
    [Required] public Guid PatientId { get; set; }
    [Required] public Guid EncounterId { get; set; }
    [Required] public Guid RequesterDoctorId { get; set; }
    public Guid? AssignedWorkforceId { get; set; }
    [Required] public GzOrderPriority Priority { get; set; } = GzOrderPriority.Routine;
    [Required, MaxLength(1000)] public string ReasonForReferral { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class UpdateGzOrderRequest
{
    public Guid? AssignedWorkforceId { get; set; }
    [Required] public GzOrderPriority Priority { get; set; }
    [Required, MaxLength(1000)] public string ReasonForReferral { get; set; } = string.Empty;
    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class CloseGzOrderRequest
{
    [Required, MaxLength(2000)] public string ClosingNote { get; set; } = string.Empty;
    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class CancelGzOrderRequest
{
    [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;
    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class SaveGzCareRecordRequest
{
    [Required] public Guid RecordedByWorkforceId { get; set; }
    public DateTime? VisitAt { get; set; }

    [Range(0.1, 500)] public decimal? Weight { get; set; }
    [Range(0.1, 300)] public decimal? Height { get; set; }
    [MaxLength(2000)] public string? AssessmentNote { get; set; }

    public Guid? NutritionDiagnosisId { get; set; }
    [MaxLength(1000)] public string? DiagnosisNote { get; set; }

    [MaxLength(2000)] public string? InterventionNote { get; set; }
    [MaxLength(500)] public string? DietPrescription { get; set; }
    [Range(1, 10000)] public int? EnergyRequirementKcal { get; set; }

    [MaxLength(2000)] public string? IntakeRecallNote { get; set; }
    [Range(0, 100)] public int? IntakePercent { get; set; }

    [MaxLength(2000)] public string? EvaluationNote { get; set; }

    public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class GzOrderPagedQuery
{
    public GzOrderStatus? Status { get; set; }
    public Guid? PatientId { get; set; }
    public Guid? AssignedWorkforceId { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

// ===================================================================== tanggapan

public class GzOrderSummaryResponse
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public Guid EncounterId { get; set; }
    public string RequesterDoctorName { get; set; } = string.Empty;
    public string? AssignedWorkforceName { get; set; }
    public GzOrderStatus Status { get; set; }
    public GzOrderPriority Priority { get; set; }
    public NutritionRiskStatus? ScreeningRiskStatus { get; set; }
    public DateTime RequestedAt { get; set; }
    public int VisitCount { get; set; }
    public DateTime? LastVisitAt { get; set; }
    public int Version { get; set; }
}

public class GzOrderDetailResponse : GzOrderSummaryResponse
{
    public Guid RequesterDoctorId { get; set; }
    public Guid? AssignedWorkforceId { get; set; }
    public string ReasonForReferral { get; set; } = string.Empty;
    public int? ScreeningScore { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ClosingNote { get; set; }
    public List<GzCareRecordResponse> CareRecords { get; set; } = [];
    public List<GzOrderHistoryResponse> Histories { get; set; } = [];
}

public class GzCareRecordResponse
{
    public Guid Id { get; set; }
    public Guid NutritionOrderId { get; set; }
    public int VisitSequence { get; set; }
    public DateTime VisitAt { get; set; }
    public Guid RecordedByWorkforceId { get; set; }
    public string RecordedByName { get; set; } = string.Empty;
    public GzCareRecordType RecordType { get; set; }

    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public decimal? Bmi { get; set; }
    public string? AssessmentNote { get; set; }

    public Guid? NutritionDiagnosisId { get; set; }
    public string? NutritionDiagnosisName { get; set; }
    public string? DiagnosisNote { get; set; }

    public string? InterventionNote { get; set; }
    public string? DietPrescription { get; set; }
    public int? EnergyRequirementKcal { get; set; }

    public string? IntakeRecallNote { get; set; }
    public int? IntakePercent { get; set; }

    public string? EvaluationNote { get; set; }
    public Guid? ProgressNoteId { get; set; }
    public int Version { get; set; }
}

public class GzOrderHistoryResponse
{
    public Guid Id { get; set; }
    public GzOrderStatus? FromStatus { get; set; }
    public GzOrderStatus ToStatus { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime OccurredAt { get; set; }
}

/// <summary>
/// Pasien rawat inap yang skrining gizinya menunjukkan risiko tetapi belum punya order.
/// </summary>
public class GzScreeningCandidateResponse
{
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public Guid EncounterId { get; set; }
    public string EncounterNumber { get; set; } = string.Empty;
    public NutritionRiskStatus RiskStatus { get; set; }
    public int? RiskScore { get; set; }
    public DateTime AssessedAt { get; set; }
}
