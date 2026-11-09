using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    /// <summary>
    /// Komponen pengembalian uang/restitusi pada penggajian bulanan (termasuk kompensasi koreksi salah potong masa lalu non-pajak) (BE-HRD-161, BE-HRD-169, HRD-DEC-131, PAY-B6).
    /// </summary>
    [Table("TrxPayrollRestitution", Schema = "public")]
    public class TrxPayrollRestitution : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string RestitutionNumber { get; set; } = string.Empty;

        [Required]
        public Guid EmployeeId { get; set; }

        public Guid? ClaimId { get; set; }

        [Required, MaxLength(50)]
        public string RestitutionType { get; set; } = "BenefitReimbursement";
        // 'BenefitReimbursement', 'SalaryAdjustmentCorrection' (HRD-DEC-131)

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public Guid? AffectedPayrollRunId { get; set; } // Slip lampau yang salah potong
        public Guid? AffectedPayslipId { get; set; }

        public Guid? TargetPayrollRunId { get; set; } // Siklus penggajian berjalan tempat uang dikembalikan

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Pending";
        // 'Pending', 'Approved', 'InjectedToPayroll', 'Paid', 'Cancelled'

        public string? Justification { get; set; }

        public bool IsTaxable { get; set; } = false; // Restitusi & koreksi salah potong berstatus non-pajak PPh 21 (HRD-ASM-33, DED-B5)

        public MstEmployee? Employee { get; set; }
        public TrxEmployeeRestitutionClaim? Claim { get; set; }
        public TrxPayrollRun? AffectedPayrollRun { get; set; }
        public TrxPayrollPayslip? AffectedPayslip { get; set; }
        public TrxPayrollRun? TargetPayrollRun { get; set; }
    }
}
