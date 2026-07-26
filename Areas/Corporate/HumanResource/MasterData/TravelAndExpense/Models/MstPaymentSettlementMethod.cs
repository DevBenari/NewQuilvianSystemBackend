using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models
{
    [Table("MstPaymentSettlementMethod", Schema = "public")]
    public class MstPaymentSettlementMethod : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string SettlementMethodCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string SettlementMethodName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string SettlementType { get; set; } = "BankTransfer";
        // BankTransfer, Payroll, Cash, CorporateCard, OffsetAdvance, Other.

        public bool IsForTravelAdvance { get; set; } = true;

        public bool IsForTravelSettlement { get; set; } = true;

        public bool IsForExpenseReimbursement { get; set; } = true;

        public bool IsForEmployeeRefund { get; set; } = true;

        public bool RequiresEmployeeBankAccount { get; set; } = true;

        public bool RequiresPayrollCycle { get; set; } = false;

        public bool RequiresFinanceVerification { get; set; } = true;

        public decimal? MaximumSettlementAmount { get; set; }

        public int ProcessingDays { get; set; } = 0;

        public bool IsDefault { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
