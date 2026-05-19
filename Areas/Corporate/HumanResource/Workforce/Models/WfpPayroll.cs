using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models
{
    [Table("WfpPayroll", Schema = "public")]
    public class WfpPayroll : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [MaxLength(50)]
        public string PayrollGroup { get; set; } = "Default";

        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "BankTransfer";

        public Guid? PrimaryBankAccountId { get; set; }

        public decimal BasicSalary { get; set; } = 0;

        public decimal FixedAllowance { get; set; } = 0;

        public decimal FixedDeduction { get; set; } = 0;

        public bool IsOvertimeEligible { get; set; } = false;

        public bool IsPayrollActive { get; set; } = true;

        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public WfpBankAccount? PrimaryBankAccount { get; set; }
    }
}