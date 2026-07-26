using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models
{
    [Table("MstSalaryGrade", Schema = "public")]
    public class MstSalaryGrade : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? EmployeeGradeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string SalaryGradeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string SalaryGradeName { get; set; } = string.Empty;

        public int GradeLevel { get; set; } = 0;

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal MinimumSalary { get; set; } = 0m;

        public decimal MidpointSalary { get; set; } = 0m;

        public decimal MaximumSalary { get; set; } = 0m;

        public decimal AnnualIncrementPercentage { get; set; } = 0m;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public MstEmployeeGrade? EmployeeGrade { get; set; }

        public ICollection<MstSalaryStructure> SalaryStructures { get; set; }
            = new List<MstSalaryStructure>();

        public ICollection<MstBenefitEligibilityRule> BenefitEligibilityRules { get; set; }
            = new List<MstBenefitEligibilityRule>();
    }
}
