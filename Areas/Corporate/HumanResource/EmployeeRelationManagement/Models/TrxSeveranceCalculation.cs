using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models
{
    [Table("trx_severance_calculations", Schema = "public")]
    public class TrxSeveranceCalculation : IdentityModel
    {
        [Key]
        [Column("calculation_id")]
        public Guid CalculationId { get; set; } = Guid.NewGuid();

        [NotMapped]
        public Guid Id
        {
            get => CalculationId;
            set => CalculationId = value;
        }

        [Required]
        [Column("separation_id")]
        public Guid SeparationId { get; set; }

        [Column("base_monthly_wage", TypeName = "numeric(14,2)")]
        public decimal BaseMonthlyWage { get; set; }

        [Column("service_years")]
        public int ServiceYears { get; set; }

        [Column("service_months")]
        public int ServiceMonths { get; set; }

        [Column("severance_pay_amount", TypeName = "numeric(14,2)")]
        public decimal SeverancePayAmount { get; set; } // Uang Pesangon (UP)

        [Column("service_award_amount", TypeName = "numeric(14,2)")]
        public decimal ServiceAwardAmount { get; set; } // Uang Penghargaan Masa Kerja (UPMK)

        [Column("rights_compensation_amount", TypeName = "numeric(14,2)")]
        public decimal RightsCompensationAmount { get; set; } // Uang Penggantian Hak (UPH)

        [Column("separation_allowance_amount", TypeName = "numeric(14,2)")]
        public decimal SeparationAllowanceAmount { get; set; } // Uang Pisah

        [Column("total_severance_gross", TypeName = "numeric(14,2)")]
        public decimal TotalSeveranceGross { get; set; }

        [Column("tax_deduction", TypeName = "numeric(14,2)")]
        public decimal TaxDeduction { get; set; } // PPh 21 Final

        [Column("total_severance_net", TypeName = "numeric(14,2)")]
        public decimal TotalSeveranceNet { get; set; }

        [Required]
        [MaxLength(32)]
        [Column("status")]
        public string Status { get; set; } = "Draft"; // Draft, VerifiedByHr, ApprovedByFinanceDirector

        [Column("is_approved_by_finance_director")]
        public bool IsApprovedByFinanceDirector { get; set; } = false; // VR-EMP-23

        [Column("finance_approval_notes")]
        public string? FinanceApprovalNotes { get; set; }

        [Column("approved_at")]
        public DateTime? ApprovedAt { get; set; }
    }
}
