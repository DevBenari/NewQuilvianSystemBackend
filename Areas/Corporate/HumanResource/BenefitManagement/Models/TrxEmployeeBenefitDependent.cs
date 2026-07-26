using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models
{
    [Table("TrxEmployeeBenefitDependent", Schema = "public")]
    public class TrxEmployeeBenefitDependent : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EmployeeBenefitEnrollmentId { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public Guid? FamilyMemberId { get; set; }

        public Guid? DependentId { get; set; }

        [Required]
        [MaxLength(200)]
        public string DependentName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RelationshipType { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? IdentityNumber { get; set; }

        public DateTime? BirthDate { get; set; }

        [MaxLength(30)]
        public string? GenderSnapshot { get; set; }

        [Required]
        [MaxLength(30)]
        public string DependentStatus { get; set; } = "Draft";

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public decimal CoverageLimitAmount { get; set; } = 0m;

        public decimal UsedAmount { get; set; } = 0m;

        public decimal RemainingAmount { get; set; } = 0m;

        public bool IsEligible { get; set; } = false;

        public bool IsCovered { get; set; } = false;

        public bool IsPrimaryDependent { get; set; } = false;

        [MaxLength(1000)]
        public string? EligibilityReason { get; set; }

        public string? SupportingDocumentJson { get; set; }

        public DateTime? RequestedAt { get; set; }

        public Guid? RequestedByUserId { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public Guid? VerifiedByUserId { get; set; }

        [MaxLength(1000)]
        public string? VerificationNotes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxEmployeeBenefitEnrollment? EmployeeBenefitEnrollment { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public WfpFamilyMember? FamilyMember { get; set; }
        public WfpDependent? Dependent { get; set; }
        public ApplicationUser? RequestedByUser { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }

    }
}
