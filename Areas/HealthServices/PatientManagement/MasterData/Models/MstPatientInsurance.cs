using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models
{
    [Table("MstPatientInsurance", Schema = "public")]
    public class MstPatientInsurance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid InsuranceProviderId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PolicyNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? CardNumber { get; set; }

        [MaxLength(100)]
        public string? MemberNumber { get; set; }

        [MaxLength(150)]
        public string? PlanName { get; set; }

        [MaxLength(100)]
        public string? ClassName { get; set; }

        [MaxLength(100)]
        public string? BenefitPlanCode { get; set; }

        [MaxLength(200)]
        public string? HolderName { get; set; }

        [MaxLength(50)]
        public string? HolderRelationship { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsEligible { get; set; } = true;

        public DateTime? LastEligibilityCheckAt { get; set; }

        [MaxLength(100)]
        public string? LastEligibilityReferenceNumber { get; set; }

        [MaxLength(250)]
        public string? EligibilityNote { get; set; }

        public decimal? AnnualLimitAmount { get; set; }

        public decimal? RemainingLimitAmount { get; set; }

        public decimal? CoPaymentPercent { get; set; }

        public decimal? CoPaymentAmount { get; set; }

        public bool IsNeedGuaranteeLetter { get; set; } = true;

        public bool IsNeedReferralLetter { get; set; } = false;

        public bool IsAllowExcessPaymentByPatient { get; set; } = true;

        [MaxLength(500)]
        public string? CardImagePath { get; set; }

        [MaxLength(250)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstPatient? Patient { get; set; }

        public MstInsuranceProvider? InsuranceProvider { get; set; }
    }
}
