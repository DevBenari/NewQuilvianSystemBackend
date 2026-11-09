using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    /// <summary>
    /// Rincian eksekusi pemotongan gaji per periode penggajian dengan hard-cap 50% dan alokasi prioritas (BE-HRD-161, BE-HRD-164, BE-HRD-165, DED-B1, DED-B2, DED-B3).
    /// </summary>
    [Table("TrxPayrollDeductionItem", Schema = "public")]
    public class TrxPayrollDeductionItem : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PayrollRunId { get; set; }

        [Required]
        public Guid DeductionScheduleId { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        [Required, MaxLength(100)]
        public string DeductionCategory { get; set; } = string.Empty;

        public int PriorityLevel { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ScheduledAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ActualDeductedAmount { get; set; } = 0.00m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DeferredAmount { get; set; } = 0.00m; // Nilai ditunda karena mentok batas 50%

        [Required, MaxLength(50)]
        public string ExecutionStatus { get; set; } = "Pending";
        // 'Pending', 'DeductedInFull', 'PartiallyDeducted', 'DeferredDueToSalaryCap', 'Cancelled'

        public string? DeferralReason { get; set; }

        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

        public TrxPayrollRun? PayrollRun { get; set; }
        public TrxPayrollDeductionSchedule? DeductionSchedule { get; set; }
        public MstEmployee? Employee { get; set; }
    }
}
