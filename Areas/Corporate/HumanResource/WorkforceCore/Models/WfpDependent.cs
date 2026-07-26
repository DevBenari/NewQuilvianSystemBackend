using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("WfpDependent", Schema = "public")]
    public class WfpDependent : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? FamilyMemberId { get; set; }
        public Guid? BenefitPlanId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DependentType { get; set; } = "Family";

        [Required]
        [MaxLength(50)]
        public string DependentStatus { get; set; } = "Active";

        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsTaxDependent { get; set; } = false;
        public bool IsBenefitEligible { get; set; } = true;
        public bool IsInsuranceEligible { get; set; } = true;
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public WfpFamilyMember? FamilyMember { get; set; }
        public MstBenefitPlan? BenefitPlan { get; set; }
    }
}
