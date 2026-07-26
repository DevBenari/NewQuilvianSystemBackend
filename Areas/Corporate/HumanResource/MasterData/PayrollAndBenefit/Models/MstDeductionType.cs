using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models
{
    [Table("MstDeductionType", Schema = "public")]
    public class MstDeductionType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? PayrollComponentId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DeductionTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string DeductionTypeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string DeductionCategory { get; set; } = "General";
        // Statutory, Tax, Insurance, Loan, Attendance, Benefit, Other.

        [Required]
        [MaxLength(50)]
        public string CalculationMethod { get; set; } = "Fixed";
        // Fixed, Percentage, Formula, BalanceBased, ManualInput.

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal DefaultAmount { get; set; } = 0m;

        public decimal DefaultPercentage { get; set; } = 0m;

        public decimal? MaximumAmount { get; set; }

        public bool IsRecurring { get; set; } = true;

        public bool IsStatutory { get; set; } = false;

        public bool IsPreTax { get; set; } = false;

        public bool RequiresApproval { get; set; } = false;

        public bool AllowPartialDeduction { get; set; } = true;

        public int Priority { get; set; } = 0;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public MstPayrollComponent? PayrollComponent { get; set; }
    }
}
