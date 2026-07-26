using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models
{
    [Table("TrxEmployeeLoanInstallment", Schema = "public")]
    public class TrxEmployeeLoanInstallment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EmployeeLoanId { get; set; }

        public Guid? PayrollPeriodId { get; set; }

        public Guid? PayrollRunEmployeeId { get; set; }

        public Guid? PayrollComponentId { get; set; }

        public int InstallmentNumber { get; set; } = 1;

        public DateTime DueDate { get; set; }

        public decimal ExpectedAmount { get; set; } = 0m;

        public decimal PrincipalAmount { get; set; } = 0m;

        public decimal InterestAmount { get; set; } = 0m;

        public decimal PaidAmount { get; set; } = 0m;

        public decimal OutstandingAmount { get; set; } = 0m;

        [Required]
        [MaxLength(30)]
        public string InstallmentStatus { get; set; } = "Scheduled";

        public DateTime? PaidAt { get; set; }

        public Guid? PaidByUserId { get; set; }

        public DateTime? PostedAt { get; set; }

        public Guid? PostedByUserId { get; set; }

        [MaxLength(100)]
        public string? PaymentReferenceNumber { get; set; }

        public Guid? FinanceTransactionId { get; set; }

        public Guid? GlHeaderId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxEmployeeLoan? EmployeeLoan { get; set; }
        public MstPayrollPeriod? PayrollPeriod { get; set; }
        public TrxPayrollRunEmployee? PayrollRunEmployee { get; set; }
        public MstPayrollComponent? PayrollComponent { get; set; }
        public ApplicationUser? PaidByUser { get; set; }
        public ApplicationUser? PostedByUser { get; set; }

    }
}
