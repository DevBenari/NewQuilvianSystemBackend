using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models
{
    [Table("MstAllowanceType", Schema = "public")]
    public class MstAllowanceType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? PayrollComponentId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AllowanceTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string AllowanceTypeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string AllowanceCategory { get; set; } = "General";
        // Fixed, Variable, Shift, OnCall, Hazard, Transport, Meal, Communication, Other.

        [Required]
        [MaxLength(50)]
        public string CalculationMethod { get; set; } = "Fixed";
        // Fixed, Percentage, PolicyBased, ManualInput.

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal DefaultAmount { get; set; } = 0m;

        public decimal DefaultPercentage { get; set; } = 0m;

        public decimal? MaximumAmount { get; set; }

        public bool IsRecurring { get; set; } = true;

        public bool IsTaxable { get; set; } = true;

        public bool IsProrated { get; set; } = true;

        public bool RequiresAttendance { get; set; } = false;

        public bool RequiresApproval { get; set; } = false;

        public bool IsIncludedInBaseSalary { get; set; } = false;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public MstPayrollComponent? PayrollComponent { get; set; }

        public ICollection<MstShiftAllowancePolicy> ShiftAllowancePolicies { get; set; }
            = new List<MstShiftAllowancePolicy>();

        public ICollection<MstOnCallAllowancePolicy> OnCallAllowancePolicies { get; set; }
            = new List<MstOnCallAllowancePolicy>();

        public ICollection<MstHazardAllowancePolicy> HazardAllowancePolicies { get; set; }
            = new List<MstHazardAllowancePolicy>();
    }
}
