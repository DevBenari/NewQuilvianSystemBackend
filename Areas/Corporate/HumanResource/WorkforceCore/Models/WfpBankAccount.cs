using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("WfpBankAccount", Schema = "public")]
    public class WfpBankAccount : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? BankId { get; set; }

        [MaxLength(200)]
        public string? BankName { get; set; }

        [Required]
        [MaxLength(100)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string AccountHolderName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? BankBranch { get; set; }

        [Required]
        [MaxLength(50)]
        public string AccountType { get; set; } = "Savings";

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public bool IsPayrollAccount { get; set; } = true;
        public bool IsPrimary { get; set; } = false;
        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstBank? Bank { get; set; }
    }
}
