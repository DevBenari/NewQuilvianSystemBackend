using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    [Table("TrxPayrollVariableInput", Schema = "public")]
    public class TrxPayrollVariableInput : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PayrollRunEmployeeId { get; set; }

        [Required]
        public Guid PayrollComponentId { get; set; }

        [Required, MaxLength(50)]
        public string InputNumber { get; set; } = string.Empty;

        public DateOnly InputDate { get; set; }

        [Required, MaxLength(30)]
        public string InputType { get; set; } = "Manual";
        // Manual, Import, Expense, Travel, MedicalServiceFee, Incentive, Penalty.

        [Required, MaxLength(30)]
        public string InputStatus { get; set; } = "Draft";
        // Draft, Submitted, Verified, Approved, Applied, Rejected, Cancelled.

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal Quantity { get; set; } = 1m;
        public decimal Rate { get; set; } = 0m;
        public decimal Amount { get; set; } = 0m;

        [MaxLength(50)]
        public string? SourceType { get; set; }

        public Guid? SourceId { get; set; }

        [MaxLength(500)]
        public string? AttachmentPath { get; set; }

        public DateTime? SubmittedAt { get; set; }
        public Guid? SubmittedByUserId { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedByUserId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxPayrollRunEmployee? PayrollRunEmployee { get; set; }
        public MstPayrollComponent? PayrollComponent { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }
    }
}
