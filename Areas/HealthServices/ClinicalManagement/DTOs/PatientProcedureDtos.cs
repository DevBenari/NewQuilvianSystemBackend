using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    public class PatientProcedureResponse
    {
        public Guid Id { get; set; }

        public Guid EncounterId { get; set; }
        public string EncounterNumber { get; set; } = string.Empty;

        public Guid ConsultationId { get; set; }
        public string ConsultationNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;

        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;

        public Guid? ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }

        public Guid? ClinicId { get; set; }
        public string? ClinicName { get; set; }

        public Guid ProcedureId { get; set; }
        public Guid? TariffId { get; set; }
        public Guid? InsuranceTariffId { get; set; }
        public Guid? InsuranceCoverageRuleId { get; set; }

        public string ProcedureCodeSnapshot { get; set; } = string.Empty;
        public string ProcedureNameSnapshot { get; set; } = string.Empty;
        public string? ProcedureTypeSnapshot { get; set; }
        public string? ProcedureCategoryNameSnapshot { get; set; }

        public string ProcedureMasterType { get; set; } = string.Empty;
        public bool IsFromMasterProcedure { get; set; }
        public bool IsPrimaryProcedure { get; set; }
        public bool IsEmergencyProcedure { get; set; }
        public bool IsSurgeryRelated { get; set; }
        public bool IsPackageProcedure { get; set; }

        public PatientProcedureSource ProcedureSource { get; set; }
        public PatientProcedureStatus ProcedureStatus { get; set; }

        public DateTime ProcedureDateTime { get; set; }
        public DateTime? PlannedAt { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public decimal Quantity { get; set; }
        public string? UnitNameSnapshot { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        public decimal? HospitalPriceSnapshot { get; set; }
        public decimal? InsuranceContractPrice { get; set; }

        public bool IsFreeOfCharge { get; set; }
        public bool IsBillable { get; set; }

        public bool IsCoveredByInsurance { get; set; }
        public string CoverageStatus { get; set; } = string.Empty;
        public decimal CoveragePercent { get; set; }
        public decimal CoveredAmount { get; set; }
        public decimal PatientPayAmount { get; set; }

        public bool IsNeedApproval { get; set; }
        public bool IsApproved { get; set; }

        public bool IsExecuted { get; set; }
        public DateTime? ExecutedAt { get; set; }

        public DateTime? PerformedAt { get; set; }
        public Guid? PerformedByUserId { get; set; }
        public string? PerformedByUserName { get; set; }

        public bool IsBillingGenerated { get; set; }
        public DateTime? BillingGeneratedAt { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class PatientProcedureDetailResponse : PatientProcedureResponse
    {
        public string? PatientTypeSnapshot { get; set; }
        public string? PaymentTypeSnapshot { get; set; }
        public string? PatientClassNameSnapshot { get; set; }

        public string? InsuranceProviderNameSnapshot { get; set; }
        public string? BenefitPlanNameSnapshot { get; set; }
        public string? InsuranceTariffCodeSnapshot { get; set; }
        public string? InsuranceTariffNameSnapshot { get; set; }

        public string? FreeOfChargeReason { get; set; }
        public string? CoverageNote { get; set; }

        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByUserName { get; set; }
        public string? ApprovalNote { get; set; }

        public Guid? ExecutedByUserId { get; set; }
        public string? ExecutedByUserName { get; set; }

        public string? ClinicalNote { get; set; }
        public string? ResultNote { get; set; }
        public string? InstructionNote { get; set; }
        public string? DispositionNote { get; set; }
        public string? ComplicationNote { get; set; }
        public string? FollowUpInstruction { get; set; }

        public Guid? BillingItemId { get; set; }

        public DateTime? CancelledAt { get; set; }
        public Guid? CancelledByUserId { get; set; }
        public string? CancelledByUserName { get; set; }
        public string? CancelReason { get; set; }
    }

    public class PatientProcedureOptionResponse
    {
        public Guid Id { get; set; }
        public Guid ProcedureId { get; set; }
        public string ProcedureCodeSnapshot { get; set; } = string.Empty;
        public string ProcedureNameSnapshot { get; set; } = string.Empty;
        public PatientProcedureStatus ProcedureStatus { get; set; }
        public bool IsPrimaryProcedure { get; set; }
        public bool IsExecuted { get; set; }
        public bool IsBillable { get; set; }
    }

    public class PatientProcedureFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public PatientProcedureDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PatientProcedureSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<PatientProcedureEnumOptionResponse> ProcedureSourceOptions { get; set; } = new();
        public List<PatientProcedureEnumOptionResponse> ProcedureStatusOptions { get; set; } = new();
    }

    public class PatientProcedureDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid? ConsultationId { get; set; }
        public Guid? PatientId { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public Guid? ClinicId { get; set; }
        public Guid? ProcedureId { get; set; }
        public PatientProcedureStatus? ProcedureStatus { get; set; }
        public bool? IsPrimaryProcedure { get; set; }
        public bool? IsEmergencyProcedure { get; set; }
        public bool? IsBillable { get; set; }
        public bool? IsFreeOfCharge { get; set; }
        public bool? IsCoveredByInsurance { get; set; }
        public bool? IsNeedApproval { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsExecuted { get; set; }
        public bool? IsBillingGenerated { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SortBy { get; set; } = "procedureDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class PatientProcedureSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class PatientProcedureEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreatePatientProcedureRequest
    {
        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid ConsultationId { get; set; }

        [Required]
        public Guid ProcedureId { get; set; }

        public Guid? TariffId { get; set; }

        public Guid? InsuranceTariffId { get; set; }

        public Guid? InsuranceCoverageRuleId { get; set; }

        public PatientProcedureSource ProcedureSource { get; set; } = PatientProcedureSource.DoctorOrder;

        public DateTime? ProcedureDateTime { get; set; }

        public DateTime? PlannedAt { get; set; }

        public DateTime? ScheduledAt { get; set; }

        public decimal Quantity { get; set; } = 1;

        [MaxLength(50)]
        public string? UnitNameSnapshot { get; set; }

        public bool IsPrimaryProcedure { get; set; } = false;

        public bool IsEmergencyProcedure { get; set; } = false;

        public bool IsSurgeryRelated { get; set; } = false;

        public bool IsPackageProcedure { get; set; } = false;

        public bool IsFreeOfCharge { get; set; } = false;

        [MaxLength(250)]
        public string? FreeOfChargeReason { get; set; }

        public bool IsCoveredByInsurance { get; set; } = false;

        [MaxLength(50)]
        public string CoverageStatus { get; set; } = "Unknown";

        public decimal CoveragePercent { get; set; } = 0;

        [MaxLength(250)]
        public string? CoverageNote { get; set; }

        public bool IsNeedApproval { get; set; } = false;

        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }

        [MaxLength(500)]
        public string? InstructionNote { get; set; }

        [MaxLength(500)]
        public string? DispositionNote { get; set; }

        [MaxLength(500)]
        public string? ComplicationNote { get; set; }

        [MaxLength(500)]
        public string? FollowUpInstruction { get; set; }

        public bool ExecuteImmediately { get; set; } = false;
    }

    public class UpdatePatientProcedureRequest
    {
        public Guid? TariffId { get; set; }

        public Guid? InsuranceTariffId { get; set; }

        public Guid? InsuranceCoverageRuleId { get; set; }

        public PatientProcedureSource ProcedureSource { get; set; } = PatientProcedureSource.DoctorOrder;

        public DateTime? ProcedureDateTime { get; set; }

        public DateTime? PlannedAt { get; set; }

        public DateTime? ScheduledAt { get; set; }

        public decimal Quantity { get; set; } = 1;

        [MaxLength(50)]
        public string? UnitNameSnapshot { get; set; }

        public bool IsPrimaryProcedure { get; set; } = false;

        public bool IsEmergencyProcedure { get; set; } = false;

        public bool IsSurgeryRelated { get; set; } = false;

        public bool IsPackageProcedure { get; set; } = false;

        public bool IsFreeOfCharge { get; set; } = false;

        [MaxLength(250)]
        public string? FreeOfChargeReason { get; set; }

        public bool IsCoveredByInsurance { get; set; } = false;

        [MaxLength(50)]
        public string CoverageStatus { get; set; } = "Unknown";

        public decimal CoveragePercent { get; set; } = 0;

        [MaxLength(250)]
        public string? CoverageNote { get; set; }

        public bool IsNeedApproval { get; set; } = false;

        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }

        [MaxLength(1000)]
        public string? ResultNote { get; set; }

        [MaxLength(500)]
        public string? InstructionNote { get; set; }

        [MaxLength(500)]
        public string? DispositionNote { get; set; }

        [MaxLength(500)]
        public string? ComplicationNote { get; set; }

        [MaxLength(500)]
        public string? FollowUpInstruction { get; set; }
    }

    public class PatientProcedureCreateResponse
    {
        public Guid Id { get; set; }

        public Guid EncounterId { get; set; }

        public Guid ConsultationId { get; set; }

        public Guid ProcedureId { get; set; }

        public Guid? TariffId { get; set; }

        public Guid? InsuranceTariffId { get; set; }

        public Guid? InsuranceCoverageRuleId { get; set; }

        public string ProcedureCodeSnapshot { get; set; } = string.Empty;

        public string ProcedureNameSnapshot { get; set; } = string.Empty;

        public string ProcedureMasterType { get; set; } = string.Empty;

        public bool IsFromMasterProcedure { get; set; }

        public bool IsPrimaryProcedure { get; set; }

        public bool IsEmergencyProcedure { get; set; }

        public PatientProcedureStatus ProcedureStatus { get; set; }

        public DateTime ProcedureDateTime { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public decimal CoveredAmount { get; set; }

        public decimal PatientPayAmount { get; set; }

        public bool IsFreeOfCharge { get; set; }

        public bool IsBillable { get; set; }

        public bool IsCoveredByInsurance { get; set; }

        public string CoverageStatus { get; set; } = string.Empty;

        public bool IsNeedApproval { get; set; }

        public bool IsExecuted { get; set; }

        public int ProcedureCount { get; set; }

        public bool HasProcedure { get; set; }

        public string? ProcedureText { get; set; }
    }

    public class PatientProcedureUpdateResponse : PatientProcedureCreateResponse
    {
    }

    public class ApprovePatientProcedureRequest
    {
        [MaxLength(250)]
        public string? ApprovalNote { get; set; }
    }

    public class ExecutePatientProcedureRequest
    {
        public DateTime? PerformedAt { get; set; }

        [MaxLength(1000)]
        public string? ResultNote { get; set; }

        [MaxLength(500)]
        public string? DispositionNote { get; set; }

        [MaxLength(500)]
        public string? ComplicationNote { get; set; }

        [MaxLength(500)]
        public string? FollowUpInstruction { get; set; }
    }

    public class CancelPatientProcedureRequest
    {
        [Required]
        [MaxLength(250)]
        public string CancelReason { get; set; } = string.Empty;
    }
}