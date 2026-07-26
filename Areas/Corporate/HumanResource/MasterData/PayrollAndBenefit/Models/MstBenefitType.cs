using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models
{
    [Table("MstBenefitType", Schema = "public")]
    public class MstBenefitType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string BenefitTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string BenefitTypeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string BenefitCategory { get; set; } = "Other";
        // HealthInsurance, LifeInsurance, Retirement, Medical, Wellness,
        // Meal, Transport, Communication, Education, Other.

        [Required]
        [MaxLength(50)]
        public string FundingType { get; set; } = "Employer";
        // Employer, Employee, Shared.

        public bool IsTaxable { get; set; } = false;

        public bool RequiresEnrollment { get; set; } = true;

        public bool AllowsDependents { get; set; } = false;

        public int MaximumDependents { get; set; } = 0;

        public bool IsClaimBased { get; set; } = false;

        public bool RequiresEvidence { get; set; } = false;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public ICollection<MstBenefitPlan> BenefitPlans { get; set; }
            = new List<MstBenefitPlan>();
    }
}
