using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("WfpSalaryAssignment", Schema = "public")]
    public class WfpSalaryAssignment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid SalaryStructureId { get; set; }

        [Required]
        public Guid SalaryGradeId { get; set; }

        public Guid? EmployeeGradeId { get; set; }
        public Guid? PayrollPeriodId { get; set; }

        [Required]
        public decimal BaseSalary { get; set; }

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        [Required]
        [MaxLength(50)]
        public string PaymentFrequency { get; set; } = "Monthly";

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }
        public bool IsPrimary { get; set; } = true;
        public bool IsConfidential { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstSalaryStructure? SalaryStructure { get; set; }
        public MstSalaryGrade? SalaryGrade { get; set; }
        public MstEmployeeGrade? EmployeeGrade { get; set; }
        public MstPayrollPeriod? PayrollPeriod { get; set; }
    }
}
