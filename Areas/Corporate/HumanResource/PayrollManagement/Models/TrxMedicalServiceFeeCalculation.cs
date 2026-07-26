using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    [Table("TrxMedicalServiceFeeCalculation", Schema = "public")]
    public class TrxMedicalServiceFeeCalculation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? PayrollPeriodId { get; set; }
        public Guid? PayrollRunId { get; set; }
        public Guid? PayrollRunEmployeeId { get; set; }
        public Guid? PayrollComponentId { get; set; }

        [Required, MaxLength(50)]
        public string CalculationNumber { get; set; } = string.Empty;

        public DateOnly ServicePeriodStartDate { get; set; }
        public DateOnly ServicePeriodEndDate { get; set; }

        [Required, MaxLength(30)]
        public string CalculationStatus { get; set; } = "Draft";
        // Draft, Calculated, Verified, Approved, PostedToPayroll, Paid, Cancelled.

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal GrossServiceAmount { get; set; } = 0m;
        public decimal FeePercentage { get; set; } = 0m;
        public decimal GrossFeeAmount { get; set; } = 0m;
        public decimal DeductionAmount { get; set; } = 0m;
        public decimal TaxAmount { get; set; } = 0m;
        public decimal NetFeeAmount { get; set; } = 0m;
        public decimal PaidAmount { get; set; } = 0m;

        public string? SourceSummaryJson { get; set; }
        public string? CalculationDetailJson { get; set; }

        public DateTime? CalculatedAt { get; set; }
        public Guid? CalculatedByUserId { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? PostedToPayrollAt { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public MstDoctor? Doctor { get; set; }
        public MstPayrollPeriod? PayrollPeriod { get; set; }
        public TrxPayrollRun? PayrollRun { get; set; }
        public TrxPayrollRunEmployee? PayrollRunEmployee { get; set; }
        public MstPayrollComponent? PayrollComponent { get; set; }
        public ApplicationUser? CalculatedByUser { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }

        public ICollection<TrxMedicalServiceFeePayment> Payments { get; set; }
            = new List<TrxMedicalServiceFeePayment>();
    }
}
