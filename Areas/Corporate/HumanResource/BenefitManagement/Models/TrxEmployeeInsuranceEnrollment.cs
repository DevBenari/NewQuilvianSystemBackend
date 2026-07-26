using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models
{
    [Table("TrxEmployeeInsuranceEnrollment", Schema = "public")]
    public class TrxEmployeeInsuranceEnrollment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EmployeeBenefitEnrollmentId { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid BenefitPlanId { get; set; }

        public Guid? InsuranceProfileId { get; set; }

        [Required]
        [MaxLength(50)]
        public string InsuranceEnrollmentNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string InsuranceType { get; set; } = "Private";

        [Required]
        [MaxLength(200)]
        public string ProviderName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? PolicyNumber { get; set; }

        [MaxLength(100)]
        public string? MemberNumber { get; set; }

        [MaxLength(100)]
        public string? CoverageClass { get; set; }

        [Required]
        [MaxLength(30)]
        public string EnrollmentStatus { get; set; } = "Draft";

        public DateTime? SubmittedDate { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal PremiumAmount { get; set; } = 0m;

        public decimal EmployerContributionAmount { get; set; } = 0m;

        public decimal EmployeeContributionAmount { get; set; } = 0m;

        public int CoveredDependentCount { get; set; } = 0;

        [MaxLength(100)]
        public string? ExternalReferenceNumber { get; set; }

        public string? ExternalResponseJson { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public Guid? VerifiedByUserId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxEmployeeBenefitEnrollment? EmployeeBenefitEnrollment { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public MstBenefitPlan? BenefitPlan { get; set; }
        public WfpInsurance? InsuranceProfile { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }

    }
}
