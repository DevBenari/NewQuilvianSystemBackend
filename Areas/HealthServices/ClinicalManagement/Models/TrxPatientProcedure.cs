using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    [Table("TrxPatientProcedure", Schema = "public")]
    public class TrxPatientProcedure : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid ConsultationId { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid DoctorId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        [Required]
        public Guid ProcedureId { get; set; }

        public Guid? TariffId { get; set; }

        public Guid? InsuranceTariffId { get; set; }

        public Guid? InsuranceCoverageRuleId { get; set; }

        // =========================
        // PROCEDURE SNAPSHOT
        // =========================
        [Required]
        [MaxLength(50)]
        public string ProcedureCodeSnapshot { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string ProcedureNameSnapshot { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ProcedureTypeSnapshot { get; set; }

        [MaxLength(100)]
        public string? ProcedureCategoryNameSnapshot { get; set; }

        [MaxLength(50)]
        public string ProcedureMasterType { get; set; } = "Master";
        // Master, Manual, External

        public bool IsFromMasterProcedure { get; set; } = true;

        public bool IsPrimaryProcedure { get; set; } = false;

        public bool IsEmergencyProcedure { get; set; } = false;

        public bool IsSurgeryRelated { get; set; } = false;

        public bool IsPackageProcedure { get; set; } = false;

        [MaxLength(100)]
        public string? PatientTypeSnapshot { get; set; }

        [MaxLength(100)]
        public string? PaymentTypeSnapshot { get; set; }

        [MaxLength(100)]
        public string? PatientClassNameSnapshot { get; set; }

        [MaxLength(200)]
        public string? InsuranceProviderNameSnapshot { get; set; }

        [MaxLength(150)]
        public string? BenefitPlanNameSnapshot { get; set; }

        [MaxLength(50)]
        public string? InsuranceTariffCodeSnapshot { get; set; }

        [MaxLength(250)]
        public string? InsuranceTariffNameSnapshot { get; set; }

        public PatientProcedureSource ProcedureSource { get; set; } = PatientProcedureSource.DoctorOrder;

        public PatientProcedureStatus ProcedureStatus { get; set; } = PatientProcedureStatus.Planned;

        public DateTime ProcedureDateTime { get; set; } = DateTime.UtcNow;

        public DateTime? PlannedAt { get; set; }

        public DateTime? ScheduledAt { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        // =========================
        // QUANTITY & PRICE SNAPSHOT
        // =========================
        public decimal Quantity { get; set; } = 1;

        [MaxLength(50)]
        public string? UnitNameSnapshot { get; set; }

        public decimal UnitPrice { get; set; } = 0;

        public decimal TotalPrice { get; set; } = 0;

        public decimal? HospitalPriceSnapshot { get; set; }

        public decimal? InsuranceContractPrice { get; set; }

        public bool IsFreeOfCharge { get; set; } = false;

        [MaxLength(250)]
        public string? FreeOfChargeReason { get; set; }

        public bool IsBillable { get; set; } = true;

        // =========================
        // INSURANCE / COVERAGE SNAPSHOT
        // =========================
        public bool IsCoveredByInsurance { get; set; } = false;

        [MaxLength(50)]
        public string CoverageStatus { get; set; } = "Unknown";
        // Unknown, Covered, PartiallyCovered, NotCovered, NeedApproval

        public decimal CoveragePercent { get; set; } = 0;

        public decimal CoveredAmount { get; set; } = 0;

        public decimal PatientPayAmount { get; set; } = 0;

        [MaxLength(250)]
        public string? CoverageNote { get; set; }

        public bool IsNeedApproval { get; set; } = false;

        public bool IsApproved { get; set; } = false;

        public DateTime? ApprovedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        [MaxLength(250)]
        public string? ApprovalNote { get; set; }

        // =========================
        // EXECUTION
        // =========================
        public bool IsExecuted { get; set; } = false;

        public DateTime? ExecutedAt { get; set; }

        public Guid? ExecutedByUserId { get; set; }

        public DateTime? PerformedAt { get; set; }

        public Guid? PerformedByUserId { get; set; }

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

        // =========================
        // BILLING
        // =========================
        public Guid? BillingItemId { get; set; }

        public bool IsBillingGenerated { get; set; } = false;

        public DateTime? BillingGeneratedAt { get; set; }

        // =========================
        // CANCEL
        // =========================
        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        [MaxLength(250)]
        public string? CancelReason { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxPatientEncounter? Encounter { get; set; }

        public TrxDoctorConsultation? Consultation { get; set; }

        public MstPatient? Patient { get; set; }

        public MstDoctor? Doctor { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstClinic? Clinic { get; set; }

        public MstProcedure? Procedure { get; set; }

        public MstTariff? Tariff { get; set; }

        public MstInsuranceTariff? InsuranceTariff { get; set; }

        public MstInsuranceCoverageRule? InsuranceCoverageRule { get; set; }

        public ApplicationUser? ApprovedByUser { get; set; }

        public ApplicationUser? ExecutedByUser { get; set; }

        public ApplicationUser? PerformedByUser { get; set; }

        public ApplicationUser? CancelledByUser { get; set; }
    }
}